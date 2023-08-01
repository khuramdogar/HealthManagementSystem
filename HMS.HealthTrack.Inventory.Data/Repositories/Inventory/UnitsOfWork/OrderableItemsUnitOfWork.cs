using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class OrderableItemsUnitOfWork : IOrderableItemsUnitOfWork
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;

      public OrderableItemsUnitOfWork(IDbContextInventoryContext context, IPropertyProvider propertyProvider,
         ICustomLogger logger)
      {
         _context = context;
         _propertyProvider = propertyProvider;
         _logger = logger;
         OrderChannelRepository = new OrderChannelRepository(context);
         ConfigurationRepository = new ConfigurationRepository(context, _logger);
         ProductRepository = new ProductRepository(context, _propertyProvider);
         StockAdjustmentRepository = new StockAdjustmentRepository(context);
         StockRequestRepository = new StockRequestRepository(context);
         StockRepository = new StockRepository(context);
         OrderRepository = new OrderRepository(context);
         HealthTrackConsumptionRepository = new HealthTrackConsumptionRepository(context);
         PaymentClassMappingRepository = new PaymentClassMappingRepository(context);
      }

      public IPaymentClassMappingRepository PaymentClassMappingRepository { get; }

      public IProductRepository ProductRepository { get; }

      public IStockAdjustmentRepository StockAdjustmentRepository { get; }

      public IStockRequestRepository StockRequestRepository { get; }

      public IStockRepository StockRepository { get; }

      public IOrderRepository OrderRepository { get; }

      public IConfigurationRepository ConfigurationRepository { get; }

      public IHealthTrackConsumptionRepository HealthTrackConsumptionRepository { get; }

      public IOrderChannelRepository OrderChannelRepository { get; }

      public List<OrderableItem> GetOrderableItems()
      {
         var orderableItems = new List<OrderableItem>();
         var primaryPriceId = _propertyProvider.PrimaryBuyPriceTypeId;

         // find products where stock level is below the threshold and has both reorder threshold and target stock level and has had a stock take done
         var lowStocks = from ls in StockRepository.GetLowStock()
            where (ls.Product.UseCategorySettings
                     ? ls.Product.ProductCategories.SelectMany(pc => pc.Category.StockSettings)
                        .All(s => s.SettingId != InventoryConstants.StockSettings.Unorderable)
                     : ls.Product.ProductSettings.All(s => s.SettingId != InventoryConstants.StockSettings.Unorderable))
                  &&
                  ls.Product.StockTakeItems.Any(
                     sti => !sti.DeletedOn.HasValue && sti.Status == StockTakeItemStatus.Complete)
            // TODO: stock take at location
            select ls;

         var lowStock = lowStocks.ToList();

         //Top up
         AddStockReplenishment(orderableItems, primaryPriceId, lowStock);

         //Invoice
         AddItemsForInvoice(orderableItems, primaryPriceId);

         AddConsumptionsForReplacement(orderableItems, primaryPriceId);

         //Request
         orderableItems.AddRange(GetRequestedItems(primaryPriceId));

         return orderableItems;
      }

      public void AddStockReplenishment(List<OrderableItem> orderableItems, int primaryPriceId, List<LowStock> lowStock)
      {
         var orderedStock = StockRepository.GetOrderedStockByProduct().ToList();

         var heldBackConsumedProducts =
            StockAdjustmentRepository.FindDeductionsMissingRequiredPaymentClass().ToList();

         //  products and quantities where number in stock and the number ordered is lower than the reorder threshold for that product
         var productsToReplenish = from ls in lowStock
            let primaryPrice = ls.Product.Prices.FirstOrDefault(pp => pp.PriceTypeId == primaryPriceId)
            let productOrderableItems = orderableItems.Where(oi => oi.ProductId == ls.Product.ProductId)
            let productOrderedStock = orderedStock.Where(os => os.Product.ProductId == ls.Product.ProductId)
            let heldConsumptions = heldBackConsumedProducts.Where(c => c.ProductId == ls.Product.ProductId)
            let totalOrderedStock =
               productOrderedStock != null
                  ? ls.StockCount + productOrderedStock.Sum(os => os.StockCount)
                  : ls.StockCount
            let totalOrderedHeldStock =
               heldConsumptions != null
                  ? totalOrderedStock + heldConsumptions.Sum(c => c.Quantity)
                  : totalOrderedStock
            let totalStock =
               productOrderableItems != null
                  ? productOrderableItems.Sum(oi => oi.Quantity) + totalOrderedHeldStock
                  : totalOrderedHeldStock
            where totalStock <= ls.ReorderThreshold && ls.Product.ManageStock && ls.Product.AutoReorderSetting == ReorderSettings.SpecifyLevels
            select new OrderableItem
            {
               BuyPrice = primaryPrice != null ? primaryPrice.BuyPrice : null,
               Currency = primaryPrice != null ? primaryPrice.BuyCurrency : null,
               Description = ls.Product.Description,
               Quantity =
                  CalculateReorderAmount(totalStock, ls.TargetStockLevel, ls.Product.MinimumOrder,
                     ls.Product.OrderMultiple),
               ProductId = ls.Product.ProductId,
               SPC = ls.Product.SPC,
               Supplier =
                  ls.Product.PrimarySupplier.HasValue ? ls.Product.PrimarySupplierCompany.companyName : string.Empty,
               Source = OrderableItemSource.Topup
            };
         orderableItems.AddRange(productsToReplenish);
      }

      public void AddConsumptionsForReplacement(List<OrderableItem> orderableItems, int primaryPriceId)
      {
         var unmanagedConsumptions = from nic in HealthTrackConsumptionRepository.FindNonArchivedConsumptions()
            join pcm in PaymentClassMappingRepository.FindAll() on nic.PaymentClass equals pcm.PaymentClass into gpcm
            from pcm in gpcm.DefaultIfEmpty()
            join epm in _context.ExternalProductMappings on nic.ProductId equals epm.ExternalProductId
            join p in ProductRepository.FindAll() on epm.InventoryProductId equals p.ProductId
            let requiresPaymentClass = p.UseCategorySettings // requires payment class
               ? p.ProductCategories.SelectMany(pc => pc.Category.StockSettings)
                  .Any(s => s.SettingId == InventoryConstants.StockSettings.RequiresPaymentClass)
               : p.ProductSettings.Any(ps => ps.SettingId == InventoryConstants.StockSettings.RequiresPaymentClass)
            where p.ManageStock == false && p.AutoReorderSetting == ReorderSettings.OneForOneReplace
                                         && (p.ReplaceAfter == null || nic.ConsumedOn > p.ReplaceAfter)
                                         && nic.deleted == false
                                         && nic.ProcessingStatus == ConsumptionProcessingStatus.Processed
                                         && (requiresPaymentClass && nic.PaymentClass != null || !requiresPaymentClass)
            // either requires a payment class and has one or doesn't require a payment class
            let price =
               p.UsePaymentClassPrice && pcm != null && pcm.PriceTypeId != null
                  ? p.Prices.FirstOrDefault(pp => pp.PriceTypeId == pcm.PriceTypeId)
                  : p.Prices.FirstOrDefault(pp => pp.PriceTypeId == primaryPriceId)
            // product can use the payment class to dictate the price, try and use mapped product price type
            select new OrderableItem
            {
               BuyPrice = price.BuyPrice,
               ConsumptionId = nic.UsedId,
               Currency = price.BuyCurrency,
               Description = p.Description,
               ProductId = p.ProductId,
               Quantity = nic.Quantity.Value,
               Source = OrderableItemSource.Replacement,
               SPC = p.SPC,
               Supplier = p.PrimarySupplier != null ? p.PrimarySupplierCompany.companyName : string.Empty
            };
         orderableItems.AddRange(unmanagedConsumptions);
      }

      public List<OrderableItem> GetRequestedItems(int primaryPriceId)
      {
         // add requests on top of what is already there
         var openRequests = StockRequestRepository.FindApprovedRequests();
         var requestItems = from sr in openRequests
            group sr by sr.Product
            into gsr
            let productPrice = gsr.Key.Prices.FirstOrDefault(price => price.PriceTypeId == primaryPriceId)
            select new OrderableItem
            {
               BuyPrice = productPrice.BuyPrice,
               Currency = productPrice.BuyCurrency,
               Description = gsr.Key.Description,
               ProductId = gsr.Key.ProductId,
               Quantity = gsr.Sum(sr => sr.ApprovedQuantity.Value),
               RequestIds =
                  openRequests.Where(request => request.ProductId == gsr.Key.ProductId)
                     .Select(request => request.StockRequestId)
                     .ToList(),
               SPC = gsr.Key.SPC,
               Source = OrderableItemSource.Request,
               Supplier = gsr.Key.PrimarySupplier.HasValue ? gsr.Key.PrimarySupplierCompany.companyName : string.Empty
            };
         return requestItems.ToList();
      }

      public bool AddOrderableItemsToOrder(Order order, List<OrderableItemDTO> orderableItemDTOs, out string errorMessage, string username)
      {
         var orderableItems = orderableItemDTOs.Select(oi => new OrderableItem
         {
            ProductId = oi.ProductId,
            Quantity = oi.Quantity,
            Source = oi.OrderableItemSource,
            RequestIds = oi.OrderableItemSource == OrderableItemSource.Request ? oi.Ids.ToList() : null,
            ConsumptionId = oi.OrderableItemSource == OrderableItemSource.Replacement ? oi.Ids.Single() : (int?) null
         });

         return AddOrderableItemsToOrder(order, orderableItems.ToList(), out errorMessage, username);
      }

      public bool AddOrderableItemsToOrder(Order order, List<OrderableItem> orderableItems, out string errorMessage, string username)
      {
         IQueryable<ProductStockRequest> requests;
         if (!GetRequestsForOrderableItemsIfValid(orderableItems, out errorMessage, out requests))
            return false;

         IQueryable<HealthTrackConsumption> consumptions;
         if (!GetConsumptionsForOrderableItemsIfValid(orderableItems, out errorMessage, out consumptions))
            return false;

         if (requests.Any(r => r.IsUrgent)) order.IsUrgent = true;

         // create order item per product
         var orderableItemProducts = orderableItems.Where(oi => oi.Source != OrderableItemSource.Invoice).GroupBy(oi => oi.ProductId);
         foreach (var orderableItemProduct in orderableItemProducts)
         {
            var product = ProductRepository.Find(orderableItemProduct.Key);
            if (product == null)
            {
               _logger.Warning("Unable to find product {ProductId} when creating an order for this product.", orderableItemProduct.Key);
               errorMessage = "Unable to create order. Please refresh and try again";
               return false;
            }

            // get order item
            var orderItem = GetOrderItemForProduct(orderableItemProduct.Key, order);

            // add requests
            AddRequestsToOrderItem(requests, orderItem);

            // add top ups
            AddTopUpsToOrderItem(orderableItemProduct.Where(oip => oip.Source == OrderableItemSource.Topup), orderItem);

            // replacements
            AddReplacementsToOrderItems(consumptions, orderItem);

            // archive consumptions 
            if (!HealthTrackConsumptionRepository.ArchiveConsumptions(consumptions.Select(c => c.UsedId).ToList(), out errorMessage, username)) return false;
         }

         return true;
      }

      public bool InvoiceConsumption(int consumptionId, int deliveryLocationId, int productId, string orderName,
         int? ledgerId, string username, out string errorMessage)
      {
         var management =
            HealthTrackConsumptionRepository.FindConsumptionNotificationManagement(consumptionId);
         // new management
         if (management == null)
         {
            management = new ConsumptionNotificationManagement
            {
               Reported = false,
               invUsed_ID = consumptionId,
               Invoiced = false
            };
            HealthTrackConsumptionRepository.AddConsumptionNotificationManagement(management);
         }
         else if (management.Invoiced)
         {
            _logger.Warning("Cannot invoice consumption that has already be invoiced");
            errorMessage = "Invoice already created.";
            return false;
         }

         var quantity = HealthTrackConsumptionRepository.Find(consumptionId).Quantity;
         if (!quantity.HasValue)
         {
            _logger.Warning("Cannot invoice consumption without a specified quantity");
            errorMessage = "No quantity specified for invoice";
            return false;
         }

         var product = ProductRepository.Find(productId);
         if (product == null)
         {
            _logger.Warning("Cannot invoice consumption for product with {ProductId} which cannot be found", productId);
            errorMessage = "Cannot find product";
            return false;
         }

         int priceTypeId;
         if (product.UsePaymentClassPrice)
         {
            var consumption = HealthTrackConsumptionRepository.Find(consumptionId);
            if (consumption == null)
            {
               _logger.Warning("Cannot find consumption record to retreive payment class");
               errorMessage = "Cannot find consumption record";
               return false;
            }

            var paymentClassMapping = PaymentClassMappingRepository.Find(consumption.PaymentClass);
            if (paymentClassMapping == null || !paymentClassMapping.PriceTypeId.HasValue)
            {
               _logger.Warning("Cannot find mapping for consumption's payment class {PaymentClass}",
                  consumption.PaymentClass);
               errorMessage = "The payment class for this record has not been mapped. Please correct this before continuing.";
               return false;
            }

            priceTypeId = paymentClassMapping.PriceTypeId.Value;
         }
         else
         {
            priceTypeId = _propertyProvider.PrimaryBuyPriceTypeId;
         }

         var price = product.Prices.SingleOrDefault(p => p.PriceTypeId == priceTypeId);
         if (price == null)
         {
            _logger.Warning("Cannot find price for product {ProductId} with price type {PriceTypeId}", productId,
               priceTypeId);
            errorMessage = "Cannot find price for product";
            return false;
         }

         var invoiceItem = OrderRepository.InvoiceConsumption(deliveryLocationId, username, productId, consumptionId,
            orderName, ledgerId, quantity.Value, price.BuyPrice);
         management.OrderItem = invoiceItem;
         management.Invoiced = true;

         errorMessage = null;
         return true;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public static int CalculateReorderAmount(int currentStock, int targetLevel, int minOrder, int multiplesOf)
      {
         var amountToOrder = targetLevel - currentStock;

         //No stock required
         if (amountToOrder <= 0)
            return 0;

         //Must meet the minimum
         if (minOrder != 0 && amountToOrder < minOrder)
            amountToOrder = minOrder;

         //"Round" to product's "order in multiples of"
         amountToOrder = multiplesOf == 0
            ? amountToOrder
            : (int) (Math.Ceiling(amountToOrder / Convert.ToDouble(multiplesOf)) * multiplesOf);

         return amountToOrder;
      }

      private void AddItemsForInvoice(List<OrderableItem> orderableItems, int primaryPriceId)
      {
         var itemsForInvoice = from nic in HealthTrackConsumptionRepository.FindNonInvoicedConsumptions()
            join pcm in PaymentClassMappingRepository.FindAll() on nic.PaymentClass equals pcm.PaymentClass into gpcm
            from pcm in gpcm.DefaultIfEmpty()
            join epm in _context.ExternalProductMappings on nic.ProductId equals epm.ExternalProductId
            join p in ProductRepository.FindAll() on epm.InventoryProductId equals p.ProductId
            let forInvoice = p.UseCategorySettings // request invoice
               ? p.ProductCategories.SelectMany(pc => pc.Category.StockSettings)
                  .Any(s => s.SettingId == InventoryConstants.StockSettings.SupplierInvoiceOnUse)
               : p.ProductSettings.Any(ps => ps.SettingId == InventoryConstants.StockSettings.SupplierInvoiceOnUse)
            let requiresPaymentClass = p.UseCategorySettings // requires payment class
               ? p.ProductCategories.SelectMany(pc => pc.Category.StockSettings)
                  .Any(s => s.SettingId == InventoryConstants.StockSettings.RequiresPaymentClass)
               : p.ProductSettings.Any(ps => ps.SettingId == InventoryConstants.StockSettings.RequiresPaymentClass)
            where forInvoice && nic.deleted == false
                             && nic.ArchivedOn == null
                             && nic.ProcessingStatus != ConsumptionProcessingStatus.Ignored
                             && (requiresPaymentClass && nic.PaymentClass != null || !requiresPaymentClass)

            // either requires a payment class and has one or doesn't require a payment class
            let price =
               p.UsePaymentClassPrice && pcm != null && pcm.PriceTypeId != null
                  ? p.Prices.FirstOrDefault(pp => pp.PriceTypeId == pcm.PriceTypeId)
                  : p.Prices.FirstOrDefault(pp => pp.PriceTypeId == primaryPriceId)
            // product can use the payment class to dictate the price, try and use mapped product price type
            select new OrderableItem
            {
               BuyPrice = price.BuyPrice,
               ConsumptionId = nic.UsedId,
               Currency = price.BuyCurrency,
               Description = p.Description,
               ProductId = p.ProductId,
               Quantity = nic.Quantity.Value,
               Source = OrderableItemSource.Invoice,
               SPC = p.SPC,
               Supplier = p.PrimarySupplier != null ? p.PrimarySupplierCompany.companyName : string.Empty
            };

         orderableItems.AddRange(itemsForInvoice);
      }

      private bool GetConsumptionsForOrderableItemsIfValid(List<OrderableItem> orderableItems, out string errorMessage, out IQueryable<HealthTrackConsumption> consumptions)
      {
         if (orderableItems.All(oi => oi.Source != OrderableItemSource.Replacement))
         {
            errorMessage = string.Empty;
            consumptions = new List<HealthTrackConsumption>().AsQueryable();
            return true;
         }

         if (orderableItems.Any(oi => oi.Source == OrderableItemSource.Replacement && oi.ConsumptionId == null))
         {
            _logger.Warning("Unable to create an order from selected items. Consumption replacement items are missing ids.");
            errorMessage = "Unable to create an order from the specified replacements.";
            consumptions = null;
            return false;
         }

         var consumptionIds = orderableItems.Select(oi => oi.ConsumptionId).ToList();
         consumptions = HealthTrackConsumptionRepository.FindHealthTrackConsumptions().Where(htc => consumptionIds.Contains(htc.UsedId));
         if (consumptions.Count() != consumptionIds.Count() || consumptions.Any(c => c.Quantity == null))
         {
            consumptions = null;
            errorMessage = "Unable to create an order from the specified replacements.";
            return false;
         }

         errorMessage = string.Empty;
         return true;
      }

      private bool GetRequestsForOrderableItemsIfValid(List<OrderableItem> orderableItems, out string errorMessage, out IQueryable<ProductStockRequest> requests)
      {
         if (orderableItems.All(oi => oi.Source != OrderableItemSource.Request))
         {
            errorMessage = string.Empty;
            requests = new List<ProductStockRequest>().AsQueryable();
            return true;
         }

         if (
            orderableItems.Any(
               oi => oi.Source == OrderableItemSource.Request && (oi.RequestIds == null || !oi.RequestIds.Any())))
         {
            _logger.Warning("Unable to create an order from selected items. Request items are missing ids.");
            errorMessage = "Unable to create an order from the specified requests.";
            requests = null;
            return false;
         }

         var requestIds =
            orderableItems.Where(oi => oi.Source == OrderableItemSource.Request).SelectMany(oi => oi.RequestIds).ToArray();
         requests = StockRequestRepository.Find(requestIds.ToArray());
         var requestCount = requests.Count();

         // check all requests exist and none are duplicates and are approved
         if (requestCount != requestIds.Count()
             || requestCount != requestIds.Distinct().Count()
             || requests.Any(r => r.RequestStatus != RequestStatus.Approved)
             || requests.Any(r => !r.ApprovedQuantity.HasValue))
         {
            _logger.Warning("Attempting to add invalid requests to order.");
            {
               errorMessage = "Unable to create order from specified requests.";
               return false;
            }
         }

         errorMessage = string.Empty;
         return true;
      }

      private static void AddRequestsToOrderItem(IEnumerable<ProductStockRequest> requests, OrderItem orderItem)
      {
         foreach (var request in requests)
         {
            request.RequestStatus = RequestStatus.Ordered;
            // check if trying to add already existing request
            if (orderItem.OrderItemSources.Any(source => source.StockRequestId == request.StockRequestId))
               continue;

            orderItem.OrderItemSources.Add(new OrderItemSource
            {
               StockRequestId = request.StockRequestId,
               Quantity = request.ApprovedQuantity.Value
            });
            orderItem.Quantity += request.ApprovedQuantity.Value;
         }
      }

      private void AddTopUpsToOrderItem(IEnumerable<IOrderableItem> orderableItems, OrderItem orderItem)
      {
         foreach (var item in orderableItems)
         {
            orderItem.OrderItemSources.Add(new OrderItemSource
            {
               Quantity = item.Quantity
            });

            // update quantity of order item
            orderItem.Quantity = orderItem.Quantity += item.Quantity;
         }
      }

      private void AddReplacementsToOrderItems(IEnumerable<HealthTrackConsumption> consumptions, OrderItem orderItem)
      {
         foreach (var consumption in consumptions)
         {
            orderItem.OrderItemSources.Add(new OrderItemSource
            {
               invUsed_ID = consumption.UsedId,
               Quantity = consumption.Quantity.Value
            });
            orderItem.Quantity += consumption.Quantity.Value;
         }
      }

      public OrderItem GetOrderItemForProduct(int productId, Order order)
      {
         var price = ProductRepository.GetPrimaryPrice(productId);
         var orderItem =
            order.Items.SingleOrDefault(oi => oi.ProductId == productId && oi.Status == OrderItemStatus.Ordered);
         if (orderItem != null) return orderItem;

         orderItem = new OrderItem
         {
            ProductId = productId,
            Status = OrderItemStatus.Ordered,
            UnitPrice = price == null ? null : price.BuyPrice
         };
         order.Items.Add(orderItem);
         return orderItem;
      }
   }

   public interface IOrderableItemsUnitOfWork
   {
      IProductRepository ProductRepository { get; }
      IStockAdjustmentRepository StockAdjustmentRepository { get; }
      IStockRequestRepository StockRequestRepository { get; }
      IStockRepository StockRepository { get; }
      IOrderRepository OrderRepository { get; }
      IConfigurationRepository ConfigurationRepository { get; }
      IHealthTrackConsumptionRepository HealthTrackConsumptionRepository { get; }
      IOrderChannelRepository OrderChannelRepository { get; }
      void Commit();
      List<OrderableItem> GetOrderableItems();
      List<OrderableItem> GetRequestedItems(int primaryPriceId);
      void AddStockReplenishment(List<OrderableItem> orderableItems, int primaryPriceId, List<LowStock> lowStock);

      bool InvoiceConsumption(int consumptionId, int deliveryLocationId, int productId, string orderName, int? ledgerId,
         string username, out string errorMessage);

      void AddConsumptionsForReplacement(List<OrderableItem> orderableItems, int primaryPriceId);
      bool AddOrderableItemsToOrder(Order order, List<OrderableItem> orderableItems, out string errorMessage, string username);
      bool AddOrderableItemsToOrder(Order order, List<OrderableItemDTO> orderableItemDTOs, out string errorMessage, string username);
   }
}