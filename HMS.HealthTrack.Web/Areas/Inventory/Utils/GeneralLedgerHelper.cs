using HMS.HealthTrack.Web.Areas.Inventory.Models.ProductImport;
using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Utils
{
   public static class GeneralLedgerHelper
   {
      public static string GetGeneralLedgerCode(IGeneralLedgerRepository repository, IGeneralLedgerTierRepository tierRepository, IPropertyProvider propertyProvider, int ledgerId, int ledgerType)
      {
         var levelCount = tierRepository.GetTierTypeCount(ledgerType);
         var maxLevel = tierRepository.GetMaxLevel(ledgerType);
         var startDepth = maxLevel - levelCount;
         var delimiter = propertyProvider.GlcSectionDelimiter;

         // Find the root of the tree which contains the LedgerId
         var absoluteRoot = repository.FindSubtreeRoot(ledgerId);

         var ledgerParents = repository.FindAllClosures().Where(glc => glc.ChildId == ledgerId).ToList();
         var ledgerParentIds = ledgerParents.Select(p => p.ParentId);

         // Find the root of the subtree which contains the LedgerId and is of the specified ledger type
         var subTreeRoot = repository.FindAllClosures().Single(glc => glc.ParentId == absoluteRoot && glc.Depth == startDepth
            && ledgerParentIds.Contains(glc.ChildId)).ChildId;  // where one of child's children is the ledgerId 

         var ledgerCode = ConstructLedgerCode(repository, tierRepository, ledgerType, levelCount, ledgerId, delimiter, subTreeRoot);
         return ledgerCode;
      }

      public static string ConstructLedgerCode(IGeneralLedgerRepository repository, IGeneralLedgerTierRepository tierRepository, int ledgerType, int segmentCount, int ledgerId, string delimiter, int rootId)
      {
         var parentTree = repository.FindAllClosures().Where(c => c.ChildId == ledgerId).ToList();
         var rootDepthFromChild = parentTree.Single(t => t.ParentId == rootId).Depth;
         var parentSubTree = parentTree.Where(t => t.Depth <= rootDepthFromChild).OrderByDescending(t => t.Depth).ToList();
         var parentSubTreeSize = parentSubTree.Count();
         if (parentSubTreeSize > segmentCount)
         {
            throw new Exception("Unable to construct general ledger code. Number of parents of the ledger too great for number of segments in general ledger code");
         }
         var code = new string[segmentCount];
         for (var ii = 0; ii < segmentCount; ii++)
         {
            if (ii < parentSubTreeSize)
            {
               var closure = parentSubTree[ii];
               code[ii] = closure.GeneralLedgerParent.Code;
            }
            else
            {
               var rootDepth = repository.FindAllClosures().Where(c => c.ChildId == rootId).Max(c => c.Depth);
               code[ii] = GetDefaultLedgerString(tierRepository, ledgerType, rootDepth + ii);
            }
         }
         return string.Join(delimiter, code);
      }

      private static string GetDefaultLedgerString(IGeneralLedgerTierRepository tierRepository, int ledgerType, int depth)
      {
         var tier = depth + 1; // because depth is zero based
         var setting = tierRepository.GetGlcSetting(ledgerType, tier);
         return string.Join("", Enumerable.Repeat(setting.FillCharacter, setting.Length));
      }

      public static int? GetGeneralLedgerId(IGeneralLedgerRepository repository,
         IGeneralLedgerTierRepository tierRepository, IPropertyProvider propertyProvider,
         IList<ProductImportGeneralLedgerCode> codeTierPairs, int ledgerType)
      {
         if (!codeTierPairs.Any())
            return null;

         // at least one code/tier pair
         var importTiers = codeTierPairs.Select(ic => ic.TierId);
         var tiers = tierRepository.FindAll().Where(t => importTiers.Contains(t.TierId)).OrderByDescending(t => t.Tier).ToList();

         var bottomTier = tiers.First();
         var ledgers = repository.FindAll();
         var codeTierPair = codeTierPairs.Single(ic => ic.TierId == bottomTier.TierId);
         ledgers = ledgers.Where(gl => gl.Code == codeTierPair.Code && gl.TierId == codeTierPair.TierId && gl.GeneralLedgerTier.LedgerType == ledgerType);

         // one ledger specified but many exist with code and tier - requires user intervention
         if (codeTierPairs.Count() == 1 && ledgers.Count() != 1)
         {
            return null;
         }

         // multiple codes specified, the bottom most is a match to a single ledger
         if (ledgers.Count() == 1)
         {
            var ledgerId = ledgers.Single().LedgerId;
            // this must be the corresponding general ledger, validate the tree is correct
            var tree = repository.FindAllClosures().Where(c => c.ChildId == ledgerId).OrderByDescending(c => c.Depth).ToList();
            if (ValidGeneralLedgerTree(repository, tree, tiers, codeTierPairs, ledgerType))
               return ledgerId;
            return null; // invalid tree
         }

         // multiple ledger codes specified, prepare to validate each
         var ledgerTrees = new Dictionary<int, List<GeneralLedgerClosure>>();
         foreach (var ledger in ledgers)
         {
            var tree = repository.FindAllClosures().Where(c => c.ChildId == ledger.LedgerId).OrderByDescending(c => c.Depth).ToList();
            ledgerTrees.Add(ledger.LedgerId, tree);
         }

         var validLedgers = new List<int>();
         // validate each ledger tree
         foreach (var ledgerTree in ledgerTrees)
         {
            var validTree = ValidGeneralLedgerTree(repository, ledgerTree.Value, tiers, codeTierPairs, ledgerType);
            if (validTree)
            {
               validLedgers.Add(ledgerTree.Key);
            }
         }

         if (validLedgers.Count() == 1)
         {
            return validLedgers.Single();
         }
         return null; // many possible ledgers, requires user intervention
      }

      public static List<GeneralLedgerTierCodeModel> DeconstructGeneralLedgerCode(IGeneralLedgerRepository repository, IGeneralLedgerTierRepository tierRepository, int ledgerId, int ledgerType)
      {
         var generalLedgerParentsTree = repository.FindAllClosures().Where(c => c.ChildId == ledgerId).Select(c => c.GeneralLedgerParent);

         var tiers = (from t in tierRepository.FindAll()
                      join p in generalLedgerParentsTree on t.TierId equals p.TierId into gp
                      from p in gp.DefaultIfEmpty()
                      where t.LedgerType == ledgerType

                      select new GeneralLedgerTierCodeModel
                      {
                         Code = p != null ? p.Code : string.Empty,
                         Name = t.Name,
                         LedgerId = p.LedgerId.ToString(),
                         Tier = t.Tier,
                         TierId = t.TierId
                      }).OrderBy(m => m.Tier).ToList();

         return tiers;
      }

      /// <summary>
      /// Iterates over the general ledger heirachy in reverse order, checking if each ledger in the tree corresponds to the corresponding entry in the 
      /// user input general ledger codes.
      /// </summary>
      /// <param name="repository"></param>
      /// <param name="ledgerTree"></param>
      /// <param name="orderedTiers"></param>
      /// <param name="codesToImport"></param>
      /// <returns></returns>
      private static bool ValidGeneralLedgerTree(IGeneralLedgerRepository repository, List<GeneralLedgerClosure> ledgerTree, List<GeneralLedgerTier> orderedTiers, IList<ProductImportGeneralLedgerCode> codesToImport, int ledgerType)
      {
         var closures = ledgerTree;

         // iterate over closures using tiers
         for (int index = 1; index < orderedTiers.Count(); index++)
         {
            // we know that the initial ledger is valid
            var closure = closures.Single(c => c.Depth == index); // start at the initial depth by subtracting zero
            var nextCodeToImport = codesToImport.Single(ic => ic.TierId == orderedTiers[index].TierId); // the user specified code corresponding to the next highest tier
            var ledger =
               repository.FindAll().SingleOrDefault(c => c.LedgerId == closure.ParentId && c.Code == nextCodeToImport.Code &&
                                                         c.TierId == nextCodeToImport.TierId && c.GeneralLedgerTier.LedgerType == ledgerType); // find the parent ledger
            if (ledger == null)
            {
               return false; // doesnt have matching Code/TierId combo, not a valid tree
            }
            // continue interation because the parent has the correct code and tier
         }
         return true;
      }
   }

   public class GeneralLedgerExportHelper
   {
      private readonly IGeneralLedgerRepository _generalLedgerRepository;
      private readonly IGeneralLedgerTierRepository _generalLedgerTierRepository;
      private readonly Dictionary<int, List<string>> _ledgerCodes;
      private readonly int _ledgerType;


      public GeneralLedgerExportHelper(IGeneralLedgerTierRepository generalLedgerTierRepository, IGeneralLedgerRepository generalLedgerRepository, int ledgerType)
      {
         _generalLedgerTierRepository = generalLedgerTierRepository;
         _generalLedgerRepository = generalLedgerRepository;
         _ledgerType = ledgerType;
         _ledgerCodes = new Dictionary<int, List<string>>();
      }

      public List<string> GetLedgerCodes(int ledgerId)
      {

         if (_ledgerCodes.ContainsKey(ledgerId))
         {
            return _ledgerCodes[ledgerId];
         }

         var absoluteRoot = _generalLedgerRepository.FindSubtreeRoot(ledgerId);
         var levelCount = _generalLedgerTierRepository.GetTierTypeCount(_ledgerType);
         var maxLevel = _generalLedgerTierRepository.GetMaxLevel(_ledgerType);
         var startDepth = maxLevel - levelCount;

         var ledgerParents = _generalLedgerRepository.FindAllClosures().Where(glc => glc.ChildId == ledgerId).ToList();
         var ledgerParentIds = ledgerParents.Select(p => p.ParentId);

         // Find the root of the subtree of which the ledger is a child and of which the subtree belongs to the specified ledger type
         var subTreeRoot = ledgerParents.Single(glc => glc.ParentId == absoluteRoot && glc.Depth == startDepth
            && ledgerParentIds.Contains(glc.ChildId)).ChildId;  // where one of child's children is the ledgerId 

         // Find the depth of the SubTree root in the tree that is all parents of the ledger
         var rootDepthFromChild = ledgerParents.Single(t => t.ParentId == subTreeRoot).Depth;
         // Find all the ledgers in the subtree starting from the subtree root
         var parentSubTree = ledgerParents.Where(t => t.Depth <= rootDepthFromChild).OrderByDescending(t => t.Depth).ToList();
         var parentSubTreeSize = parentSubTree.Count();
         // The tree should not be greater than the number of tiers
         if (parentSubTreeSize > levelCount)
         {
            // Something has gone wrong
            throw new Exception("Unable to fetch general ledger codes. Number of parents of the ledger too great for number of segments in general ledger code");
         }
         var codes = new List<string>();
         for (var ii = 0; ii < levelCount; ii++)
         {
            if (ii < parentSubTreeSize) // guard against null reference
            {
               var closure = parentSubTree[ii];
               codes.Add(closure.GeneralLedgerParent.Code);
            }
         }

         _ledgerCodes.Add(ledgerId, codes);

         return codes;
      }


   }
}