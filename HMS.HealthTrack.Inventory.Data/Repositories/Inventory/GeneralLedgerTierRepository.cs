using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class GeneralLedgerTierRepository : IGeneralLedgerTierRepository
   {
      private readonly IDbContextInventoryContext _context;

      public GeneralLedgerTierRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public int GetMaxDepth()
      {
         return _context.GeneralLedgerTiers.Max(glt => glt.Tier) - 1;
      }

      public int GetMaxDepth(int ledgerType)
      {
         return _context.GeneralLedgerTiers.Where(glt => glt.LedgerType == ledgerType).Max(glt => glt.Tier) - 1;
      }

      public int GetMaxLevel(int ledgerType)
      {
         return _context.GeneralLedgerTiers.Where(glt => glt.LedgerType == ledgerType).Max(glt => glt.Tier);
      }

      public Dictionary<int, int> GetTiersForLevel(int tierType)
      {
         return FindAll().Where(glt => glt.LedgerType == tierType).ToDictionary(glt => glt.Tier, glt => glt.TierId);
      }

      public int? GetNextTierId(int ledgerType, int? currentTier)
      {
         GeneralLedgerTier tier;
         if (currentTier.HasValue)
            tier = _context.GeneralLedgerTiers.SingleOrDefault(
               glt => glt.LedgerType == ledgerType && glt.Tier == currentTier + 1);
         else
            tier = _context.GeneralLedgerTiers.SingleOrDefault(
               glt => glt.LedgerType == ledgerType && glt.Tier == 1);

         return tier != null ? tier.TierId : (int?) null;
      }

      public IQueryable<GeneralLedgerTier> FindAll()
      {
         return _context.GeneralLedgerTiers.Where(glt => !glt.DeletedOn.HasValue);
      }

      public int GetTierTypeCount(int ledgerType)
      {
         return _context.GeneralLedgerTiers.Count(glt => glt.LedgerType == ledgerType);
      }

      public GeneralLedgerTier Find(int tierId)
      {
         return _context.GeneralLedgerTiers.SingleOrDefault(glt => glt.TierId == tierId);
      }

      public GlcSetting GetGlcSetting(int ledgerType, int tierNumber)
      {
         return
            _context.GeneralLedgerTiers.Include(glt => glt.GlcSetting)
               .Single(glt => glt.LedgerType == ledgerType && glt.Tier == tierNumber)
               .GlcSetting;
      }

      /// <summary>
      /// </summary>
      /// <param name="ledgerType"></param>
      /// <returns>
      ///    <TierId, TierName + Suffix>
      /// </returns>
      public Dictionary<int, string> GetSegmentNamesForExportImport(int ledgerType)
      {
         return FindAll()
            .Where(t => t.LedgerType == ledgerType)
            .OrderBy(t => t.Tier)
            .ToDictionary(t => t.TierId, t => t.Name + InventoryConstants.GeneralLedgerTierNameExportSuffix);
      }
   }

   public interface IGeneralLedgerTierRepository
   {
      int GetMaxDepth();
      int GetMaxDepth(int ledgerType);
      IQueryable<GeneralLedgerTier> FindAll();
      int GetTierTypeCount(int ledgerType);
      GeneralLedgerTier Find(int tierId);
      GlcSetting GetGlcSetting(int ledgerType, int tierNumber);
      int GetMaxLevel(int ledgerType);
      Dictionary<int, int> GetTiersForLevel(int tierType);
      int? GetNextTierId(int ledgerType, int? currentTier);
      Dictionary<int, string> GetSegmentNamesForExportImport(int ledgerType);
   }
}