using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Orders;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class OrderableItemsController : ApiController
   {
      private readonly IOrderableItemsUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;
      private readonly IOrderSubmissionUnitOfWork _orderSubmissionUnitOfWork;

      public OrderableItemsController(IOrderableItemsUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider, IOrderSubmissionUnitOfWork orderSubmissionUnitOfWork)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
         _propertyProvider = propertyProvider;
         _orderSubmissionUnitOfWork = orderSubmissionUnitOfWork;
      }

      [HttpGet]
      public DataSourceResult GetOrderableItems(
         [ModelBinder(typeof(WebApiDataSourceRequestModelBinder))] DataSourceRequest request)
      {
         try
         {
            var orderableItems = _unitOfWork.GetOrderableItems();
            return orderableItems.ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Orderable Items."));
            return new DataSourceResult();
         }
      }

      [HttpPost]
      [ActionName("Order")]
      public IHttpActionResult PostOrder(PostOrderDTO orderInfo)
      {
         try
         {
            _logger.Information("Attemping to create order {OrderName} for products {ProductIds}", orderInfo.OrderName, orderInfo.OrderableItems.Select(oi => oi.ProductId));

            var result = new OrderCreationResult{Errors = new List<string>()};

            if (orderInfo.OrderableItems.Any(oi => oi.Quantity < 1))
            {
               const string message = "Could not create order for item with invalid quantity.";
               _logger.Warning(message);
               result.CreationSuccess = false;
               result.Errors.Add(message);
               result.AdditionalInfo = message;
               return Json(result);
            }

            var containsUnorderableProducts =
              _unitOfWork.ProductRepository.ContainsUnorderableProducts(orderInfo.OrderableItems.Select(oi => oi.ProductId));

            if (containsUnorderableProducts)
            {
               _logger.Warning("Attempted to create order {OrderName} for unorderable product {ProductId}", orderInfo.OrderName, orderInfo.OrderableItems.Select(oi => oi.ProductId));
               const string message = "Cannot create order containing unorderable products.";
               _logger.Warning(message);
               result.CreationSuccess = false;
               result.Errors.Add(message);
               result.AdditionalInfo = message;
               return Json(result);
            }

            var order = new Order
            {
               CreatedBy = User.Identity.Name,
               DeliveryLocationId = orderInfo.DeliveryLocationId,
               LastModifiedBy = User.Identity.Name,
               LedgerId = orderInfo.LedgerId,
               Name = orderInfo.OrderName,
               Status = OrderStatus.Ordered,
            };

            string errorMessage;
            // add items to order to the order
            var nonInvoiceItems = orderInfo.OrderableItems.Where(oi => oi.OrderableItemSource != OrderableItemSource.Invoice).ToList();
            if (nonInvoiceItems.Any())
            {
               if (!_unitOfWork.AddOrderableItemsToOrder(order, nonInvoiceItems, out errorMessage, User.Identity.Name))
               {
                  _logger.Warning(errorMessage);
                  result.CreationSuccess = false;
                  result.Errors.Add(errorMessage);
                  result.AdditionalInfo = errorMessage;
                  return Json(result);
               }
               _unitOfWork.OrderRepository.Add(order);
            }

            var itemsToInvoice =
               orderInfo.OrderableItems.Where(oi => oi.OrderableItemSource == OrderableItemSource.Invoice).ToList();

            if (itemsToInvoice.Any(inv => inv.Ids == null || inv.Ids.Length == 0))
            {
               _logger.Warning("Could not create invoice for orderable item without a consumption ID");
               var message = "Unable to create invoice for record";
               _logger.Warning(message);
               result.CreationSuccess = false;
               result.Errors.Add(message);
               result.AdditionalInfo = message;
               return Json(result);
               
            }

            // create separate orders(invoices) for each item
            foreach (var itemToInvoice in itemsToInvoice)
            {
               var consumptionId = itemToInvoice.Ids[0];

               if (!_unitOfWork.InvoiceConsumption(consumptionId, orderInfo.DeliveryLocationId, itemToInvoice.ProductId,
                  orderInfo.OrderName, orderInfo.LedgerId, User.Identity.Name, out errorMessage))
               {
                  _logger.Warning(errorMessage);
                  result.CreationSuccess = false;
                  result.Errors.Add(errorMessage);
                  result.AdditionalInfo = errorMessage;
                  return Json(result);
               }
            }


            var consumptionIds = orderInfo.OrderableItems.Where(
               oi => oi.OrderableItemSource == OrderableItemSource.Replacement)
               .SelectMany(oi => oi.Ids).ToList();
            if (!_unitOfWork.HealthTrackConsumptionRepository.ArchiveConsumptions(consumptionIds, out errorMessage, User.Identity.Name))
            {
               _logger.Warning(errorMessage);
               result.CreationSuccess = false;
               result.Errors.Add(errorMessage);
               result.AdditionalInfo = errorMessage;
               return Json(result);
            }
            
            _unitOfWork.Commit();

            result.CreationSuccess = true;
            result.OrderId = order.InventoryOrderId;

            //Submit order to order channels
            try
            {
               //Check order needs submitting
               var processor = new OrderSubmissionProcessor(_logger, _propertyProvider, _orderSubmissionUnitOfWork);
               processor.PrepareUnknownOrders(new List<Order> { order }.AsQueryable());
               
               //Submit order to order channel
               var submission = processor.SubmitOrder(order.InventoryOrderId);
               if (submission.SubmissionStatus == SubmissionStatus.Queued)
               {
                  _logger.Information("Order {OrderId} is queued for order channel submission",order.InventoryOrderId);
                  result.SubmittedToChannel = true;
               }
               else
               {
                  var message = "Order {OrderId} was not submitted to channel: " + submission.AdditionalDetails;
                  _logger.Warning(message);
                  result.SubmittedToChannel = false;
                  result.Errors.Add(message);
               }
            }
            catch (Exception exception)
            {
               _logger.Error(exception,"There was an error attempting to submit order {OrderId} to an order channel", order.InventoryOrderId);
               result.Errors.Add("Submission failure: " + exception.Message);
            }
            
            return Json(result);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem creating an order from an orderable item {OrderName} from {OrderableItemsCount} items.", orderInfo.OrderName, orderInfo.OrderableItems.Count());
            return InternalServerError(exception);
         }
      }
   }
}
