using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class GeneralLedgerUnitOfWork : IGeneralLedgerUnitOfWork
   {
      private readonly IDbContextInventoryContext _context;

      private readonly ICustomLogger _logger;

      public GeneralLedgerUnitOfWork(IDbContextInventoryContext context, ICustomLogger logger, IPropertyProvider propertyProvider)
      {
         _context = context;
         _logger = logger;
         PropertyProvider = propertyProvider;

         GeneralLedgerTierRepository = new GeneralLedgerTierRepository(_context);
         GeneralLedgerRepository = new GeneralLedgerRepository(_context);
      }

      public IGeneralLedgerTierRepository GeneralLedgerTierRepository { get; }

      public IGeneralLedgerRepository GeneralLedgerRepository { get; }

      public IPropertyProvider PropertyProvider { get; }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public void CreateLedger(GeneralLedger ledger, int? parentId, string username, int ledgerType)
      {
         var nextTierId = NextTier(parentId, ledgerType);
         if (nextTierId == null) throw new Exception("Cannot create ledger for tier which does not exist");
         ledger.TierId = nextTierId.Value;

         GeneralLedgerRepository.Add(ledger, username);
         Commit();
         var closures = new List<GeneralLedgerClosure>();

         if (parentId.HasValue)
         {
            // add closure entity for every parent of the parentId
            var parentClosures = GeneralLedgerRepository.GetParentClosures(parentId.Value);
            closures.AddRange(parentClosures.Select(closure => new GeneralLedgerClosure
            {
               ParentId = closure.ParentId,
               Depth = closure.Depth + 1,
               ChildId = ledger.LedgerId
            }));
         }

         // add self reference
         closures.Add(new GeneralLedgerClosure
         {
            ParentId = ledger.LedgerId,
            Depth = 0,
            ChildId = ledger.LedgerId
         });

         foreach (var closure in closures) _context.GeneralLedgerClosures.Add(closure);
      }

      /// <summary>
      ///    A Ledger can have children if it is not associated with the bottom most tier
      /// </summary>
      /// <param name="ledgerId"></param>
      /// <param name="ledgerType"></param>
      /// <returns></returns>
      public bool CanHaveChildren(int ledgerId, int ledgerType)
      {
         return NextTier(ledgerId, ledgerType) != null;
      }

      /// <summary>
      ///    Finds ledgers which are suitable potential parents for the specified ledger.
      ///    Suitable parent: A ledger that is of the same type, is not at the maximum depth as specified by the related tier
      ///    type,
      ///    and is not a child of the ledger specified.
      /// </summary>
      /// <param name="ledgerId"></param>
      /// <param name="ledgerType"></param>
      /// <returns></returns>
      public IQueryable<GeneralLedger> FindParents(int? ledgerId, int ledgerType)
      {
         var leaves = FindLeaves(ledgerType);
         var withoutLeaves = GeneralLedgerRepository.FindAll().Where(gl => gl.GeneralLedgerTier.LedgerType == ledgerType && !leaves.Contains(gl.LedgerId));
         if (ledgerId.HasValue)
         {
            var children = _context.GeneralLedgerClosures.Where(glc => glc.ParentId == ledgerId.Value).Select(glc => glc.ChildId);
            return withoutLeaves.Where(glc => !children.Contains(glc.LedgerId));
         }

         return withoutLeaves;
      }

      public bool UpdateLedger(GeneralLedger ledger, int? parentId, string username, int ledgerType)
      {
         var existing = GeneralLedgerRepository.Find(ledger.LedgerId);
         if (existing == null) return false;

         var parentClosures = GeneralLedgerRepository.GetParentClosures(ledger.LedgerId);
         var parent = parentClosures.SingleOrDefault(c => c.Depth == 1);

         ledger.LastModifiedBy = username;
         ledger.LastModifiedOn = DateTime.Now;
         _context.Entry(existing).CurrentValues.SetValues(ledger);
         _context.Entry(existing).Property(p => p.TierId).IsModified = false;

         if (!parentId.HasValue || parent == null || parentId.Value != parent.ParentId)
         {
            if (!ValidMove(ledger, parentId, parent)) return false;
            MoveTree(ledger.LedgerId, parentId);
            UpdateLedgerTiers(existing, parentId, ledgerType);
            _context.Entry(existing).Property(p => p.TierId).IsModified = true;
            // if no parent and no parent id then it was a root and is still a root, no valiation needed
         }

         _context.Entry(existing).Property(p => p.CreatedOn).IsModified = false;
         _context.Entry(existing).Property(p => p.CreatedBy).IsModified = false;

         return true;
      }

      public IQueryable<GeneralLedgerClosure> FindAllClosures()
      {
         return _context.GeneralLedgerClosures;
      }

      public int? NextTier(int? ledgerId, int ledgerType)
      {
         GeneralLedger ledger = null;
         if (ledgerId.HasValue) ledger = GeneralLedgerRepository.Find(ledgerId.Value);
         return ledger == null
            ? GeneralLedgerTierRepository.GetNextTierId(ledgerType, null)
            : GeneralLedgerTierRepository.GetNextTierId(ledgerType, ledger.GeneralLedgerTier.Tier);
      }

      public GeneralLedger FindRoot(int ledgerId)
      {
         var root =
            _context.GeneralLedgerClosures.Where(glc => glc.ChildId == ledgerId)
               .OrderByDescending(glc => glc.Depth)
               .First().GeneralLedgerParent;
         return root;
      }

      /// <summary>
      ///    Finds nodes which cannot have children due to the depth restriction placed on the tier by it's type
      /// </summary>
      /// <param name="ledgerType"></param>
      /// <returns></returns>
      private IQueryable<int> FindLeaves(int ledgerType)
      {
         var maxDepth = GeneralLedgerTierRepository.GetMaxDepth(ledgerType);
         var leaveIds = _context.GeneralLedgerClosures.Where(glc => glc.Depth == maxDepth).Select(glc => glc.ChildId);
         return leaveIds;
      }

      private void UpdateLedgerTiers(GeneralLedger ledger, int? parentId, int ledgerType)
      {
         var ledgerTiers = GeneralLedgerTierRepository.GetTiersForLevel(ledgerType);
         var tierLevel = parentId.HasValue ? GeneralLedgerRepository.Find(parentId.Value).GeneralLedgerTier.Tier + 1 : 1;

         ledger.TierId = GetTierIdForLevel(ledgerTiers, tierLevel, ledgerType);

         UpdateChildrenLedgerTiers(ledger.GeneralLedgerChildren.Where(glc => glc.Depth > 0).Select(glc => glc.GeneralLedgerChild), tierLevel, ledgerTiers, ledgerType);
      }

      private int GetTierIdForLevel(Dictionary<int, int> ledgerTiers, int tierLevel, int ledgerType)
      {
         int tierId;
         if (!ledgerTiers.TryGetValue(tierLevel, out tierId))
         {
            _logger.Error("Could not update ledger with the new tier level {TierLevel} as it does not exist for the type {LedgerType}", tierLevel, ledgerType);
            throw new Exception(string.Format("Could not update ledger with new tier. Tier level {0} does not exist for ledger type {1}.", tierLevel, ledgerType));
         }

         return tierId;
      }

      private void UpdateChildrenLedgerTiers(IEnumerable<GeneralLedger> children, int tierLevel, Dictionary<int, int> ledgerTiers, int ledgerType)
      {
         if (!children.Any()) return;
         tierLevel += 1;

         var tierId = GetTierIdForLevel(ledgerTiers, tierLevel, ledgerType);

         foreach (var ledger in children)
         {
            ledger.TierId = tierId;
            UpdateChildrenLedgerTiers(
               ledger.GeneralLedgerChildren.Where(glc => glc.Depth > 0).Select(glc => glc.GeneralLedgerChild), tierLevel,
               ledgerTiers, ledgerType);
         }
      }

      private bool ValidMove(GeneralLedger ledger, int? parentId, GeneralLedgerClosure parent)
      {
         //validation
         var maxDepth = GeneralLedgerTierRepository.GetMaxDepth();
         var subtreeSize = GeneralLedgerRepository.SubTreeSize(ledger.LedgerId);

         if (parentId.HasValue)
         {
            var newParentDepth = GeneralLedgerRepository.GetLedgerDepth(parentId.Value);

            if (parent == null || parentId.Value != parent.ParentId)
            {
               var valid = newParentDepth + subtreeSize <= maxDepth;
               if (!valid) return false;
            }
         }

         return true;
      }

      private void MoveTree(int ledgerId, int? destinationLedger)
      {
         // remove tree from graph
         var treeLedger = _context.GeneralLedgerClosures.Where(glc => glc.ParentId == ledgerId).ToList();
         var treeLedgerIds = treeLedger.Select(glc => glc.ChildId).ToList();

         //remove all where child is part of the subtree but the parent is not
         var toRemove =
            _context.GeneralLedgerClosures.Where(
               glc => treeLedgerIds.Contains(glc.ChildId) && !treeLedgerIds.Contains(glc.ParentId));
         foreach (var closure in toRemove) _context.GeneralLedgerClosures.Remove(closure);

         if (!destinationLedger.HasValue) return; // moved to root

         //reconnect tree to graph at node
         var parentTree = _context.GeneralLedgerClosures.Where(glc => glc.ChildId == destinationLedger.Value);
         var toInsert = new List<GeneralLedgerClosure>();
         foreach (var closure in parentTree)
         foreach (var node in treeLedger)
            toInsert.Add(new GeneralLedgerClosure
            {
               ParentId = closure.ParentId,
               ChildId = node.ChildId,
               Depth = closure.Depth + node.Depth + 1
            });

         foreach (var closure in toInsert) _context.GeneralLedgerClosures.Add(closure);
      }
   }

   public interface IGeneralLedgerUnitOfWork
   {
      IGeneralLedgerTierRepository GeneralLedgerTierRepository { get; }
      IGeneralLedgerRepository GeneralLedgerRepository { get; }
      IPropertyProvider PropertyProvider { get; }
      void Commit();
      void CreateLedger(GeneralLedger ledger, int? parentId, string username, int ledgerType);
      bool CanHaveChildren(int ledgerId, int ledgerType);
      IQueryable<GeneralLedger> FindParents(int? ledgerId, int ledgerType);
      bool UpdateLedger(GeneralLedger ledger, int? parentId, string username, int ledgerType);
      IQueryable<GeneralLedgerClosure> FindAllClosures();
   }
}