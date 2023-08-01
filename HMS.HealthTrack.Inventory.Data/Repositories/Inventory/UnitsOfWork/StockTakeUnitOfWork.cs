using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Serilog;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class StockTakeUnitOfWork : IStockTakeUnitOfWork
   {
      private readonly StockAdjustmentHelper _adjustmentHelper;
      private readonly IDbContextInventoryContext _context;
      private readonly ILogger _logger;

      public StockTakeUnitOfWork(IDbContextInventoryContext context, ICustomLogger logger, IPropertyProvider propertyProvider)
      {
         _context = context;
         StockTakeRepository = new StockTakeRepository(_context);
         StockRepository = new StockRepository(_context);
         _logger = logger;
         ProductRepository = new ProductRepository(context, propertyProvider);
         StockLocationRepository = new StockLocationRepository(context);
         PreferenceRepository = new PreferenceRepository(context);
         StockAdjustmentRepository = new StockAdjustmentRepository(context);
         _adjustmentHelper = new StockAdjustmentHelper(StockRepository, StockAdjustmentRepository);
      }

      public IStockAdjustmentRepository StockAdjustmentRepository { get; }

      public IProductRepository ProductRepository { get; }
      public IStockTakeRepository StockTakeRepository { get; }
      public IStockRepository StockRepository { get; }
      public IStockLocationRepository StockLocationRepository { get; }
      public IPreferenceRepository PreferenceRepository { get; }

      public void Commit()
      {
         var toSave = _context.StockTakes.Local.Where(st => st.StockTakeId == 0).ToList();
         _context.ObjectContext.SaveChanges();
         foreach (var stockTake in toSave.Where(stockTake => string.IsNullOrWhiteSpace(stockTake.Name))) stockTake.Name = "ST-" + stockTake.StockTakeId;
         _context.ObjectContext.SaveChanges();
      }

      public void ProcessStockTake(StockTake stockTakeToProcess)
      {
         if (stockTakeToProcess.LocationId < 1)
         {
            _logger.Warning("Could not process stock take {StockTakeId} with invalid location", stockTakeToProcess.StockTakeId);
            return;
         }

         //Process
         ProcessStockTakeItems(stockTakeToProcess, stockTakeToProcess.StockTakeItems.Where(sti => !sti.DeletedOn.HasValue), StockTakeType.Standard);

         //Success
         stockTakeToProcess.Status = StockTakeStatus.Complete;
      }

      public bool ProcessStockTake(int stockTakeId, string username)
      {
         var stockTakeToProcess = StockTakeRepository.Fetch(stockTakeId);

         var validStockTake = ValidateStockTake(stockTakeToProcess, stockTakeId);
         if (!validStockTake) return false;

         //Mark as being processed
         stockTakeToProcess.SubmittedBy = username;
         stockTakeToProcess.SubmittedOn = DateTime.Now;

         try
         {
            //Process
            ProcessStockTake(stockTakeToProcess);

            //Complete
            stockTakeToProcess.Status = StockTakeStatus.Complete;
            StockTakeRepository.Commit();
         }
         catch (Exception exception)
         {
            //Failed
            _logger.Error(exception, "Failed to process stock take {StockTakeId}", stockTakeId);
            return false;
         }

         return true;
      }

      private void ProcessStockTakeItems(StockTake stockTakeToProcess, IEnumerable<StockTakeItem> stockTakeItems, StockTakeType stockTakeType)
      {
         foreach (var stockTakeItem in stockTakeItems.Where(sti => !sti.DeletedOn.HasValue))
         {
            if (!stockTakeItem.StockLevel.HasValue)
            {
               _logger.Warning("Cannot process stock take item {StockTakeItemId} without stock level",
                  stockTakeItem.StockTakeItemId);
               stockTakeItem.Message = "No stock take level provided.";
               stockTakeItem.Status = StockTakeItemStatus.Error;
               return;
            }

            var availableStock = 0;
            var product = ProductRepository.Find(stockTakeItem.ProductId);
            if (product == null)
            {
               stockTakeItem.Message = "Could not find the associated product. Product may have been deleted.";
               stockTakeItem.Status = StockTakeItemStatus.Error;
               _logger.Warning("Failed to find product for stock take item {StockTakeItemId} with ProductId {ProductId}",
                  stockTakeItem.StockTakeItemId, stockTakeItem.ProductId);
               return;
            }

            if (stockTakeType == StockTakeType.Standard) availableStock = StockRepository.GetStockCount(stockTakeItem.ProductId, stockTakeToProcess.LocationId);

            var stockAtDate = CalculateStockLevelAtDateTime(stockTakeItem, availableStock, stockTakeToProcess);
            // if stock at date is 0, it could have been negative but that doesn't matter as negative stock should not be factored into the stock take calculation

            //Calculate adjustment
            stockTakeItem.Adjustment = stockTakeItem.StockLevel - stockAtDate;
            stockTakeItem.PreviousStockLevel = stockAtDate;
            stockTakeItem.NewStockLevel = stockTakeItem.StockLevel;

            //Check there is an adjustment to do
            if (stockTakeItem.Adjustment != 0) AdjustStock(stockTakeItem, stockTakeToProcess.SubmittedBy, stockTakeToProcess.LocationId);

            if (!product.ManageStock) UpdateProductToManageStock(product, stockTakeItem.NewStockLevel.Value, stockTakeToProcess.SubmittedBy);

            stockTakeItem.ProcessedOn = DateTime.Now;
            stockTakeItem.Status = StockTakeItemStatus.Complete;
         }
      }

      private void UpdateProductToManageStock(Product product, int stockLevel, string username)
      {
         product.AutoReorderSetting = ReorderSettings.SpecifyLevels;
         product.ManageStock = true;
         product.LastModifiedBy = username;
         product.LastModifiedOn = DateTime.Now;
         product.ReorderThreshold = stockLevel - 1;
         product.TargetStockLevel = stockLevel;
      }

      private int CalculateStockLevelAtDateTime(StockTakeItem item, int currentAvailableStock, StockTake stockTake)
      {
         var deductionsSince = StockAdjustmentRepository.FindDeductions(item.ProductId, stockTake.LocationId).Where(sa => sa.AdjustedOn > stockTake.StockTakeDate);
         var quantityDeductedSince = deductionsSince.Any() ? deductionsSince.Sum(d => d.Quantity) : 0;

         var additionsSince = StockAdjustmentRepository.FindPositiveAdjustments(item.ProductId, stockTake.LocationId).Where(sa => sa.AdjustedOn > stockTake.StockTakeDate);
         var quantityReceivedSince = additionsSince.Any() ? additionsSince.Sum(s => s.Quantity) : 0;

         var stockAtDateTime = currentAvailableStock + quantityDeductedSince - quantityReceivedSince;

         return stockAtDateTime;
      }

      private void AdjustStock(StockTakeItem stockTakeItem, string submittedBy, int locationId)
      {
         var adjustment = StockAdjustmentHelper.CreateItemAdjustment(stockTakeItem.ProductId, locationId, stockTakeItem.Adjustment.Value, false, AdjustmentSource.StockTake, submittedBy);

         //For surplus we need to add new stock
         if (stockTakeItem.Adjustment > 0)
         {
            adjustment.IsPositive = true;
            _adjustmentHelper.AddPositiveAdjustmentWithStockToStockTakeItem(adjustment, stockTakeItem, submittedBy);
         }

         //For write offs we need to consume current stock
         if (stockTakeItem.Adjustment < 0)
         {
            adjustment.IsPositive = false;
            adjustment.Quantity = -adjustment.Quantity;

            //Consume items
            var product = ProductRepository.Find(stockTakeItem.ProductId);
            _adjustmentHelper.AdjustItem(adjustment, product, submittedBy);
         }
      }

      private bool ValidateStockTake(StockTake stockTakeToProcess, int stockTakeId)
      {
         //Validate the stock take
         if (stockTakeToProcess == null)
         {
            _logger.Warning("Failed to find valid stock take {StockTakeId} to process", stockTakeId);
            return false;
         }

         if (!stockTakeToProcess.StockTakeItems.Any() || stockTakeToProcess.StockTakeItems.All(sti => sti.DeletedOn.HasValue))
         {
            _logger.Warning("Stock take {StockTakeId} does not have any stock take items available to process", stockTakeId);
            return false;
         }

         if (stockTakeToProcess.Status != StockTakeStatus.Created && stockTakeToProcess.Status != StockTakeStatus.Failed)
         {
            _logger.Warning("Cannot process stock take {StockTakeId} due to it's status being {StockTakeStatus}", stockTakeId, stockTakeToProcess.Status);
            return false;
         }

         return true;
      }
   }

   public interface IStockTakeUnitOfWork
   {
      IProductRepository ProductRepository { get; }
      IStockTakeRepository StockTakeRepository { get; }
      IStockRepository StockRepository { get; }
      IStockLocationRepository StockLocationRepository { get; }
      IPreferenceRepository PreferenceRepository { get; }
      void ProcessStockTake(StockTake stockTakeToProcess);
      bool ProcessStockTake(int stockTakeId, string username);
      void Commit();
   }
}