using System;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class StockAdjustmentRepository : IStockAdjustmentRepository
   {
      private readonly IDbContextInventoryContext _inventoryContext;

      public StockAdjustmentRepository(IDbContextInventoryContext inventoryContext)
      {
         _inventoryContext = inventoryContext;
      }

      public IQueryable<DeductionRequiringPaymentClass> FindDeductionsRequiringPaymentClass()
      {
         return _inventoryContext.DeductionsRequiringPaymentClass;
      }

      public IQueryable<DeductionRequiringPaymentClass> FindDeductionsMissingRequiredPaymentClass()
      {
         return from c in FindDeductionsRequiringPaymentClass()
            where !c.PaymentClass.HasValue
            select c;
      }

      public StockAdjustment FindDeduction(long id)
      {
         return
            (from s in _inventoryContext.StockAdjustments where s.StockAdjustmentId == id && s.IsPositive == false select s).Include(
               s => s.StockAdjustmentStocks.Select(sas => sas.Stock.Product)).Include(s => s.StockAdjustmentStocks.Select(sas => sas.Stock.Product)).SingleOrDefault();
      }

      public StockAdjustment FindPositiveAdjustment(long id)
      {
         return
            (from s in _inventoryContext.StockAdjustments where s.StockAdjustmentId == id && s.IsPositive select s)
            .Include(
               s => s.StockAdjustmentStocks.Select(sas => sas.Stock.Product))
            .Include(s => s.StockAdjustmentStocks.Select(sas => sas.Stock.Product))
            .SingleOrDefault();
      }

      public void Commit()
      {
         _inventoryContext.ObjectContext.SaveChanges();
      }

      public IQueryable<StockAdjustment> FindDeduction(long[] adjustmentIds)
      {
         return _inventoryContext.StockAdjustments.Where(xx => adjustmentIds.Contains(xx.StockAdjustmentId));
      }

      public IQueryable<StockAdjustment> FindAllDeductions()
      {
         return _inventoryContext.StockAdjustments
            .Include(sd => sd.StockAdjustmentStocks.Select(sas => sas.Stock.Product))
            .Include(sd => sd.StockAdjustmentStocks.Select(sas => sas.Stock.StorageLocation))
            .Where(sd => sd.DeletedOn == null && sd.IsPositive == false);
      }

      public IQueryable<StockAdjustment> FindDeductions(int productId, int locationId)
      {
         return FindAdjustments(productId, locationId, false);
      }

      public void Add(StockAdjustment newEntity, string username)
      {
         newEntity.CreatedBy = username;
         newEntity.LastModifiedBy = username;
         newEntity.LastModifiedOn = DateTime.Now;
         _inventoryContext.StockAdjustments.Add(newEntity);
      }

      public void Remove(StockAdjustment entity)
      {
         entity.DeletedOn = DateTime.Now;
      }

      public IQueryable<StockAdjustment> GetStockDeductionHistory()
      {
         return FindAllDeductions().Where(xx => xx.IsPositive == false).Include(s => s.StockAdjustmentStocks.Select(sas => sas.Stock.Product));
      }

      public IQueryable<StockAdjustment> FindDeductionsMissingSerial()
      {
         var results = from sa in _inventoryContext.StockAdjustments
            let stocks = sa.StockAdjustmentStocks.Select(s => s.Stock)
            where sa.IsPositive == false
                  && stocks.Any(s => (s.SerialNumber == null || s.SerialNumber == string.Empty)
                                     && s.Product.UseCategorySettings
                     ? s.Product.ProductCategories.SelectMany(pc => pc.Category.StockSettings)
                        .Any(ss => ss.SettingId == InventoryConstants.StockSettings.RequiresSerialNumber)
                     : s.Product.ProductSettings.Any(
                        ps => ps.SettingId == InventoryConstants.StockSettings.RequiresSerialNumber))
            select sa;
         return results;
      }

      public IQueryable<StockAdjustment> FindDeductionsMissingLot()
      {
         var results = from sa in _inventoryContext.StockAdjustments
            let stocks = sa.StockAdjustmentStocks.Select(s => s.Stock)
            where sa.IsPositive == false
                  && stocks.Any(s => (s.BatchNumber == null || s.BatchNumber == string.Empty)
                                     && s.Product.UseCategorySettings
                     ? s.Product.ProductCategories.SelectMany(pc => pc.Category.StockSettings)
                        .Any(ss => ss.SettingId == InventoryConstants.StockSettings.RequiresBatchNumber)
                     : s.Product.ProductSettings.Any(
                        ps => ps.SettingId == InventoryConstants.StockSettings.RequiresBatchNumber))
            select sa;

         return results;
      }

      public IQueryable<StockAdjustment> FindDeductionsMissingPatients()
      {
         var results = from sa in _inventoryContext.StockAdjustments
            let stocks = sa.StockAdjustmentStocks.Select(s => s.Stock)
            where sa.IsPositive == false &&
                  stocks.Any(s => s.Product.UseCategorySettings
                     ? s.Product.ProductCategories.SelectMany(pc => pc.Category.StockSettings)
                        .Any(ss => ss.SettingId == InventoryConstants.StockSettings.RequiresPatientDetails)
                     : s.Product.ProductSettings.Any(
                        ps => ps.SettingId == InventoryConstants.StockSettings.RequiresPatientDetails))
                  && !sa.PatientId.HasValue
            select sa;
         return results;
      }

      public IQueryable<StockAdjustmentReason> GetReasons()
      {
         return _inventoryContext.StockAdjustmentReasons.Where(r => r.DeletedOn.HasValue == false);
      }

      public void CreateReason(StockAdjustmentReason stockAdjustmentReason)
      {
         stockAdjustmentReason.CreatedBy = stockAdjustmentReason.CreatedBy;
         stockAdjustmentReason.CreatedOn = DateTime.Now;
         stockAdjustmentReason.LastModifiedOn = stockAdjustmentReason.CreatedOn;
         stockAdjustmentReason.LastModifiedUser = stockAdjustmentReason.CreatedBy;
         _inventoryContext.StockAdjustmentReasons.Add(stockAdjustmentReason);
      }

      public void UpdateReason(StockAdjustmentReason stockAdjustmentReason)
      {
         var reason = _inventoryContext.StockAdjustmentReasons.SingleOrDefault(r => r.StockAdjustmentReasonId == stockAdjustmentReason.StockAdjustmentReasonId);
         if (reason == null) return;

         reason.Name = stockAdjustmentReason.Name;
         reason.Description = stockAdjustmentReason.Description;
         reason.Disabled = stockAdjustmentReason.Disabled;
         reason.LastModifiedOn = DateTime.Now;
         reason.LastModifiedUser = stockAdjustmentReason.LastModifiedUser;
      }

      public void DeleteReason(StockAdjustmentReason stockAdjustmentReason)
      {
         var reason = _inventoryContext.StockAdjustmentReasons.SingleOrDefault(r => r.StockAdjustmentReasonId == stockAdjustmentReason.StockAdjustmentReasonId);
         if (reason == null) return;

         reason.DeletedBy = stockAdjustmentReason.DeletedBy;
         reason.DeletedOn = DateTime.Now;
         reason.LastModifiedOn = reason.LastModifiedOn;
         reason.LastModifiedUser = reason.DeletedBy;
      }

      public bool ReasonNameExists(string name)
      {
         return GetReasons().Any(sar => sar.Name.ToUpper() == name.Trim().ToUpper());
      }

      public StockAdjustmentReason GetSystemReason(string reasonName)
      {
         return GetReasons().Single(sar => sar.IsSystemReason && sar.Name.ToUpper() == reasonName.ToUpper());
      }

      public IQueryable<StockAdjustment> FindAllPositiveAdjustments()
      {
         return _inventoryContext.StockAdjustments
            .Include(sa => sa.StockAdjustmentStocks.Select(sas => sas.Stock.Product))
            .Include(sa => sa.StockAdjustmentStocks.Select(sas => sas.Stock.StorageLocation))
            .Include(p => p.OrderItem.Order)
            .Where(sa => sa.DeletedOn == null && sa.IsPositive);
      }

      public IQueryable<StockAdjustment> FindPositiveAdjustments(int productId, int locationId)
      {
         return FindAdjustments(productId, locationId, true);
      }

      public IQueryable<StockAdjustment> GetStockReceipts()
      {
         var adjustments =
            FindAllPositiveAdjustments().Where(a => a.Source == AdjustmentSource.Web || a.Source == AdjustmentSource.Order);
         return adjustments;
      }

      public StockAdjustment CreateStockAdjustment(ItemAdjustment adjustment, string username)
      {
         var stockAdjustment = new StockAdjustment
         {
            AdjustedBy = username,
            AdjustedOn = DateTime.Now,
            ClinicalRecordId = adjustment.ClinicalRecordId,
            CreatedBy = username,
            CreatedOn = DateTime.Now,
            GeneralLedgerId = adjustment.LedgerId,
            IsPositive = adjustment.IsPositive,
            LastModifiedBy = username,
            LastModifiedOn = DateTime.Now,
            Note = adjustment.Note,
            PatientId = adjustment.PatientId,
            Quantity = adjustment.Quantity,
            Source = adjustment.Source,
            StockAdjustmentReasonId = adjustment.ReasonId
         };
         return stockAdjustment;
      }

      private IQueryable<StockAdjustment> FindAdjustments(int productId, int locationId, bool isPositive)
      {
         return from sa in _inventoryContext.StockAdjustments
               .Include(sa => sa.StockAdjustmentStocks.Select(sas => sas.Stock.Product))
               .Include(sa => sa.StockAdjustmentStocks.Select(sas => sas.Stock.StorageLocation))
            join sas in _inventoryContext.StockAdjustmentStocks on sa.StockAdjustmentId equals sas.StockAdjustmentId
            join s in _inventoryContext.Stocks on sas.StockId equals s.StockId
            where
               sa.DeletedOn == null && s.DeletedOn == null &&
               s.ProductId == productId && s.StoredAt == locationId &&
               sa.IsPositive == isPositive
            select sa;
      }
   }

   public interface IStockAdjustmentRepository
   {
      StockAdjustment FindDeduction(long id);
      StockAdjustment FindPositiveAdjustment(long id);
      void Commit();
      IQueryable<StockAdjustment> FindDeduction(long[] adjustmentIds);
      IQueryable<StockAdjustment> FindAllDeductions();
      void Add(StockAdjustment newEntity, string username);
      void Remove(StockAdjustment entity);
      IQueryable<DeductionRequiringPaymentClass> FindDeductionsRequiringPaymentClass();
      IQueryable<DeductionRequiringPaymentClass> FindDeductionsMissingRequiredPaymentClass();
      IQueryable<StockAdjustment> GetStockDeductionHistory();
      IQueryable<StockAdjustment> FindDeductionsMissingSerial();
      IQueryable<StockAdjustment> FindDeductionsMissingLot();
      IQueryable<StockAdjustment> FindDeductionsMissingPatients();

      IQueryable<StockAdjustmentReason> GetReasons();
      void CreateReason(StockAdjustmentReason stockAdjustmentReason);
      void UpdateReason(StockAdjustmentReason stockAdjustmentReason);
      void DeleteReason(StockAdjustmentReason stockAdjustmentReason);
      bool ReasonNameExists(string name);
      StockAdjustment CreateStockAdjustment(ItemAdjustment adjustment, string username);
      StockAdjustmentReason GetSystemReason(string reasonName);
      IQueryable<StockAdjustment> GetStockReceipts();
      IQueryable<StockAdjustment> FindAllPositiveAdjustments();
      IQueryable<StockAdjustment> FindPositiveAdjustments(int productId, int locationId);
      IQueryable<StockAdjustment> FindDeductions(int productId, int locationId);
   }
}