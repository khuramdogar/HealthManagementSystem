using AutoMapper;
using DevExpress.Web.Mvc;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Orders;
using HMS.HealthTrack.Web.Areas.Inventory.Reporting;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Clinical;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Clinical;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = "Inventory")]
   public class OrdersController : Controller
   {
      private readonly IOrderUnitOfWork _unitOfWork;
      private readonly IOrderRepository _orderRepository;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;
      private readonly IPreferenceRepository _preferenceRepository;
      private readonly IInventoryUsedRepository _inventoryUsedRepository;
      private readonly IMedicalRecordRepository _medicalRecordRepository;
      private readonly IConfigurationRepository _configurationProvider;
      private readonly IOrderItemRepository _orderItemRepository;
      private readonly int _productLedgerType;
      private readonly int _orderLedgerType;
      private readonly IOrderSubmissionUnitOfWork _orderSubmissionUnitOfWork;

      private const string ReversedOrderMessageKey = "ReversedOrder";

      public OrdersController(IOrderUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider, IPreferenceRepository preferenceRepository,
         IInventoryUsedRepository inventoryUsedRepository, IMedicalRecordRepository medicalRecordRepository, IConfigurationRepository configurationProvider,
         IOrderSubmissionUnitOfWork orderSubmissionUnitOfWork, IOrderItemRepository orderItemRepository)
      {
         _unitOfWork = unitOfWork;
         _orderRepository = unitOfWork.OrderRepository;
         _logger = logger;
         _propertyProvider = propertyProvider;
         _preferenceRepository = preferenceRepository;
         _inventoryUsedRepository = inventoryUsedRepository;
         _medicalRecordRepository = medicalRecordRepository;
         _configurationProvider = configurationProvider;
         _orderSubmissionUnitOfWork = orderSubmissionUnitOfWork;
         _productLedgerType = _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;
         _orderLedgerType = _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.OrderLedgerType).LedgerTypeId;
         _orderItemRepository = orderItemRepository;
      }

      // GET: Order
      public ActionResult Index()
      {
         if (TempData[ReversedOrderMessageKey] != null)
         {
            ViewBag.ReversedOrder = TempData[ReversedOrderMessageKey];
         }
         return View();
      }
      
      // GET: Order/Details/5
      public ActionResult Details(int? id)
      {
         try
         {
            if (!id.HasValue)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var inventoryOrder = _orderRepository.Find(id.Value);
            if (inventoryOrder == null)
               return HttpNotFound();

            ViewBag.StockLocations = new SelectList(_unitOfWork.StockLocationRepository.FindAll()
               .Select(l => new SelectListItem { Value = l.LocationId.ToString(), Text = l.Name }), "Value", "Text");

            var model = Mapper.Map<Order, DetailsOrdersViewModel>(inventoryOrder);

            if (inventoryOrder.LedgerId.HasValue)
            {
               model.LedgerCode = GeneralLedgerHelper.GetGeneralLedgerCode(_unitOfWork.GeneralLedgerRepository,
                  _unitOfWork.GeneralLedgerTierRepository, _propertyProvider, inventoryOrder.LedgerId.Value,
                  _orderLedgerType);
            }

            return View(model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Order with ID '{0}'.", id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "Orders", "Details"));
         }
      }

      // GET: Order/Details/5
      public ActionResult SimplifiedDetails(int? id)
      {
         try
         {
            if (id == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var inventoryOrder = _orderRepository.FindAsync(id);
            if (inventoryOrder == null)
            {
               return HttpNotFound();
            }
            return View(Mapper.Map<Order, DetailsOrdersViewModel>(inventoryOrder));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Order with ID '{0}'.", id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "Orders", "SimplifiedDetails"));
         }
      }

      // POST: Order/Delete/5
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
         try
         {
            var inventoryOrder = _orderRepository.FindAsync(id);
            inventoryOrder.DeletedBy = User.Identity.Name;
            _orderRepository.Remove(inventoryOrder);
            _orderRepository.Commit();
            return RedirectToAction("Index");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem deleting the Order with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Orders", "Delete"));
         }
      }

      public ActionResult Approve()
      {
         try
         {
            ViewBag.ConsumptionsMissingPaymentClassCount =
               _unitOfWork.StockConsumptionRepository.FindDeductionsRequiringPaymentClass()
                  .Count(c => !c.PaymentClass.HasValue);

            ViewBag.UserPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_preferenceRepository,
               _propertyProvider, User.Identity);

            ViewBag.LedgerType =
               _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.OrderLedgerType).LedgerTypeId;
            return View();
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving consumptions missing payment class."));
            return View("Error", new HandleErrorInfo(exception, "Orders", "Approve"));
         }
      }

      public ActionResult Request(int id)
      {
         try
         {
            var order = _orderRepository.Find(id);

            if (order == null)
            {
               ModelState.AddModelError("", "Failed to find order " + id);
               return RedirectToAction("Index");
            }

            order.LastModifiedBy = User.Identity.Name;
            order.LastModifiedOn = DateTime.Now;
            order.Status = OrderStatus.PendingApproval;
            _orderRepository.Commit();

            return RedirectToAction("Details", new { id = id });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem requesting the Order with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Orders", "Request"));
         }
      }

      public ActionResult Place(int id)
      {
         try
         {
            var order = _orderRepository.Find(id);

            if (order == null)
            {
               ModelState.AddModelError("", "Failed to find order " + id);
               return RedirectToAction("Index");
            }

            order.LastModifiedBy = User.Identity.Name;
            order.LastModifiedOn = DateTime.Now;
            order.Status = OrderStatus.Ordered;

            foreach (var item in order.Items)
            {
               item.Status = (byte)OrderItemStatus.Ordered;
            }
            _orderRepository.Commit();

            return RedirectToAction("Details", new { id = id });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem placing the Order with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Orders", "Place"));
         }
      }

      /// <summary>
      /// Direct action for pushing an order through to order channel submission
      /// </summary>
      /// <param name="id">Order id</param>
      /// <returns>Submisison result</returns>
      public ActionResult SubmitOrderToOrderChannel(int id)
      {
         var order = _orderRepository.Find(id);
         //TODO:Check order status

         //Check order needs submitting
         var processor = new OrderSubmissionProcessor(_logger, _propertyProvider, _orderSubmissionUnitOfWork);
         processor.PrepareUnknownOrders(new List<Order> { order }.AsQueryable());
         //TODO:Check order status for submission requirements

         //Submit order to order channel
         var submission = processor.SubmitOrder(id);

         //Check result
         return Json(submission, JsonRequestBehavior.AllowGet);
      }

      public ActionResult Reject(int id)
      {
         try
         {
            var order = _orderRepository.Find(id);

            if (order == null)
            {
               ModelState.AddModelError("", "Failed to find order " + id);
               return RedirectToAction("Index");
            }

            order.LastModifiedBy = User.Identity.Name;
            order.LastModifiedOn = DateTime.Now;
            order.Status = OrderStatus.Rejected;

            foreach (var item in order.Items)
            {
               item.Status = OrderItemStatus.Cancelled;
            }
            _orderRepository.Commit();

            return RedirectToAction("Details", new { id = id });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem rejecting the Order with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Orders", "Reject"));
         }
      }

      [HttpGet]
      public ActionResult Print(int id)
      {
         try
         {
            var model = new PrintOrdersDisplayModel
            {
               ProductId = id
            };
            return View(model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem printing the Order with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Orders", "Print"));
         }
      }

      public ActionResult OrderReportPartial(int id)
      {
         try
         {
            var displayModel = new PrintOrdersDisplayModel
            {
               ProductId = id,
               Report = GetOrderReport(id)

            };
            return PartialView("_OrderReportPartial", displayModel);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem generating the printable report for the Order with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Orders", "OrderReportPartial"));
         }
      }

      public ActionResult OrderReportPartialExport(int id)
      {
         try
         {
            var displayModel = new PrintOrdersDisplayModel
            {
               ProductId = id,
               Report = GetOrderReport(id)

            };
            return DocumentViewerExtension.ExportTo(displayModel.Report, base.Request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem exporting the printable report for the Order with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Orders", "OrderReportPartialExport"));
         }
      }

      private OrderReport GetOrderReport(int? id)
      {
         var order = _orderRepository.FindDetails(id);
         var reportLogo = GetLocationLogo(order.DeliveryLocation);
         var reportModel = GetReportDataSource(order);
         var report = new OrderReport(reportLogo, order.DeliveryLocation, order.DeliveryLocation, order.Status, reportModel); //TODO: One of the delivery locations should be the user's (sender) locationS
         return report;
      }

      /// <summary>
      /// Gets the data to be bound to the report.
      /// Returns a List as the Datasource must be a list.
      /// </summary>
      /// <param name="order"></param>
      /// <returns></returns>
      private List<PrintOrdersReportModel> GetReportDataSource(Order order)
      {
         var reportModel = new PrintOrdersReportModel();

         GeneralLedger generalLedger = null;
         var ledgerCode = string.Empty;
         if (order.LedgerId.HasValue)
         {
            var ledgerType = _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.OrderLedgerType).LedgerTypeId;
            generalLedger = _unitOfWork.GeneralLedgerRepository.Find(order.LedgerId.Value);
            ledgerCode = GeneralLedgerHelper.GetGeneralLedgerCode(_unitOfWork.GeneralLedgerRepository,
               _unitOfWork.GeneralLedgerTierRepository, _propertyProvider, generalLedger.LedgerId, ledgerType);
         }

         var orderDetails = new OrderDetails
         {
            ChargeAccount = generalLedger == null ? string.Empty : generalLedger.Name,
            CreatedBy = order.CreatedBy.UsernameToShortName(),
            DateCreated = order.DateCreated,
            LedgerCode = ledgerCode,
            Name = order.Name,
            Notes = order.Notes,
            OrderId = order.InventoryOrderId,
            OrderStatus = order.Status,
            Status = HelperMethods.GetEnumDisplayName<OrderStatus>((OrderStatus)order.Status),
         };
         reportModel.OrderDetails = orderDetails;

         if (order.Status == OrderStatus.PartiallyReceived || order.Status == OrderStatus.Complete)
         {
            reportModel.Products = CreateOrderItems(order.Items.Where(oi => oi.Status == OrderItemStatus.Received || oi.Status == OrderItemStatus.PartiallyReceived), ReportDataProductClassification.Received);

            reportModel.CancelledProducts = CreateOrderItems(order.Items.Where(oi => oi.Status == OrderItemStatus.Cancelled), ReportDataProductClassification.Cancelled);

            reportModel.BackOrderProducts = CreateOrderItems(order.Items.Where(
               oi => oi.Status == OrderItemStatus.Ordered || oi.Status == OrderItemStatus.PartiallyReceived), ReportDataProductClassification.BackOrdered);

            var orderTotal = reportModel.Products.Aggregate(0m,
               (current, product) => current + product.LineTotal) + reportModel.CancelledProducts.Aggregate(0m,
                  (current, product) => current + product.LineTotal) + reportModel.BackOrderProducts.Aggregate(0m,
                     (current, product) => current + product.LineTotal);
            reportModel.OrderDetails.Total = orderTotal;
         }
         else if (order.Status == OrderStatus.Invoiced)
         {
            reportModel.Products = CreateOrderItems(order.Items,
               ReportDataProductClassification.Consignment);
            reportModel.OrderDetails.Total = reportModel.Products.Aggregate(0m, (current, product) => current + product.LineTotal);
         }
         else
         {
            reportModel.Products = CreateOrderItems(order.Items, ReportDataProductClassification.All);
            reportModel.OrderDetails.Total = reportModel.Products.Aggregate(0m, (current, product) => current + product.LineTotal);
         }

         var reportModels = new List<PrintOrdersReportModel>();
         reportModels.Add(reportModel);
         return reportModels;
      }

      private enum ReportDataProductClassification
      {
         All,
         Received,
         BackOrdered,
         Cancelled,
         Consignment
      }

      private OrderLineItem CreateLineItem(int quantity, OrderItem orderItem, DateTime? receivedOn)
      {
         var product = orderItem.Product;

         var lineItem = new OrderLineItem
         {
            Description = product.Description,
            LPC = product.LPC,
            Manufacturer = product.Manufacturer,
            ProductId = product.ProductId,
            Quantity = quantity,
            RebateCode = product.RebateCode,
            ReceivedOn = receivedOn,
            SPC = product.SPC,
            UnitPrice = orderItem.UnitPrice.HasValue ? orderItem.UnitPrice.Value : 0,
         };
         if (product.LedgerId.HasValue)
         {
            lineItem.GLC = GeneralLedgerHelper.GetGeneralLedgerCode(_unitOfWork.GeneralLedgerRepository,
               _unitOfWork.GeneralLedgerTierRepository, _propertyProvider, product.LedgerId.Value, _productLedgerType);

            lineItem.GLC = string.Format("{0} ({1})", lineItem.GLC, product.GeneralLedger.Name);
         }
         else
         {
            lineItem.GLC = string.Empty;
         }
         return lineItem;
      }

      private List<OrderLineItem> CreateOrderItems(IEnumerable<OrderItem> orderItems, ReportDataProductClassification classification)
      {
         var lineItems = new List<OrderLineItem>();
         foreach (var orderItem in orderItems)
         {
            switch (classification)
            {
               case ReportDataProductClassification.BackOrdered:
                  var receivedQuantity = orderItem.StockAdjustments.Sum(s => s.Quantity);
                  var backOrderedQuantity = orderItem.Quantity - receivedQuantity;
                  lineItems.Add(CreateLineItem(backOrderedQuantity, orderItem, null));
                  break;

               case ReportDataProductClassification.Received:
                  lineItems.AddRange(orderItem.StockAdjustments.Select(adjustment => CreateLineItem(adjustment.Quantity, orderItem, adjustment.AdjustedOn)));
                  break;

               case ReportDataProductClassification.Consignment:

                  var consignmentLineItem = CreateLineItem(orderItem.Quantity, orderItem, null);
                  var invUsedId = orderItem.ConsumptionNotificationManagements.Single().invUsed_ID;
                  var invUsed = _inventoryUsedRepository.Find(invUsedId);
                  if (invUsed != null && invUsed.container_ID != null)
                  {
                     // add patient details
                     var patientDetails = _medicalRecordRepository.GetPatientDetailsFromConsumption(invUsed.container_ID.Value);
                     consignmentLineItem.MedicareNumber = patientDetails.Medicare;
                     consignmentLineItem.PatientDOB = patientDetails.Dob.Value.ToShortDateString();
                     consignmentLineItem.PatientId = patientDetails.PatientId;
                     consignmentLineItem.PatientName = string.Format("{0} {1} {2}", patientDetails.Title,
                        patientDetails.FirstName, patientDetails.Surname);
                     consignmentLineItem.PatientDetails = string.Format("{0} - {1} ({2})",
                        consignmentLineItem.PatientId, consignmentLineItem.PatientName, consignmentLineItem.PatientDOB);
                     if (string.IsNullOrWhiteSpace(consignmentLineItem.MedicareNumber))
                     {
                        consignmentLineItem.PatientDetails += string.Format(" - Medicare: {0}", consignmentLineItem.MedicareNumber);
                     }
                  }
                  lineItems.Add(consignmentLineItem);
                  break;
               default:
                  lineItems.Add(CreateLineItem(orderItem.Quantity, orderItem, null));
                  break;
            }
         }
         return lineItems;
      }

      private Image GetLocationLogo(StockLocation location)
      {
         if (location != null && location.LogoImage != null && location.LogoImage.Length > 0)
         {
            var logoImage = location.LogoImage;
            var logoStream = new MemoryStream(logoImage);
            var logo = Image.FromStream(logoStream);
            return logo;
         }
         return null;
      }

      [HttpGet]
      public ActionResult OrderStockSet()
      {
         try
         {
            var model = new OrderQuickAdd { AvailableStockSets = _unitOfWork.StockSetRepository.FindAll() };
            return View(model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving stock sets."));
            return View("Error", new HandleErrorInfo(exception, "Orders", "OrderStockSet"));
         }
      }

      [HttpPost]
      public ActionResult OrderStockSet(int stockSetId)
      {
         try
         {
            var stockSet = _unitOfWork.StockSetRepository.Find(stockSetId);

            var order = new Order { CreatedBy = User.Identity.Name, Name = stockSet.Name, LastModifiedBy = User.Identity.Name };

            foreach (var stockSetItem in stockSet.Items)
            {
               order.Items.Add(new OrderItem
               {
                  ProductId = stockSetItem.ProductId,
                  Quantity = stockSetItem.Quantity,
                  UnitPrice = stockSetItem.Product.BuyPrice
               });
            }

            _orderRepository.Add(order);

            _unitOfWork.Commit();

            return RedirectToAction("Details", new { id = order.InventoryOrderId });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem ordering the stock set with ID '{0}'.", stockSetId));
            return View("Error", new HandleErrorInfo(exception, "Orders", "OrderStockSet"));
         }
      }

      public ActionResult AdditionalDetails(int id)
      {
         try
         {
            var order = _orderRepository.Find(id);
            return View("AdditionalDetails", Mapper.Map<Order, DetailsOrdersViewModel>(order));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Order with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Orders", "AdditionalDetails"));
         }
      }

      public ActionResult History(int id)
      {
         try
         {
            var order = _orderRepository.Find(id);
            return View("History", Mapper.Map<Order, DetailsOrdersViewModel>(order));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem deleting the Order with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Orders", "History"));
         }
      }

      public ActionResult Reverse(int id)
      {
         var order = _orderRepository.Find(id);
         if (order == null)
         {
            _logger.Warning("Attempt to reverse Order with ID {OrderId} failed. Order could not be found.", id);
            return View("Error", new HandleErrorInfo(new Exception("Order does not exist."), "Orders", "Details"));
         }

         _logger.Information("Attempting to reverse Order {OrderName} with {OrderId}.", order.Name, id);
         _unitOfWork.ReverseOrder(order, User.Identity.Name);
         OrderRepository.UpdateOrderStatus(order, User.Identity.Name);
         _unitOfWork.Commit();

         TempData[ReversedOrderMessageKey] = order.Name;
         return RedirectToAction("Index");
      }

      public ActionResult GetOrderStatuses()
      {
         try
         {
            var orderStatusesToDisplay = (OrderStatus[])Enum.GetValues(typeof(OrderStatus));

            var orderStatusList = orderStatusesToDisplay.Select(orderStatus => new SelectListItem
            {
               Text = HelperMethods.GetEnumDisplayName<OrderStatus>(orderStatus),
               Value = ((int)orderStatus).ToString()
            }).OrderBy(o => o.Text).ToList();


            return Json(orderStatusList, JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the order statuses."));
            return View("Error", new HandleErrorInfo(exception, "Orders", "GetOrderStatuses"));
         }
      }

      public ActionResult GetOpenOrdersForProduct([DataSourceRequest] DataSourceRequest request, int productId)
      {
         try
         {
            var orders =
               _unitOfWork.OrderRepository.OrdersForProduct(productId).Where(o => (o.Status == OrderStatus.Ordered || o.Status == OrderStatus.PartiallyReceived) && o.Items.Any(oi => oi.ProductId == productId && (oi.Status == OrderItemStatus.Ordered || oi.Status == OrderItemStatus.PartiallyReceived)));

            return Json(orders.Select(Mapper.Map<Order, IndexOrdersViewModel>).ToDataSourceResult(request),
               JsonRequestBehavior.AllowGet);
         }
         catch (Exception ex)
         {
            _logger.Error(ex, "An error occurred while attempting to request stock levels");
            return Json(null, JsonRequestBehavior.AllowGet);
         }
      }

      public ActionResult GetOrdersForProduct([DataSourceRequest] DataSourceRequest request, int? productId)
      {
         try
         {
            var orders = productId.HasValue ? _unitOfWork.OrderRepository.OrdersForProduct(productId.Value) : _unitOfWork.OrderRepository.FindAll();

            return Json(orders.Select(Mapper.Map<Order, IndexOrdersViewModel>).ToDataSourceResult(request),
               JsonRequestBehavior.AllowGet);
         }
         catch (Exception ex)
         {
            _logger.Error(ex, "An error occurred while attempting to request stock levels");
            return Json(null, JsonRequestBehavior.AllowGet);
         }
      }

      public JsonResult ProductHasOpenOrders(int productId)
      {
         return Json(_unitOfWork.OrderRepository.OrdersForProduct(productId).Any(o => o.Status == OrderStatus.Ordered || o.Status == OrderStatus.PartiallyReceived), JsonRequestBehavior.AllowGet);
      }

      public ActionResult Cancel(int id)
      {
         try
         {
            var order = _orderRepository.Find(id);

            if (order == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unable to find the specified order.");
            }

            if (order.Items.Any(oi => oi.Status != OrderItemStatus.Ordered))
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot cancel order as some items have been processed. To resolve, set remaining order items to Cancelled and complete the order.");
            }

            _orderRepository.Cancel(order, User.Identity.Name);
            _orderRepository.Commit();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem rejecting the Order with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Orders", "Reject"));
         }
      }


      //TODO: Move to order controller
      [System.Web.Http.HttpGet]
      public DataSourceResult GetPatientDetails(DataSourceRequest request, int orderItemId,
         int productId)
      {
         try
         {
            var orderItem = _orderItemRepository.Find(orderItemId);
            if (orderItem == null)
            {
               _logger.Warning("Cannot find order item {OrderItemId} to retrieve patient details", orderItemId);
               return null;
            }

            if (!orderItem.ConsumptionNotificationManagements.Any())
            {
               return null;
            }

            if (orderItem.ConsumptionNotificationManagements.Count() > 1)
            {
               _logger.Error("Multiple consumption notifications found for order item with ID {OrderItemID}",
                  orderItemId);
               throw new Exception("Multiple consumption notifications for order item");
            }

            var consumptionId = orderItem.ConsumptionNotificationManagements.Single().invUsed_ID;
            var consumption = _inventoryUsedRepository.Find(consumptionId);
            if (consumption == null)
            {
               _logger.Warning("Cannot find consumption for order item with ID {OrderItemId}", orderItemId);
               return null;
            }

            if (consumption.container_ID == null)
            {
               _logger.Warning("Cannot find container for consumption with consumption ID {ConsumptionId}",
                  consumptionId);
               return null;
            }

            var patientDetails = _medicalRecordRepository.GetPatientDetailsFromConsumption(consumption.container_ID.Value);

            return new List<PatientDetails> { patientDetails }.ToDataSourceResult(request);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Patient Details from consumptions with an Order Item ID '{0}' and a Product ID of '{1}'.", orderItemId, productId));
            return new DataSourceResult();
         }
      }
   }
}
