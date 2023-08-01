using System;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class StockAdjustmentUnitOfWork : IStockAdjustmentUnitOfWork
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;


      public StockAdjustmentUnitOfWork(IDbContextInventoryContext context, IPropertyProvider propertyProvider,
         ICustomLogger logger)
      {
         _context = context;

         StockAdjustmentRepository = new StockAdjustmentRepository(context);
         StockRepository = new StockRepository(context);
         ProductRepository = new ProductRepository(context, propertyProvider);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public IProductRepository ProductRepository { get; }

      public IStockRepository StockRepository { get; }

      public IStockAdjustmentRepository StockAdjustmentRepository { get; }

      public IQueryable<StockAdjustment> FindDeductionsForNegativeStockLevel(int productId, int locationId)
      {
         // check if stock level is negative
         var isNegative =
            StockRepository.GetNegativeStock().Any(ns => ns.ProductId == productId && ns.StoredAt == locationId);
         if (!isNegative)
            return null;

         var nonNegativeStockEntries =
            StockRepository.FindAll().Where(s => s.ProductId == productId && s.StoredAt == locationId && !s.IsNegative);

         var mostRecentNonNegativeStockId = Convert.ToInt32(nonNegativeStockEntries.Max(s => (int?) s.StockId));

         var deductions = from sd in StockAdjustmentRepository.FindAllDeductions()
            where sd.StockAdjustmentStocks.Any(sas => sas.Stock.IsNegative && sas.StockId > mostRecentNonNegativeStockId && sas.Stock.ProductId == productId && sas.Stock.StoredAt == locationId)
            select sd;
         return deductions;
      }

      public void AdjustReceivedStock(StockAdjustment stockAdjustment, int newQuantity, string username)
      {
         var delta = newQuantity - stockAdjustment.Quantity;

         var adjustableStockEntries =
            stockAdjustment.StockAdjustmentStocks.Where(sas => sas.Stock.BatchNumber == null && sas.Stock.SerialNumber == null && sas.Stock.StockStatus == StockStatus.Available)
               .Select(sas => sas.Stock).ToList();

         var existingStock = stockAdjustment.StockAdjustmentStocks.First().Stock;

         if (delta == 0)
            return;

         if (delta > 0)
         {
            // need to add more stock, add a new stock entry
            stockAdjustment.StockAdjustmentStocks.Add(new StockAdjustmentStock
            {
               Stock = StockRepository.CreateNewStock(existingStock.ProductId, existingStock.StoredAt, delta,
                  username)
            });
         }
         else if (delta < 0)
         {
            var absDelta = Math.Abs(delta);
            if (adjustableStockEntries.Sum(s => s.Quantity) < absDelta) throw new StockException("Unable to adjust stock for order. Not enough available stock left.");

            // reduce stock
            while (absDelta > 0)
            {
               var stockToAdjust = adjustableStockEntries.FirstOrDefault(s => s.StockStatus == StockStatus.Available);
               if (stockToAdjust == null) throw new StockException("Unable to adjust stock as there are no stock items available for the current adjustment.");
               if (stockToAdjust.Quantity >= absDelta)
               {
                  stockToAdjust.Quantity -= absDelta;
                  stockToAdjust.ReceivedQuantity -= absDelta;
                  absDelta = 0;
               }
               else
               {
                  var toDeduct = stockToAdjust.Quantity;
                  stockToAdjust.Quantity -= toDeduct;
                  stockToAdjust.ReceivedQuantity -= toDeduct;
                  absDelta -= toDeduct;

                  stockToAdjust.StockStatus = StockStatus.Deducted;
               }
            }
         }

         stockAdjustment.LastModifiedBy = username;
         stockAdjustment.LastModifiedOn = DateTime.Now;
         stockAdjustment.Quantity = newQuantity;
      }

      /// <summary>
      ///    Creates a new Stock entity with the same information as the original stock entry
      /// </summary>
      /// <param name="existingStock"></param>
      /// <param name="itemAdjustment"></param>
      /// <param name="quantityToSplit"></param>
      /// <returns>The new stock entry </returns>
      public static Stock SplitStock(Stock existingStock, ItemAdjustment itemAdjustment, int quantityToSplit)
      {
         var newStock = new Stock
         {
            BatchNumber = itemAdjustment.BatchNumber,
            BoughtPrice = existingStock.BoughtPrice,
            CreatedBy = existingStock.CreatedBy,
            ExpiresOn = existingStock.ExpiresOn,
            LastModifiedOn = existingStock.LastModifiedOn,
            Owner = existingStock.Owner,
            PriceModelOnReceipt = existingStock.PriceModelOnReceipt,
            ProductId = existingStock.ProductId,
            Quantity = quantityToSplit,
            ReceivedQuantity = quantityToSplit,
            SellPrice = existingStock.SellPrice,
            SerialNumber = itemAdjustment.SerialNumber,
            StockStatus = StockStatus.Available,
            StoredAt = existingStock.StoredAt,
            TaxRateOnReceipt = existingStock.TaxRateOnReceipt
         };

         // get the positive adjustment if it exists, you should not be able to have more than one positive adjustment per stock item
         var positiveAdjustmentStock = existingStock.StockAdjustmentStocks.SingleOrDefault(sas => sas.StockAdjustment.IsPositive);
         if (positiveAdjustmentStock != null)
            newStock.StockAdjustmentStocks.Add(new StockAdjustmentStock
            {
               StockAdjustment = positiveAdjustmentStock.StockAdjustment
            });

         existingStock.Quantity = existingStock.Quantity - quantityToSplit;
         existingStock.ReceivedQuantity = existingStock.ReceivedQuantity - quantityToSplit;
         if (existingStock.Quantity < 1) existingStock.StockStatus = StockStatus.Deducted;

         return newStock;
      }
   }

   public interface IStockAdjustmentUnitOfWork
   {
      IStockRepository StockRepository { get; }
      IProductRepository ProductRepository { get; }
      IStockAdjustmentRepository StockAdjustmentRepository { get; }
      void Commit();
      IQueryable<StockAdjustment> FindDeductionsForNegativeStockLevel(int productId, int locationId);
      void AdjustReceivedStock(StockAdjustment stockAdjustment, int newQuantity, string username);
   }
}