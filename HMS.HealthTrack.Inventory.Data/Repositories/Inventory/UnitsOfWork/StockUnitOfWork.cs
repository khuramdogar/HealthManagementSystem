using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class StockUnitOfWork : IStockUnitOfWork
   {
      private readonly StockAdjustmentHelper _adjustmentHelper;
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;

      public StockUnitOfWork(IDbContextInventoryContext context, IPropertyProvider propertyProvider, ICustomLogger logger)
      {
         _context = context;
         _logger = logger;
         ConsumptionRepository = new HealthTrackConsumptionRepository(context);
         ExternalProductMappingRepository = new ExternalProductMappingRepository(context, logger);
         ProductRepository = new ProductRepository(_context, propertyProvider);
         OrderRepository = new OrderRepository(_context);
         StockRepository = new StockRepository(context);
         CategoryRepository = new CategoryRepository(_context);
         OrderItemRepository = new OrderItemRepository(_context, propertyProvider);
         StockRequestRepository = new StockRequestRepository(_context);
         StockAdjustmentRepository = new StockAdjustmentRepository(_context);
         StockLocationRepository = new StockLocationRepository(_context);
         StockSetRepository = new StockSetRepository(context);
         _propertyProvider = propertyProvider;

         _adjustmentHelper = new StockAdjustmentHelper(StockRepository, StockAdjustmentRepository);
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public async Task CommitAsync()
      {
         await _context.ObjectContext.SaveChangesAsync();
      }

      public void ReceiveOrderItem(OrderItem orderItem, string username)
      {
         OrderItemRepository.ReceiveItem(orderItem);
         StockRequestRepository.CloseRequests(orderItem, username, true);
      }

      public void CancelOrderItem(OrderItem orderItem, string username)
      {
         OrderItemRepository.Cancel(orderItem);
         StockRequestRepository.CloseRequests(orderItem, username, false);
      }

      public void ProcessOrderItem(OrderItem orderItem, ProcessOrderDTO data, string username, int deliveryLocation)
      {
         if (data.Action == OrderItemAction.Cancel)
         {
            CancelOrderItem(orderItem, username);
            return;
         }

         if (data.Quantity < 0)
         {
            _logger.Warning("Attempted to receive negative stock for order {OrderId}", orderItem.OrderItemId);
            throw new StockException("Cannot receive a negative quantity for an order item.");
         }

         if (orderItem.Status == OrderItemStatus.Received) return;

         if (data.Quantity == 0)
         {
            if (data.Action == OrderItemAction.KeepOpen) // leave item untouched
               return;

            _logger.Debug("Closing order item {OrderItemId} as there is nothing to receive.", data.OrderItemId);
            ReceiveOrderItem(orderItem, username); // close off item
            return;
         }

         if (orderItem.Product.ManageStock) ReceiveNewStock(orderItem, data, username, deliveryLocation);

         orderItem.ReceivedOn = DateTime.Now; // update order item's received on to reflect the action performed
         switch (data.Action)
         {
            case OrderItemAction.Complete:
               ReceiveOrderItem(orderItem, username);
               break;
            case OrderItemAction.KeepOpen:
               OrderItemRepository.PartiallyReceive(orderItem);
               break;
         }
      }

      public void ReceiveNewStock(ItemAdjustment itemAdjustment, DateTime? expiresOn, string username)
      {
         if (itemAdjustment.Quantity < 1)
            throw new StockException("Cannot receive stock with a quantity less than one.");

         var product = ProductRepository.Find(itemAdjustment.ProductId);
         if (product == null) throw new StockException("Cannot receive stock for a product which does not exist.");

         if (product.ManageStock == false) throw new StockException("Cannot receive stock for a product which does not manage it's stock.");

         if (itemAdjustment.StockLocationId == null) throw new StockException("Cannot receive stock to an empty location.");

         // Serial number checks
         if (!string.IsNullOrWhiteSpace(itemAdjustment.SerialNumber))
         {
            //Check item individuality
            if (itemAdjustment.Quantity != 1)
               throw new StockException(
                  "Cannot receive stock for product {0} with a quantity of {1} and the same serial number ({2}) for all of them",
                  itemAdjustment.ProductId, itemAdjustment.Quantity, itemAdjustment.SerialNumber);
            // Check if already in stock
            if (StockRepository.SerialInStock(itemAdjustment.ProductId, itemAdjustment.SerialNumber))
               throw new StockException(
                  "Cannot receive stock for product {0} with a serial number of {1}. This item already exists.",
                  itemAdjustment.ProductId, itemAdjustment.SerialNumber);
         }

         var newStock = _adjustmentHelper.CreateNewStockWithPositiveAdjustment(itemAdjustment, username);

         newStock.BatchNumber = itemAdjustment.BatchNumber;
         newStock.ExpiresOn = expiresOn;
         newStock.PriceModelOnReceipt = product.PriceModelId; // this doesn't have a valid use yet.
         newStock.SerialNumber = itemAdjustment.SerialNumber;
         newStock.TaxRateOnReceipt = _propertyProvider.TaxRate;

         var defaultPriceTypeId = _propertyProvider.PrimaryBuyPriceTypeId;
         var price = product.Prices.Single(p => p.PriceTypeId == defaultPriceTypeId);
         newStock.BoughtPrice = price.BuyPrice;

         StockRepository.Add(newStock);
      }

      public IEnumerable<StockLevel> GetAvailableProductStockLevelByLocation(int? productId)
      {
         var availableStock = StockRepository.GetAvailableStock().Include(s => s.StorageLocation).Include(s => s.Product);

         if (productId.HasValue) availableStock = availableStock.Where(p => p.ProductId == productId.Value);

         using (new CodeTimer("GetAvailableProductStockLevelByLocation"))
         {
            var stockLevel =
               from s in availableStock.ToList() // apparently this requires evaluating in order for eager loading of product collections (e.g. ProductCategories) to function correctly
               where s.DeletedOn == null && s.StockStatus == StockStatus.Available && s.Quantity > 0
               group s by new
               {
                  s.Product,
                  s.StorageLocation
               }
               into gs
               select new StockLevel
               {
                  Product = gs.Key.Product,
                  Stock =
                     gs.Key.Product.Stocks.Where(
                           s => s.DeletedOn == null && s.StockStatus == StockStatus.Available && s.Quantity > 0)
                        .AsEnumerable(),
                  StorageLocation = gs.Key.StorageLocation
               };
            return stockLevel;
         }
      }

      public bool ReallocateStock(Stock stock, StockLocation targetLocation, int? quantity, string username)
      {
         if (targetLocation == null)
         {
            _logger.Warning("No target location specified for reallocating stock {StockId}", stock.StockId);
            return false;
         }

         if (stock.StockStatus != StockStatus.Available && stock.StockStatus != StockStatus.Reserved || stock.DeletedOn.HasValue || stock.Quantity < 0)
         {
            _logger.Information("Stock {StockId} not reallocatable - not available, deleted or 0 quantity", stock.StockId);
            return false;
         }

         if (quantity.HasValue)
         {
            if (quantity > stock.Quantity)
            {
               _logger.Information("Cannot reallocate greater quantity ({QuantityToAllocate}) than is in stock ({QuantityInStock}) for {StockId}", quantity.Value, stock.Quantity, stock.StockId);
               return false;
            }

            if (quantity < stock.Quantity)
            {
               // split stock
               _logger.Information("Reallocating {QuantityToAllocate} of {QuantityInStock} for {StockId}", quantity, stock.Quantity, stock.StockId);

               var itemAdjustment = new ItemAdjustment(); // if splitting stock, won't have serial or batch so leave adjustment empty
               var newStock = StockAdjustmentUnitOfWork.SplitStock(stock, itemAdjustment, quantity.Value);
               StockRepository.Add(newStock);

               ChangeStockLocation(newStock, targetLocation.LocationId, username);
               return true;
            }
         }

         ChangeStockLocation(stock, targetLocation.LocationId, username);
         return true;
      }

      public bool ReallocateAllStock(int currentLocation, StockLocation targetLocation, string username)
      {
         if (targetLocation == null) return false;

         var stockAtCurrentLocation = StockRepository.GetAvailableStock().Where(stock => stock.StoredAt == currentLocation);
         var result = true;
         foreach (var stock in stockAtCurrentLocation)
            if (!ReallocateStock(stock, targetLocation, null, username))
               result = false;

         if (result)
            Commit();

         return result;
      }

      public void CreateRequestsForStockSet(StockSet stockSet, int? location, bool urgent, string user)
      {
         foreach (var item in stockSet.Items)
         {
            var request = new ProductStockRequest
            {
               ApprovedQuantity = item.Quantity,
               CreatedBy = user,
               IsUrgent = urgent,
               LastModifiedBy = user,
               ProductId = item.ProductId,
               RequestLocationId = location,
               RequestedQuantity = item.Quantity
            };
            StockRequestRepository.Add(request);
         }
      }

      private void ReceiveNewStock(OrderItem orderItem, ProcessOrderDTO processData, string username, int deliveryLocation)
      {
         var adjustment = StockAdjustmentHelper.CreateItemAdjustment(orderItem.ProductId, deliveryLocation, processData.Quantity, true, AdjustmentSource.Order, username);

         var newStock = _adjustmentHelper.CreateNewStockWithPositiveAdjustment(adjustment, username, orderItem);
         StockRepository.Add(newStock);
      }

      public void ChangeStockLocation(Stock stock, int targetLocation, string username)
      {
         stock.StoredAt = targetLocation;
         stock.LastModifiedBy = username;
         stock.LastModifiedOn = DateTime.Now;
      }

      #region Repositories

      public IProductRepository ProductRepository { get; }

      public IHealthTrackConsumptionRepository ConsumptionRepository { get; }

      public ICategoryRepository CategoryRepository { get; }

      public IExternalProductMappingRepository ExternalProductMappingRepository { get; }

      public IOrderRepository OrderRepository { get; }

      public IStockRepository StockRepository { get; }

      public IOrderItemRepository OrderItemRepository { get; }

      public IStockRequestRepository StockRequestRepository { get; }

      public IStockAdjustmentRepository StockAdjustmentRepository { get; }

      public IStockLocationRepository StockLocationRepository { get; }

      public IStockSetRepository StockSetRepository { get; }

      #endregion
   }

   public interface IStockUnitOfWork
   {
      IProductRepository ProductRepository { get; }
      IOrderRepository OrderRepository { get; }
      IStockRepository StockRepository { get; }
      ICategoryRepository CategoryRepository { get; }
      IExternalProductMappingRepository ExternalProductMappingRepository { get; }
      IOrderItemRepository OrderItemRepository { get; }
      IStockRequestRepository StockRequestRepository { get; }
      IStockAdjustmentRepository StockAdjustmentRepository { get; }
      IStockLocationRepository StockLocationRepository { get; }
      IHealthTrackConsumptionRepository ConsumptionRepository { get; }
      IStockSetRepository StockSetRepository { get; }
      void Commit();
      Task CommitAsync();
      void ReceiveOrderItem(OrderItem orderItem, string username);
      void ProcessOrderItem(OrderItem orderItem, ProcessOrderDTO data, string username, int deliveryLocationId);
      IEnumerable<StockLevel> GetAvailableProductStockLevelByLocation(int? productId);
      bool ReallocateAllStock(int currentLocation, StockLocation targetLocation, string username);
      bool ReallocateStock(Stock stock, StockLocation targetLocation, int? quantity, string username);
      void CreateRequestsForStockSet(StockSet stockSet, int? location, bool urgent, string user);
      void CancelOrderItem(OrderItem orderItem, string username);
      void ReceiveNewStock(ItemAdjustment itemAdjustment, DateTime? expiresOn, string username);
   }
}