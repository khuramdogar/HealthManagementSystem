using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class ProductUnitOfWork : IProductUnitOfWork
   {
      private readonly StockAdjustmentHelper _adjustmentHelper;
      private readonly IDbContextInventoryContext _context;

      public ProductUnitOfWork(IDbContextInventoryContext context, IPropertyProvider propertyProvider, ICustomLogger logger)
      {
         _context = context;
         HealthTrackConsumptionRepository = new HealthTrackConsumptionRepository(context);
         ProductRepository = new ProductRepository(context, propertyProvider);
         CompanyRepo = new CompanyRepository(context);
         CategoryRepo = new CategoryRepository(context);
         ProductPriceRepository = new ProductPriceRepository(context);
         ExternalProductMappingRepository = new ExternalProductMappingRepository(context, logger);
         MedicareProductsRepository = new MedicareProductsRepository(context);
         StockRepository = new StockRepository(context);
         StockAdjustmentRepository = new StockAdjustmentRepository(context);
         ProductImportRepository = new ProductImportRepository(context);
         ProductImportDataRepository = new ProductImportDataRepository(context);
         StockLocationRepository = new StockLocationRepository(context);
         OrderRepository = new OrderRepository(context);
         _adjustmentHelper = new StockAdjustmentHelper(StockRepository, StockAdjustmentRepository);
      }

      public IProductRepository ProductRepository { get; }
      public ICompanyRepository CompanyRepo { get; }
      public ICategoryRepository CategoryRepo { get; }
      public IProductPriceRepository ProductPriceRepository { get; }
      public IHealthTrackConsumptionRepository HealthTrackConsumptionRepository { get; }
      public IExternalProductMappingRepository ExternalProductMappingRepository { get; }
      public IMedicareProductsRepository MedicareProductsRepository { get; }
      public IStockRepository StockRepository { get; }
      public IStockAdjustmentRepository StockAdjustmentRepository { get; }
      public IProductImportRepository ProductImportRepository { get; }
      public IProductImportDataRepository ProductImportDataRepository { get; }
      public IStockLocationRepository StockLocationRepository { get; }
      public IOrderRepository OrderRepository { get; }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public IQueryable<Product> FindUnclassified()
      {
         return ProductRepository.FindAll().Where(prod => !prod.ProductCategories.Any());
      }

      public void CreateInitialStock(Product product, int locationId, int quantity, string username)
      {
         var initialStockReason = StockAdjustmentRepository.GetSystemReason(InventoryConstants.StockAdjustmentReasons.InitialStock);
         var adjustment = StockAdjustmentHelper.CreateItemAdjustment(product.ProductId, locationId, quantity, true,
            AdjustmentSource.Web, username);

         adjustment.ReasonId = initialStockReason.StockAdjustmentReasonId;

         var newStockWithAdjustment = _adjustmentHelper.CreateNewStockWithPositiveAdjustment(adjustment, username);
         product.Stocks.Add(newStockWithAdjustment);
      }

      public void Update(Product entity, IEnumerable<string> selectedSettings, IEnumerable<int> selectedCategories, string username)
      {
         var existing = ProductRepository.Find(entity.ProductId);
         if (existing == null) return;

         existing.LastModifiedBy = username;

         HandleReorderSettingUpdate(entity, existing.AutoReorderSetting);

         HandleManageStockUpdate(existing, entity.ManageStock, username);

         ProductRepository.UpdateProductProperties(entity, selectedSettings, selectedCategories, existing);
      }

      public void HandleReorderSettingUpdate(Product productToUpdate, ReorderSettings currentSetting)
      {
         // change to one for one replacement - i.e. start recording and displaying replacements
         if (currentSetting != ReorderSettings.OneForOneReplace &&
             productToUpdate.AutoReorderSetting == ReorderSettings.OneForOneReplace)
            productToUpdate.ReplaceAfter = DateTime.Now;
      }

      public void HandleManageStockUpdate(Product existing, bool manageStock, string username)
      {
         if (existing.ManageStock && !manageStock)
            // deduct any available stock, marking it as written off
            WriteOffAvailableStockForProductPerLocation(existing, username);
      }

      public void WriteOffAvailableStockForProductPerLocation(Product product, string username)
      {
         var writeOffReason = StockAdjustmentRepository.GetSystemReason(InventoryConstants.StockAdjustmentReasons.StockManagementWriteOff);

         var stock = StockRepository.GetAvailableStock().Where(s => s.ProductId == product.ProductId);
         if (!stock.Any())
            return;

         var totalStock = stock.Sum(s => s.Quantity);

         var adjustment = StockAdjustmentHelper.CreateItemAdjustment(product.ProductId, null, totalStock, false,
            AdjustmentSource.System, username);
         adjustment.ReasonId = writeOffReason.StockAdjustmentReasonId;
         _adjustmentHelper.AdjustItem(adjustment, product, username);
      }

      public void IgnorePreviousConsumptionNotifications(int productId, DateTime ignoreBefore, string username)
      {
         var nonIgnoredConsumptions = HealthTrackConsumptionRepository.FindNonArchivedConsumptionNotifications();
         var healthTrackConsumptions = HealthTrackConsumptionRepository.FindHealthTrackConsumptions();
         var externalProductIds = ExternalProductMappingRepository.GetInventoryProductMapping(productId);

         var notificationsToIgnore = from nicnm in nonIgnoredConsumptions
            join htc in healthTrackConsumptions on nicnm.invUsed_ID equals htc.UsedId
            join ep in externalProductIds on htc.ProductId equals ep.ExternalProductId
            where htc.ConsumedOn < ignoreBefore
            select nicnm;

         foreach (var consumptionNotificationManagement in notificationsToIgnore) HealthTrackConsumptionRepository.ArchiveConsumptionNotification(consumptionNotificationManagement, username);
      }
   }

   public interface IProductUnitOfWork
   {
      IProductRepository ProductRepository { get; }
      ICompanyRepository CompanyRepo { get; }
      ICategoryRepository CategoryRepo { get; }
      IProductPriceRepository ProductPriceRepository { get; }
      IHealthTrackConsumptionRepository HealthTrackConsumptionRepository { get; }
      IExternalProductMappingRepository ExternalProductMappingRepository { get; }
      IMedicareProductsRepository MedicareProductsRepository { get; }
      IStockRepository StockRepository { get; }
      IStockAdjustmentRepository StockAdjustmentRepository { get; }
      IProductImportRepository ProductImportRepository { get; }
      IProductImportDataRepository ProductImportDataRepository { get; }
      IStockLocationRepository StockLocationRepository { get; }
      IOrderRepository OrderRepository { get; }
      void Commit();
      IQueryable<Product> FindUnclassified();
      void CreateInitialStock(Product product, int locationId, int quantity, string username);
      void Update(Product entity, IEnumerable<string> selectedSettings, IEnumerable<int> selectedCategories, string username);
      void IgnorePreviousConsumptionNotifications(int productId, DateTime ignoreBefore, string username);
      void WriteOffAvailableStockForProductPerLocation(Product product, string username);
      void HandleManageStockUpdate(Product existing, bool manageStock, string username);
      void HandleReorderSettingUpdate(Product productToUpdate, ReorderSettings currentSetting);
   }
}