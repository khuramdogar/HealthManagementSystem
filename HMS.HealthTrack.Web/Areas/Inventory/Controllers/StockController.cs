using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Orders;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockLocations;
using HMS.HealthTrack.Web.Utils;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class StockController : Controller
   {
      private readonly IStockUnitOfWork _unitOfWork;
      private readonly IProductRepository _productRepository;
      private readonly IStockRepository _stockRepository;
      private readonly ICustomLogger _logger;
      private readonly IPreferenceRepository _preferenceRepository;
      private readonly IPropertyProvider _propertyProvider;

      public StockController(IStockUnitOfWork unitOfWork, ICustomLogger logger, IPreferenceRepository preferenceRepository, IPropertyProvider propertyProvider)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
         _preferenceRepository = preferenceRepository;
         _propertyProvider = propertyProvider;
         _productRepository = _unitOfWork.ProductRepository;
         _stockRepository = _unitOfWork.StockRepository;
      }

      [HttpPost]
      public ActionResult GetStock([DataSourceRequest] DataSourceRequest request, int[] categoryIds, string[] statuses)
      {
         try
         {
            var stock = _unitOfWork.GetAvailableProductStockLevelByLocation(null);
            if (categoryIds != null && !(categoryIds.Length == 1 && categoryIds[0] == 0))
            // when calling .read() on datasource it sends categoryIds = ['0']
            {
               stock = stock.Where(p => p.Product.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
            }

            stock = ApplyProductStatusFilter(stock, statuses);
            return Json(stock.Select(Mapper.Map<StockLevel, IndexStockViewModel>).ToDataSourceResult(request),
               JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Stock."));
            return Json(new DataSourceResult(), JsonRequestBehavior.AllowGet);
         }
      }

      private IEnumerable<StockLevel> ApplyProductStatusFilter(IEnumerable<StockLevel> stock, string[] statuses)
      {
         using (new CodeTimer("ApplyProductStatusFilter"))
         {
            if (statuses != null && statuses.Length > 0 &&
                !(statuses.Length == 1 && string.IsNullOrWhiteSpace(statuses[0])))
            {
               var statusEnums = statuses.Select(s => (int) Enum.Parse(typeof(ProductStatus), s));
               return stock.Where(p => statusEnums.Contains((int) p.Product.ProductStatus));
            }

            return stock.Where(p => p.Product.ProductStatus != ProductStatus.Disabled);
         }
      }


      [HttpGet]
      public ActionResult ReceiveProduct()
      {
         ViewBag.UserPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_preferenceRepository, _propertyProvider, User.Identity);
         var newStockAdded = TempData["newStock"];
         if (newStockAdded != null)
         {
            ViewBag.NewStock = newStockAdded;
         }
         return View();
      }

      [HttpPost]
      public ActionResult ReceiveProduct(ReceiveProductInput productInput)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               _logger.Warning("Attempting to receive product with invalid data.");
               return View(productInput);
            }

            if (productInput == null || !productInput.ProductId.HasValue || productInput.ProductId < 1)
               return ReturnReceiveError(productInput, "Please select a product to receive");

            if (!productInput.Quantity.HasValue || productInput.Quantity.Value < 1)
               return ReturnReceiveError(productInput, "Please enter a valid quantity above zero");

            int storageLocation;
            if (!int.TryParse(productInput.SelectedLocation, out storageLocation))
               return ReturnReceiveError(productInput, "Please enter a valid location");

            if (!string.IsNullOrWhiteSpace(productInput.SerialNumber))
            {
               if (productInput.Quantity != 1)
               {
                  return ReturnReceiveError(productInput,
                     "You cannot input a serial number and have a quantity other than 1.");
               }

               if (_stockRepository.SerialInStock(productInput.ProductId.Value, productInput.SerialNumber))
               {
                  return ReturnReceiveError(productInput,
                     "The specified serial number is already in stock for this product.");
               }
            }

            try
            {
               var adjustment = StockAdjustmentHelper.CreateItemAdjustment(productInput.ProductId.Value,
               storageLocation, productInput.Quantity.Value, true, AdjustmentSource.Web, User.Identity.Name);

               adjustment.BatchNumber = productInput.BatchNumber;
               adjustment.SerialNumber = productInput.SerialNumber;

               _unitOfWork.ReceiveNewStock(adjustment, productInput.ExpiresOn, User.Identity.Name);
               _unitOfWork.Commit();

               if (productInput.ContinueReceipt)
               {
                  return Request.IsAjaxRequest()
                  ? (ActionResult)Json(new { url = Url.Action("ReceiveProduct") })
                  : RedirectToAction("ReceiveProduct");
               }
               TempData["newStock"] = _productRepository.Find(productInput.ProductId.Value).Description;
               return Request.IsAjaxRequest()
                  ? (ActionResult)Json(new { url = Url.Action("ReceivedStock", "Stock") })
                  : RedirectToAction("ReceivedStock", "Stock");
            }
            catch (StockException exception)
            {
               return ReturnReceiveError(productInput, exception.Message);
            }
            catch (Exception exception)
            {
               _logger.Error(exception, string.Format("There was a problem receiving an item."));
               ModelState.AddModelError("", exception);
               return View("Error", new HandleErrorInfo(exception, "Stock", "ReceiveProduct"));
            }
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format(
                  "There was a problem receiving a product with ID '{0}', at the location '{1}' with a quantity of '{2}'.",
                  productInput != null ? productInput.ProductId.ToString() : string.Empty,
                  productInput != null ? productInput.SelectedLocation : string.Empty,
                  productInput != null && productInput.Quantity.HasValue
                     ? productInput.Quantity.ToString()
                     : string.Empty));

            return View("Error", new HandleErrorInfo(exception, "Stock", "ReceiveProduct"));
         }
      }

      [HttpGet]
      public ActionResult ReceiveOrder(int id, int? firstProduct)
      {
         var order = _unitOfWork.OrderRepository.Find(id);
         if (order == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot find order to receive");
            // cannot find
         }

         var model = Mapper.Map<Order, ReceiveOrderViewModel>(order);

         if (firstProduct.HasValue)
         {
            ViewBag.SelectProduct = firstProduct.Value;
         }

         return View(model);
      }

      [HttpPost]
      public ActionResult ReceiveOrder(ReceiveOrderModel orderModel)
      {
         var order = _unitOfWork.OrderRepository.Find(orderModel.OrderId);
         if (order == null)
         {
            _logger.Warning("Unable to find order {InventoryOrderId} to receive.", orderModel.OrderId);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot find order to receive");
         }

         if (order.DeliveryLocationId != orderModel.LocationId)
         {
            _logger.Warning("Attempted to receive order at location {LocationId} different to that specified ({DeliveryLocationId}) on the order {InventoryOrderId}", orderModel.LocationId, order.DeliveryLocationId, order.InventoryOrderId);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Delivery location of the order is different to that displayed. Please refresh and try again.");
         }

         if (order.Items == null || !order.Items.Any())
         {
            _logger.Warning("Unable to receive stock for order {InventoryOrderId} without any order items", order.InventoryOrderId);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot find order items to receive");
         }

         foreach (var itemModel in orderModel.OrderItems)
         {
            var orderItem = order.Items.SingleOrDefault(oi => oi.OrderItemId == itemModel.OrderItemId);
            if (orderItem == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Unable to find one of the specified order items.");
            }
            _unitOfWork.ProcessOrderItem(orderItem, itemModel, User.Identity.Name, orderModel.LocationId);
         }

         OrderRepository.UpdateOrderStatus(order, User.Identity.Name);
         _unitOfWork.Commit();

         return new HttpStatusCodeResult(HttpStatusCode.OK);
      }

      private ActionResult ReturnReceiveError(ReceiveProductInput model, string message = "")
      {
         if (!string.IsNullOrWhiteSpace(message))
            ModelState.AddModelError("", message);

         if (Request.IsAjaxRequest())
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, message);
         }
         return View("ReceiveProduct", model);
      }

      // GET: Stock
      public ActionResult Index()
      {
         var filterProductId = TempData["filterProductId"];
         if (filterProductId != null)
         {
            ViewBag.ProductId = filterProductId;
         }

         var filterProductName = TempData["filterProductName"];
         if (filterProductName != null)
         {
            ViewBag.ProductName = filterProductName;
         }

         return View();
      }

      public ActionResult ViewAvailableStock(int productId, string name)
      {
         TempData["filterProductId"] = productId;
         TempData["filterProductName"] = name;
         return RedirectToAction("Index");
      }

      public ActionResult ConsumeItem(int id, int locationId, string serialNumber)
      {
         var consumeStock = new DeductStock
         {
            ProductId = id,
            UsedBy = User.Identity.Name,
            StockLocationId = locationId,
         };

         if (!string.IsNullOrWhiteSpace(serialNumber))
         {
            consumeStock.SerialNumber = serialNumber;
            consumeStock.Quantity = 1;
         }

         var ledgerType = _propertyProvider.LedgerTypes.SingleOrDefault(lt => lt.Name == InventoryConstants.OrderLedgerType);
         if (ledgerType != null) ViewBag.LedgerType = ledgerType.LedgerTypeId;

         return PartialView("_DeductItem", consumeStock);
      }

      public JsonResult GetSerialNumbers(int? productId, int locationId)
      {
         if (!productId.HasValue)
         {
            return Json(string.Empty, JsonRequestBehavior.AllowGet);
         }

         var serialNumberList = _stockRepository.GetSerialNumbers(productId.Value, locationId).Select(sn => new SelectListItem
         {
            Text = sn,
            Value = sn
         }).ToList();
         return Json(serialNumberList, JsonRequestBehavior.AllowGet);
      }

      public ActionResult ReallocateStock(int id)
      {
         var location = _unitOfWork.StockLocationRepository.Find(id);
         ViewBag.UserPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_preferenceRepository,
            _propertyProvider, User.Identity);
         return View(location);
      }

      [HttpPost]
      public JsonResult GetStockForLocation([DataSourceRequest] DataSourceRequest request, int locationId)
      {
         var availableStock = _stockRepository.GetAvailableStock(locationId);
         return Json(availableStock.Select(Mapper.Map<Stock, LocationStockModel>).ToDataSourceResult(request));
      }

      public ActionResult NegativeStock()
      {
         ViewBag.UserPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_preferenceRepository, _propertyProvider, User.Identity);
         return View();
      }

      public ActionResult ReadNegativeStock([DataSourceRequest] DataSourceRequest request)
      {
         var negativeStock = (from ns in _stockRepository.GetNegativeStock()
                              join p in _productRepository.FindAll() on ns.ProductId equals p.ProductId
                              join l in _unitOfWork.StockLocationRepository.FindAll() on ns.StoredAt equals l.LocationId
                              select new NegativeStockViewModel
                              {
                                 Description = p.Description,
                                 Location = l.Name,
                                 ProductId = ns.ProductId,
                                 Quantity = ns.NegativeQuantity.Value,
                                 SPC = p.SPC,
                                 StoredAt = ns.StoredAt,
                              }).ToList();

         return Json(negativeStock.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
      }

      public ActionResult GetStockLevels([DataSourceRequest] DataSourceRequest request)
      {
         try
         {
            int pid;
            var productId =
               Int32.TryParse(
                  GetFilterValue(request.Filters, Nameof<IndexStockViewModel>.Property(p => p.ProductId)).ToString(),
                  out pid)
                  ? pid
                  : (int?)null;

            var stock = _unitOfWork.GetAvailableProductStockLevelByLocation(productId).Select(Mapper.Map<StockLevel, IndexStockViewModel>);

            var negativeStock = _stockRepository.GetNegativeStock();
            if (productId.HasValue)
            {
               negativeStock = negativeStock.Where(ns => ns.ProductId == productId.Value);
            }

            var negativeStockModels = (from ns in negativeStock
                                       join sl in _unitOfWork.StockLocationRepository.FindAll() on ns.StoredAt equals sl.LocationId
                                       select new IndexStockViewModel
                                       {
                                          LocationId = ns.StoredAt,
                                          ProductId = ns.ProductId,
                                          StockCount = -ns.NegativeQuantity.Value,
                                          StorageLocation = sl.Name,
                                       }).ToList();

            var allStockLevels = stock.Union(negativeStockModels);
            return Json(allStockLevels.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
         }
         catch (Exception ex)
         {
            _logger.Error(ex, "An error occurred while attempting to request stock levels");
            return Json(null, JsonRequestBehavior.AllowGet);
         }
      }

      //http://www.telerik.com/forums/how-to-access-datasourcerequest-filters-in-controller-
      private object GetFilterValue(IEnumerable<IFilterDescriptor> filters, string fieldName)
      {
         var filterDescriptors = filters as IList<IFilterDescriptor> ?? filters.ToList();
         if (!filterDescriptors.Any()) return null;
         foreach (var filter in filterDescriptors)
         {
            var descriptor = filter as FilterDescriptor;
            if (descriptor != null && descriptor.Member == fieldName)
            {
               return descriptor.Value;
            }
            var filterDescriptor = filter as CompositeFilterDescriptor;
            if (filterDescriptor != null)
            {
               return GetFilterValue(filterDescriptor.FilterDescriptors, fieldName);
            }
         }
         return null;
      }

      public ActionResult ReceivedStock()
      {
         var newStockAdded = TempData["newStock"];
         if (newStockAdded != null)
         {
            ViewBag.NewStock = newStockAdded;
         }
         return View();
      }

      public ActionResult GetReceivedStock([DataSourceRequest] DataSourceRequest request)
      {
         var receivedAdjustments = _unitOfWork.StockAdjustmentRepository.GetStockReceipts();
         var models = receivedAdjustments.Select(Mapper.Map<StockAdjustment, ReceivedStockViewModel>);
         return Json(models.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
      }

      [HttpPost]
      public ActionResult GetStockForOrderItem([DataSourceRequest] DataSourceRequest request, int id)
      {
         try
         {
            var stockReceipts =
               _unitOfWork.StockAdjustmentRepository.FindAllPositiveAdjustments().Where(sa => sa.OrderItemId == id);
            var stockReceiptModels = stockReceipts.Select(Mapper.Map<StockAdjustment, AdjustStockModel>);
            return Json(stockReceiptModels.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving stock adjustments for an order item."));
            return Json(new DataSourceResult(), JsonRequestBehavior.AllowGet);
         }
      }
   }
}