using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class StockRepository : IStockRepository
   {
      private readonly IDbContextInventoryContext _context;

      public StockRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public bool ItemInStock(int productId, string serialNumber)
      {
         var query = from i in _context.Stocks
            where
               i.ProductId == productId
               && i.DeletedOn == null
               && i.StockStatus == StockStatus.Available
            select i;

         return !string.IsNullOrEmpty(serialNumber) ? query.Any(i => i.SerialNumber == serialNumber) : query.Any();
      }

      public bool ItemInStock(int productId)
      {
         return _context.Stocks.Any(s => s.ProductId == productId && s.StockStatus == StockStatus.Available && !s.DeletedOn.HasValue);
      }

      public int GetStockCount(int productId, int locationId)
      {
         var availableStock = GetAvailableStock(locationId).Where(p => p.ProductId == productId);
         var availableStockCount = availableStock.Any() ? availableStock.Sum(s => s.Quantity) : 0;
         var localStock = GetAvailableLocalStock(locationId, productId).ToList();
         var localStockCount = localStock.Any() ? localStock.Sum(ls => ls.Quantity) : 0;

         return availableStockCount + localStockCount;
      }

      public IQueryable<Stock> GetProductStockBatches(int productId)
      {
         return from i in _context.Stocks
            where i.ProductId == productId && i.StockStatus == StockStatus.Available && i.DeletedOn == null
            select i;
      }

      public Stock CreateNegativeStock(ItemAdjustment itemAdjustment, Product product, string username)
      {
         if (itemAdjustment.StockLocationId == null) throw new StockException("Unable to create negative stock at empty location");

         var stockItem = new Stock
         {
            BatchNumber = itemAdjustment.BatchNumber,
            CreatedBy = username,
            CreatedOn = DateTime.Now,
            IsNegative = true,
            LastModifiedBy = username,
            LastModifiedOn = DateTime.Now,
            Quantity = itemAdjustment.Quantity,
            ReceivedQuantity = itemAdjustment.Quantity,
            SerialNumber = itemAdjustment.SerialNumber,
            StockStatus = StockStatus.Available,
            StoredAt = itemAdjustment.StockLocationId.Value
         };

         product.Stocks.Add(stockItem);
         return stockItem;
      }

      public Stock CreateNewStock(int productId, int locationId, int quantity, string username)
      {
         return new Stock
         {
            CreatedBy = username,
            CreatedOn = DateTime.Now,
            LastModifiedBy = username,
            LastModifiedOn = DateTime.Now,
            ProductId = productId,
            Quantity = quantity,
            ReceivedQuantity = quantity,
            StockStatus = StockStatus.Available,
            StoredAt = locationId
         };
      }

      public Stock Find(int id)
      {
         return _context.Stocks.Include(s => s.StockAdjustmentStocks.Select(sas => sas.StockAdjustment))
            .SingleOrDefault(s => s.StockId == id);
      }

      public IQueryable<Stock> FindAll()
      {
         return _context.Stocks
            .Include(s => s.StockAdjustmentStocks.Select(sas => sas.StockAdjustment.OrderItem.Order))
            .Include(s => s.StorageLocation).Include(s => s.Product).Where(s => !s.DeletedOn.HasValue);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public void Add(Stock newStock)
      {
         newStock.LastModifiedOn = DateTime.Now;
         _context.Stocks.Add(newStock);
      }

      public IQueryable<Stock> FindForOrderItemAtLocation(int orderItemId, int locationId, int productId)
      {
         var stocks =
            FindAll()
               .Where(
                  s =>
                     s.StoredAt == locationId && s.ProductId == productId &&
                     s.StockAdjustmentStocks.Any(sas => sas.StockAdjustment.OrderItemId == orderItemId));
         return stocks;
      }

      public IQueryable<Stock> FindForOrderItem(int orderItemId)
      {
         var stock =
            _context.StockAdjustments.Where(sa => sa.OrderItemId == orderItemId && sa.DeletedOn == null)
               .SelectMany(sa => sa.StockAdjustmentStocks)
               .Select(sas => sas.Stock)
               .Include(s => s.StorageLocation)
               .Include(s => s.Product);
         return stock;
      }

      public IQueryable<LowStock> GetLowStock()
      {
         // products and quantity low in stock
         var currentLowStock =
            from p in
               _context.Products.Include(p => p.StockRequests)
                  .Include(
                     p => p.Stocks.Select(s => s.Product.ProductCategories.Select(pc => pc.Category.StockSettings)))
                  .Include(p => p.Stocks.Select(s => s.Product.ProductSettings))
            let stockCount =
               p.Stocks.Any(s => s.StockStatus == StockStatus.Available)
                  ? p.Stocks.Where(s => s.StockStatus == StockStatus.Available).Sum(s => s.Quantity)
                  : 0
            where p.DeletedOn == null && p.ReorderThreshold.HasValue && p.TargetStockLevel.HasValue
                  && !(p.ReorderThreshold.Value == 0 && p.TargetStockLevel.Value == 0) && p.ManageStock
            // exclude products with 0 values for stock control
            select new LowStock
            {
               Product = p,
               ReorderThreshold = p.ReorderThreshold.Value,
               StockCount = stockCount,
               TargetStockLevel = p.TargetStockLevel.Value
            };

         var orderedStock = GetOrderedStockByProduct();

         //  products and quantities where number in stock and the number ordered is lower than the reorder threshold for that product
         var lowStock = from ls in currentLowStock
            join os in orderedStock on ls.Product.ProductId equals os.Product.ProductId into oss
            from os in oss.DefaultIfEmpty()
            let totalLowStock = os != null ? ls.StockCount + os.StockCount : ls.StockCount
            where totalLowStock <= ls.ReorderThreshold
                  && totalLowStock < ls.TargetStockLevel // if low stock is at target, don't order (when reorder threshold == target stock level)
            select new LowStock
            {
               Product = ls.Product,
               ReorderThreshold = ls.ReorderThreshold,
               StockCount = totalLowStock,
               TargetStockLevel = ls.TargetStockLevel
            };

         return lowStock;
      }

      public IQueryable<LowStock> GetOrderedStockByProduct()
      {
         // products and quantity in an ordered state
         var orderedStock = from ois in _context.OrderItemSources
            where ois.OrderItem.Status == OrderItemStatus.Ordered && ois.StockRequestId == null
            group ois by ois.OrderItem.Product
            into gois
            where gois.Key.ReorderThreshold.HasValue && gois.Key.TargetStockLevel.HasValue
            let orderedCount = gois.Key.OrderItems.Sum(q => q.Quantity)
            select new LowStock
            {
               Product = gois.Key,
               ReorderThreshold = gois.Key.ReorderThreshold.Value,
               StockCount = orderedCount,
               TargetStockLevel = gois.Key.TargetStockLevel.Value
            };
         return orderedStock;
      }

      public IQueryable<Stock> GetSerialNumberStock(int productId, int locationId)
      {
         return
            GetAvailableStock()
               .Where(s => s.ProductId == productId && s.StoredAt == locationId && !string.IsNullOrEmpty(s.SerialNumber));
      }

      public IEnumerable<string> GetSerialNumbers(int productId, int locationId)
      {
         return GetSerialNumberStock(productId, locationId).Select(s => s.SerialNumber);
      }

      public bool StockWithoutSerialExists(int productId, int? locationId)
      {
         return
            GetAvailableStock()
               .Any(s => s.ProductId == productId && s.StoredAt == locationId && string.IsNullOrEmpty(s.SerialNumber));
      }

      public bool SerialInStock(int productId, string serialNumber)
      {
         return
            _context.Stocks.Any(
               s =>
                  s.ProductId == productId && s.StockStatus == StockStatus.Available &&
                  s.SerialNumber.Equals(serialNumber));
      }

      public bool HasStock(int productId, int locationId)
      {
         return GetAvailableStock(locationId).Any(stock => stock.ProductId == productId);
      }

      public void UpdateStockLocation(Stock stockItem, int location, string name)
      {
         stockItem.StoredAt = location;
         stockItem.LastModifiedBy = name;
         stockItem.LastModifiedOn = DateTime.Now;
      }

      public IQueryable<Stock> GetAvailableStock()
      {
         return
            _context.Stocks
               .Include(s => s.Product.PrimarySupplierCompany)
               .Include(s => s.Product.ProductCategories)
               .Include(s => s.Product.ProductSettings)
               .Include(s => s.Product.StockTakeItems)
               .Where(stock =>
                  stock.DeletedOn == null &&
                  stock.StockStatus == StockStatus.Available
                  && stock.Quantity > 0);
      }

      public IQueryable<Stock> GetAvailableStock(int locationId)
      {
         return
            _context.Stocks.Include(s => s.Product.PrimarySupplierCompany).Where(stock =>
               stock.StoredAt == locationId &&
               stock.DeletedOn == null &&
               stock.StockStatus == StockStatus.Available &&
               stock.Quantity > 0);
      }

      public bool UpdateSerialNumber(Stock stockItem, string serialNumber, string username)
      {
         if (
            _context.Stocks.Where(s => s.ProductId == stockItem.ProductId && s.SerialNumber != null)
               .Any(s => s.SerialNumber.Equals(serialNumber)))
            return false;
         stockItem.SerialNumber = serialNumber;
         stockItem.LastModifiedBy = username;
         stockItem.LastModifiedOn = DateTime.Now;
         return true;
      }

      public IQueryable<NegativeStock> GetNegativeStock()
      {
         return _context.NegativeStocks;
      }

      public IQueryable<Stock> Find(Expression<Func<Stock, bool>> predicate)
      {
         return _context.Stocks.Where(predicate);
      }

      public void Remove(Stock entity)
      {
         entity.DeletedOn = DateTime.Now;
      }

      private IEnumerable<Stock> GetAvailableLocalStock(int locationId, int productId)
      {
         var localStock = _context.Stocks.Local.Where(s =>
            s.StoredAt == locationId &&
            s.ProductId == productId &&
            s.DeletedOn == null &&
            s.StockStatus == StockStatus.Available &&
            s.Quantity > 0);
         return localStock;
      }

      public int GetNegativeStockCount()
      {
         return _context.NegativeStocks.Count();
      }
   }

   public interface IStockRepository
   {
      bool ItemInStock(int productId, string serialNumber);
      IQueryable<Stock> GetProductStockBatches(int productId);
      IQueryable<LowStock> GetLowStock();
      IQueryable<LowStock> GetOrderedStockByProduct();
      IQueryable<Stock> FindForOrderItem(int orderItemId);
      Stock Find(int id);
      void Commit();
      IEnumerable<string> GetSerialNumbers(int productId, int locationId);
      bool StockWithoutSerialExists(int productId, int? locationId);
      bool SerialInStock(int productId, string serialNumber);
      bool HasStock(int productId, int locationId);
      void UpdateStockLocation(Stock stockItem, int location, string name);
      IQueryable<Stock> GetSerialNumberStock(int productId, int locationId);
      IQueryable<Stock> GetAvailableStock();
      IQueryable<Stock> GetAvailableStock(int locationId);
      bool UpdateSerialNumber(Stock stockItem, string serialNumber, string username);
      void Add(Stock createNewStock);
      IQueryable<Stock> FindForOrderItemAtLocation(int orderItemId, int locationId, int productId);
      IQueryable<Stock> FindAll();
      int GetStockCount(int productId, int locationId);
      IQueryable<NegativeStock> GetNegativeStock();
      bool ItemInStock(int productId);
      Stock CreateNegativeStock(ItemAdjustment itemAdjustment, Product product, string username);
      Stock CreateNewStock(int productId, int locationId, int quantity, string username);
   }
}