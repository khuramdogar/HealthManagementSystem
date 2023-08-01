using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class OrderableItemsController : Controller
   {
      private readonly IOrderableItemsUnitOfWork _unitOfWork;
      private readonly IPropertyProvider _propertyProvider;
      private readonly ICustomLogger _logger;
      private IOrderSubmissionUnitOfWork _orderSubmissionUnitOfWork;

      public OrderableItemsController(IOrderableItemsUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider, IOrderSubmissionUnitOfWork orderSubmissionUnitOfWork)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
         _propertyProvider = propertyProvider;
         _orderSubmissionUnitOfWork = orderSubmissionUnitOfWork;
      }

      [HttpPost]
      public ActionResult OrderAll(PostOrderDTO orderInfo)
      {
         try
         {
            _logger.Information("Attempting to create orders for all outstanding orderable items");
            string errorMessage = null;
            var orderableItems = _unitOfWork.GetOrderableItems();

            // create order per supplier
            var orderableItemsBySupplier = orderableItems.GroupBy(oi => oi.Supplier);

            foreach (var itemsForSupplier in orderableItemsBySupplier)
            {
               var order = new Order
               {
                  CreatedBy = User.Identity.Name,
                  DeliveryLocationId = orderInfo.DeliveryLocationId,
                  LastModifiedBy = User.Identity.Name,
                  Name = (string.IsNullOrWhiteSpace(itemsForSupplier.Key) ? "(No supplier)" : itemsForSupplier.Key) + " - " + DateTime.Now.ToShortDateString(),
                  Status = OrderStatus.Ordered,
               };

               var nonInvoiceOrderableItems = itemsForSupplier.Where(oi => oi.Source != OrderableItemSource.Invoice).ToList();
               if (!_unitOfWork.AddOrderableItemsToOrder(order, nonInvoiceOrderableItems, out errorMessage, User.Identity.Name))
               {
                  return new HttpStatusCodeResult(HttpStatusCode.BadRequest, errorMessage);
               }

               // generate consumption notices for this supplier
               foreach (var itemToInvoice in itemsForSupplier.Where(oi => oi.Source == OrderableItemSource.Invoice))
               {
                  if (!itemToInvoice.ConsumptionId.HasValue) continue;
                  if (!_unitOfWork.InvoiceConsumption(itemToInvoice.ConsumptionId.Value, orderInfo.DeliveryLocationId,
                     itemToInvoice.ProductId, order.Name, orderInfo.LedgerId, User.Identity.Name, out errorMessage))
                  {
                     _logger.Warning("Failed to invoice consumption {ConsumptionId} - {ErrorMessage}", itemToInvoice.ConsumptionId, errorMessage);
                     return new HttpStatusCodeResult(HttpStatusCode.BadRequest, errorMessage);
                  }
               }
               _unitOfWork.OrderRepository.Add(order);
            }

            _unitOfWork.Commit();

            try
            {
               //Process any outstanding orders
               var processor = new OrderSubmissionProcessor(_logger, _propertyProvider, _orderSubmissionUnitOfWork);
               processor.ProcessOrders();
            }
            catch (Exception exception)
            {
               _logger.Error(exception,"Error processing order submissions");
            }

            return Json(true, JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem ordering all items for approval"));
            return View("Error", new HandleErrorInfo(exception, "OrderableItems", "OrderAll"));
         }
      }

      [HttpPost]
      public ActionResult ArchiveOrderableItems(int[] consumptionIds)
      {
         if (consumptionIds == null || consumptionIds.Length == 0)
         {
            _logger.Debug("No consumption ids supplied");
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest,
               "No records supplied. Please provide items to be ignored");
         }

         string errorMessage;
         if (!_unitOfWork.HealthTrackConsumptionRepository.ArchiveConsumptions(consumptionIds.ToList(), out errorMessage, User.Identity.Name))
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadGateway, errorMessage);
         }
         _unitOfWork.Commit();

         return Json(true, JsonRequestBehavior.AllowGet);
      }
   }
}