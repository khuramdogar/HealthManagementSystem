using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Data.Helpers
{
   public class StockAdjustmentHelper
   {
      private readonly IStockAdjustmentRepository _stockAdjustmentRepository;
      private readonly IStockRepository _stockRepository;

      public StockAdjustmentHelper(IStockRepository stockRepository, IStockAdjustmentRepository stockAdjustmentRepository)
      {
         _stockRepository = stockRepository;
         _stockAdjustmentRepository = stockAdjustmentRepository;
      }

      public static ItemAdjustment CreateItemAdjustment(int productId, int? locationId, int quantity, bool isPositive, AdjustmentSource source, string username)
      {
         var adjustment = new ItemAdjustment
         {
            AdjustedBy = username,
            AdjustedOn = DateTime.Now,
            IsPositive = isPositive,
            Quantity = quantity,
            ProductId = productId,
            Source = source,
            StockLocationId = locationId
         };

         return adjustment;
      }

      public void AdjustItem(ItemAdjustment itemAdjustment, Product product, string username)
      {
         //Consumption validation
         if (itemAdjustment.Quantity < 1)
            throw new StockException("Cannot deduct quantity of {0} product {1}", itemAdjustment.Quantity, itemAdjustment.ProductId);

         if (product == null)
            throw new StockException("Cannot deduct quantity of {0} for product {1} which does not exist",
               itemAdjustment.Quantity, itemAdjustment.ProductId);

         if (itemAdjustment.ProductId != product.ProductId)
            throw new StockException("Cannot deduct quantity of {0}. Deduction ProductId {1} does not match the product provided {2}", itemAdjustment.ProductId, product.ProductId);

         var adjustment = _stockAdjustmentRepository.CreateStockAdjustment(itemAdjustment, username);
         var allStock = GetAvailableStockForProductAtLocation(product.ProductId, itemAdjustment.StockLocationId);

         var stockToDeduct = GetStockEntryToDeduct(allStock, itemAdjustment, product, username);

         AdjustStock(allStock, stockToDeduct, itemAdjustment, product, username, adjustment);

         _stockAdjustmentRepository.Add(adjustment, username);
      }

      private void AdjustStock(IList<Stock> allStock, Stock stockEntry, ItemAdjustment itemAdjustment, Product product, string username, StockAdjustment adjustment)
      {
         var amountToDecrement = itemAdjustment.Quantity > stockEntry.Quantity
            ? stockEntry.Quantity
            : itemAdjustment.Quantity;

         var stockItemToUpdate = stockEntry;

         if (itemAdjustment.HasSerial || itemAdjustment.HasBatchNumber) stockItemToUpdate = DeductStockWithBatchOrSerial(stockEntry, itemAdjustment, amountToDecrement);

         //  assign stock to Adjustment
         adjustment.StockAdjustmentStocks.Add(new StockAdjustmentStock
         {
            Stock = stockItemToUpdate
         });

         stockItemToUpdate.Quantity = stockItemToUpdate.Quantity - amountToDecrement;
         stockItemToUpdate.LastModifiedBy = username;
         stockItemToUpdate.LastModifiedOn = DateTime.Now;

         if (stockItemToUpdate.Quantity == 0)
            stockItemToUpdate.StockStatus = StockStatus.Deducted;


         //Remaining amount of stock to deduct
         itemAdjustment.Quantity = itemAdjustment.Quantity - amountToDecrement;

         //Check if we are done
         if (itemAdjustment.Quantity == 0)
            return;

         // find stock entry to deduct and recurse
         var stockEntryToDeduct = GetStockEntryToDeduct(allStock, itemAdjustment, product, username);
         AdjustStock(allStock, stockEntryToDeduct, itemAdjustment, product, username, adjustment);
      }

      private List<Stock> GetAvailableStockForProductAtLocation(int productId, int? locationId)
      {
         if (productId > 0)
         {
            if (locationId.HasValue)
               return _stockRepository.GetAvailableStock(locationId.Value).Where(s => s.ProductId == productId)
                  .OrderByDescending(s => s.CreatedOn)
                  .Include(s => s.StockAdjustmentStocks)
                  .ToList();
            return
               _stockRepository.GetAvailableStock()
                  .Where(s => s.ProductId == productId)
                  .OrderByDescending(s => s.CreatedOn)
                  .Include(s => s.StockAdjustmentStocks)
                  .ToList();
         }

         return new List<Stock>(); // new products are yet to have any stock;
      }

      private Stock GetStockEntryToDeduct(IList<Stock> allStock, ItemAdjustment itemAdjustment, Product product,
         string username)
      {
         //Find a stock item to deduct
         var stockItem = GetStockEntryToDeduct(allStock, itemAdjustment);
         if (stockItem == null) stockItem = _stockRepository.CreateNegativeStock(itemAdjustment, product, username);

         //Check it hasn't already been deducted
         if (stockItem.StockStatus != StockStatus.Available) throw new InvalidStockStateException(stockItem, StockStatus.Available);
         return stockItem;
      }

      private static Stock DeductStockWithBatchOrSerial(Stock stockEntry, ItemAdjustment itemAdjustment, int amountToDecrement)
      {
         Stock stockItemToUpdate;
         if (stockEntry.Quantity <= itemAdjustment.Quantity)
         {
            if (!stockEntry.StockAdjustmentStocks.Any())
            {
               if (itemAdjustment.HasSerial && stockEntry.Quantity == 1 ||
                   itemAdjustment.HasBatchNumber && stockEntry.Quantity == itemAdjustment.Quantity)
               {
                  //Use existing stock
                  stockItemToUpdate = stockEntry;
                  if (stockItemToUpdate.SerialNumber == null)
                     stockItemToUpdate.SerialNumber = itemAdjustment.SerialNumber;
                  if (stockItemToUpdate.BatchNumber == null)
                     stockItemToUpdate.BatchNumber = itemAdjustment.BatchNumber;
               }
               else
               {
                  //Split stock
                  stockItemToUpdate = StockAdjustmentUnitOfWork.SplitStock(stockEntry, itemAdjustment, stockEntry.Quantity);
               }
            }
            else
            {
               //Split stock
               stockItemToUpdate = StockAdjustmentUnitOfWork.SplitStock(stockEntry, itemAdjustment, stockEntry.Quantity);
            }
         }
         else
         {
            //Split stock
            stockItemToUpdate = StockAdjustmentUnitOfWork.SplitStock(stockEntry, itemAdjustment, amountToDecrement);
         }

         return stockItemToUpdate;
      }

      private Stock GetStockEntryToDeduct(IEnumerable<Stock> allStock, ItemAdjustment itemAdjustment)
      {
         if (itemAdjustment.StockLocationId < 1) throw new InvalidStockAdjustmentLocationException(itemAdjustment);

         IEnumerable<Stock> stockQuery = from i in allStock
            where i.ProductId == itemAdjustment.ProductId
                  && i.StockStatus == StockStatus.Available
                  && i.DeletedOn == null
            orderby i.CreatedOn descending
            select i;

         if (itemAdjustment.StockLocationId != null) stockQuery = stockQuery.Where(s => s.StoredAt == itemAdjustment.StockLocationId);

         Func<Stock, bool> anyForProduct = stock => stock.ProductId == itemAdjustment.ProductId;
         Func<Stock, bool> noSerialNoBatch = stock => stock.SerialNumber == null && stock.BatchNumber == null;
         Func<Stock, bool> noSerialMatchBatch = s => !s.HasSerial && s.BatchNumber == itemAdjustment.BatchNumber;
         Func<Stock, bool> matchSerialNoBatch = s => s.SerialNumber == itemAdjustment.SerialNumber && s.BatchNumber == null;
         Func<Stock, bool> matchSerialMatchBatch = s => s.SerialNumber == itemAdjustment.SerialNumber && s.BatchNumber == itemAdjustment.BatchNumber;
         Func<Stock, bool> matchSerialHasBatch = s => s.SerialNumber == itemAdjustment.SerialNumber && s.BatchNumber != null;

         //Has Serial And Batch
         if (itemAdjustment.HasSerial && itemAdjustment.HasBatchNumber)
         {
            if (stockQuery.Any(matchSerialMatchBatch))
               return stockQuery.Last(matchSerialMatchBatch);

            if (stockQuery.Any(matchSerialNoBatch))
               return stockQuery.Last(matchSerialNoBatch);
         }

         //Has Serial No Batch
         if (itemAdjustment.HasSerial && !itemAdjustment.HasBatchNumber)
         {
            if (stockQuery.Any(matchSerialNoBatch))
               return stockQuery.Last(matchSerialNoBatch);

            if (stockQuery.Any(matchSerialHasBatch))
               return stockQuery.Last(matchSerialHasBatch);
         }

         //No Serial Has batch
         if (!itemAdjustment.HasSerial && itemAdjustment.HasBatchNumber)
            if (stockQuery.Any(noSerialMatchBatch))
               return stockQuery.Last(noSerialMatchBatch);


         //Default
         if (stockQuery.Any(noSerialNoBatch))
            return stockQuery.Last(noSerialNoBatch);

         if (stockQuery.Any(anyForProduct))
            throw new AmbiguousStockException("Stock found for consumption does not match details entered");

         return null;
      }


      public Stock CreateNewStockWithPositiveAdjustment(ItemAdjustment adjustment, string username, OrderItem orderItem)
      {
         adjustment.IsPositive = true;
         var stockAdjustment = _stockAdjustmentRepository.CreateStockAdjustment(adjustment, username);

         var newStock = _stockRepository.CreateNewStock(adjustment.ProductId, adjustment.StockLocationId.Value,
            adjustment.Quantity, username);

         if (orderItem != null)
         {
            stockAdjustment.OrderItemId = orderItem.OrderItemId;
            newStock.BoughtPrice = orderItem.UnitPrice;
         }

         newStock.StockAdjustmentStocks.Add(new StockAdjustmentStock
         {
            StockAdjustment = stockAdjustment
         });

         return newStock;
      }

      public Stock CreateNewStockWithPositiveAdjustment(ItemAdjustment adjustment, string username)
      {
         return CreateNewStockWithPositiveAdjustment(adjustment, username, null);
      }

      /// <summary>
      ///    Stock and Stock Adjustment are added by graph because when creating and processing
      ///    a stock take the stock take may not yet exist in the database
      /// </summary>
      /// <param name="adjustment"></param>
      /// <param name="stockTakeItem"></param>
      /// <param name="username"></param>
      public void AddPositiveAdjustmentWithStockToStockTakeItem(ItemAdjustment adjustment, StockTakeItem stockTakeItem, string username)
      {
         adjustment.IsPositive = true;
         var stockAdjustment = _stockAdjustmentRepository.CreateStockAdjustment(adjustment, username);
         var newStock = _stockRepository.CreateNewStock(adjustment.ProductId, adjustment.StockLocationId.Value,
            adjustment.Quantity, username);

         stockAdjustment.StockAdjustmentStocks.Add(new StockAdjustmentStock
         {
            Stock = newStock
         });

         stockTakeItem.StockAdjustments.Add(stockAdjustment);
      }
   }
}