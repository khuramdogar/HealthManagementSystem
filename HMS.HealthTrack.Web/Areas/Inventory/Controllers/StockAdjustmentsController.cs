using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Stock;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockAdjustment;
using HMS.HealthTrack.Web.Utils;
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
   public class StockAdjustmentsController : Controller
   {
      private readonly IStockAdjustmentUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;
      private readonly IPreferenceRepository _preferenceRepository;
      private readonly IProductRepository _productRepository;
      private readonly IStockRepository _stockRepository;
      private readonly IStockAdjustmentRepository _stockAdjustmentRepository;
      private readonly StockAdjustmentHelper _stockDeductionHelper;

      public StockAdjustmentsController(IStockAdjustmentUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider,
         IPreferenceRepository preferenceRepository)
      {
         _unitOfWork = unitOfWork;
         _productRepository = _unitOfWork.ProductRepository;
         _logger = logger;
         _propertyProvider = propertyProvider;
         _preferenceRepository = preferenceRepository;
         _stockRepository = unitOfWork.StockRepository;
         _stockAdjustmentRepository = unitOfWork.StockAdjustmentRepository;
         _stockDeductionHelper = new StockAdjustmentHelper(_stockRepository, _stockAdjustmentRepository);
      }

      [HttpGet]
      public ActionResult Deductions()
      {
         ViewBag.DeductedStock = TempData["deductedStock"];
         return View("Deductions");
      }

      [HttpGet]
      public JsonResult GetReasons()
      {
         return
            Json(
               _unitOfWork.StockAdjustmentRepository.GetReasons()
                  .Where(r => !r.Disabled && !r.IsSystemReason)
                  .Select(r => new { r.StockAdjustmentReasonId, r.Name }), JsonRequestBehavior.AllowGet);
      }

      [HttpGet]
      public ActionResult DeductStock(int? productId)
      {
         return ReturnToDeductStock(new DeductStock { ProductId = productId });
      }

      public ActionResult ReturnToDeductStock(DeductStock model)
      {
         var ledgerType = _propertyProvider.LedgerTypes.SingleOrDefault(lt => lt.Name == InventoryConstants.OrderLedgerType);
         if (ledgerType != null) ViewBag.LedgerType = ledgerType.LedgerTypeId;
         ViewBag.UserPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_preferenceRepository, _propertyProvider, User.Identity);
         return View("DeductStock", model);
      }

      [HttpPost]
      public ActionResult DeductStock(DeductStock stockDeduction)
      {
         if (!ModelState.IsValid)
            return ReturnToDeductStock(stockDeduction);

         try
         {
            //Check the product exists
            if (!stockDeduction.ProductId.HasValue || !_productRepository.Exists(stockDeduction.ProductId.Value))
            {
               ModelState.AddModelError("", "Failed to find product " + stockDeduction.ProductId);
               return ReturnToDeductStock(stockDeduction);
            }

            var adjustment = Mapper.Map<DeductStock, ItemAdjustment>(stockDeduction);
            adjustment.Source = AdjustmentSource.Web;

            var productInStock = _stockRepository.HasStock(adjustment.ProductId, adjustment.StockLocationId.Value);
            var allStockHasSerialNumbers = !_stockRepository.StockWithoutSerialExists(adjustment.ProductId, adjustment.StockLocationId);

            if (adjustment.HasSerial)
            {
               // Quantity must be 1 when specifying a Serial Number
               if (adjustment.Quantity != 1)
               {
                  ModelState.AddModelError("Quantity", " ");
                  ModelState.AddModelError("SerialNumber", " ");
                  ModelState.AddModelError("", "You cannot input a serial number and have multiple quantities");
                  return View(stockDeduction);
               }

               if (productInStock)
               {
                  var serialInStock = _stockRepository.SerialInStock(adjustment.ProductId, adjustment.SerialNumber);
                  // User must choose a serial if there is no blank stock and the serial number specified is not in stock
                  if (allStockHasSerialNumbers && !serialInStock)
                  {
                     ModelState.AddModelError("SerialNumber", " ");
                     ModelState.AddModelError("", "There is no stock for the specified product and serial number.");
                     return ReturnToDeductStock(stockDeduction);
                  }

                  if (serialInStock)
                  {
                     var stockItem =
                        _stockRepository.FindAll()
                           .Single(
                              s => s.ProductId == adjustment.ProductId && s.SerialNumber == adjustment.SerialNumber &&
                                   s.StockStatus == StockStatus.Available);
                     // Get user to confirm consumption at location
                     if (!stockDeduction.OverrideLocation)
                     {
                        // serial stored at different location
                        if (stockItem.StoredAt != adjustment.StockLocationId)
                        {
                           ViewBag.LocationError = stockItem.StoredAt;
                           ViewBag.OriginalLocationName = stockItem.StorageLocation.Name;
                           return ReturnToDeductStock(stockDeduction);
                        }
                     }
                     // User has confirmed consumption location, update stock location if necessary
                     else
                     {
                        if (stockItem.StoredAt != adjustment.StockLocationId)
                        {
                           _stockRepository.UpdateStockLocation(stockItem, adjustment.StockLocationId.Value, User.Identity.Name);
                           _stockRepository.Commit();
                        }
                     }
                  }
               }
            }

            if (!adjustment.HasSerial && productInStock && adjustment.Quantity == 1)
            {
               // User must choose serial if attempting to consume 1 item and all stock has serial numbers
               if (allStockHasSerialNumbers)
               {
                  ModelState.AddModelError("SerialNumber", " ");
                  ModelState.AddModelError("",
                     "There is no stock for the specified product and location with a blank serial number. Please choose one from the list.");
                  return ReturnToDeductStock(stockDeduction);
               }
            }

            //Mark as used
            try
            {
               var product = _productRepository.Find(adjustment.ProductId);
               _stockDeductionHelper.AdjustItem(adjustment, product, User.Identity.Name);
               _unitOfWork.Commit();
               TempData["deductedStock"] =
                  _productRepository.Find(adjustment.ProductId).Description;
               return RedirectToAction("Deductions");
            }
            catch (Exception exception)
            {
               _logger.Error(exception,
                  "There was a problem deducting stock for {ProductId} with a quantity of {Quantity} and, if present, a serial number {SerialNumber}.",
                  stockDeduction.ProductId.ToString(),
                  stockDeduction.Quantity.HasValue ? stockDeduction.Quantity.Value.ToString() : string.Empty,
                  stockDeduction.SerialNumber);
               if (exception.GetType().IsSubclassOf(typeof(StockException)))
               {
                  ModelState.AddModelError("", exception.Message);
               }
               else
               {
                  ModelState.AddModelError("", "There was a problem consuming this item: " + exception.Message);
               }

               return ReturnToDeductStock(stockDeduction);
            }
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               "There was a problem consuming stock for {ProductId} with a quantity of {Quantity} and, if present, a serial number {SerialNumber}.",
               stockDeduction.ProductId.HasValue ? stockDeduction.ProductId.ToString() : string.Empty,
               stockDeduction.Quantity.HasValue ? stockDeduction.Quantity.Value.ToString() : string.Empty,
               stockDeduction.SerialNumber);
            return View("Error", new HandleErrorInfo(exception, "StockAdjustments", "DeductStock"));
         }
      }

      // POST: Inventory/StockDeductions/Delete/5
      [HttpPost, ActionName("Delete")]
      public ActionResult DeleteConfirmed(int id)
      {
         try
         {
            var adjustment = _stockAdjustmentRepository.FindDeduction(id);
            adjustment.DeletedBy = User.Identity.Name;
            _stockAdjustmentRepository.Remove(adjustment);
            _stockAdjustmentRepository.Commit();
            return RedirectToAction("Deductions");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem deleting the Stock Adjustment with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "StockAdjustments", "Delete"));
         }
      }

      [HttpGet]
      public ActionResult RequirePaymentClass()
      {
         return View("MissingPaymentClass");
      }

      [HttpPost]
      public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
      {
         try
         {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem exporting an excel spreadsheet with the file name '{0}'.", fileName));
            return View("Error", new HandleErrorInfo(exception, "StockAdjustments", "Excel_Export_Save"));
         }
      }

      public ActionResult MissingSerialNumber()
      {
         return View();
      }

      public ActionResult MissingLotNumber()
      {
         return View();
      }

      public ActionResult MissingPatients()
      {
         return View();
      }

      [HttpPost]
      public JsonResult MissingSerialNumber([DataSourceRequest] DataSourceRequest request)
      {
         var items = _stockAdjustmentRepository.FindDeductionsMissingSerial();
         var viewModel = items.Select(Mapper.Map<StockAdjustment, IndexStockAdjustmentsViewModel>);
         return Json(viewModel.ToDataSourceResult(request));
      }

      [HttpPost]
      public JsonResult MissingLotNumber([DataSourceRequest] DataSourceRequest request)
      {
         IQueryable<StockAdjustment> items = _stockAdjustmentRepository.FindDeductionsMissingLot();
         var viewModel = items.Select(Mapper.Map<StockAdjustment, IndexStockAdjustmentsViewModel>);
         return Json(viewModel.ToDataSourceResult(request));
      }

      [HttpPost]
      public JsonResult MissingPatients([DataSourceRequest] DataSourceRequest request)
      {
         IQueryable<StockAdjustment> items = _stockAdjustmentRepository.FindDeductionsMissingPatients();
         var viewModel = items.Select(Mapper.Map<StockAdjustment, IndexStockAdjustmentsViewModel>);
         return Json(viewModel.ToDataSourceResult(request));
      }

      public ActionResult Details(int id)
      {
         try
         {
            var adjustment = _stockAdjustmentRepository.FindDeduction(id);
            var result = Mapper.Map<StockAdjustment, StockAdjustmentDetailsViewModel>(adjustment);
            return View(result);
         }
         catch (Exception exception)
         {
            var message = "There was a problem retrieving the Stock Adjustment";
            _logger.Error(exception, message);
            var result = new ContentResult { Content = message };
            return result;
         }
      }

      public ActionResult AdditionalDetails(int stockAdjustmentId)
      {
         try
         {
            var adjustment = _stockAdjustmentRepository.FindDeduction(stockAdjustmentId);
            var result = Mapper.Map<StockAdjustment, StockAdjustmentDetailsViewModel>(adjustment);
            return View(result);
         }
         catch (Exception exception)
         {
            var message = "There was a problem retrieving details for the Stock Adjustment";
            _logger.Error(exception, message);
            var result = new ContentResult { Content = message };
            return result;
         }
      }

      public ActionResult GetNegativeDeductions([DataSourceRequest] DataSourceRequest request, int productId,
         int locationId)
      {
         try
         {
            var deductions = _unitOfWork.FindDeductionsForNegativeStockLevel(productId, locationId).ToList();
            var models = deductions.Select(Mapper.Map<StockAdjustment, SimpleStockAdjustmentDetailsViewModel>);
            return Json(models.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               "Unabled to fetch the negative stock deductions for the products {ProductId} at location {LocationId}",
               productId, locationId);
            return Json(new DataSourceResult(), JsonRequestBehavior.AllowGet);
         }
      }

      [HttpPost]
      public ActionResult AdjustStockForOrderItem(AdjustStockDTO[] adjustments)
      {
         try
         {
            foreach (var adjustment in adjustments)
            {
               var stockAdjustment =
                  _unitOfWork.StockAdjustmentRepository.FindPositiveAdjustment(adjustment.StockAdjustmentId);
               if (stockAdjustment == null)
               {
                  _logger.Warning("Unable to find stock adjustment with ID {StockAdjustmentId} to adjust", adjustment.StockAdjustmentId);
                  return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot find the entry to adjust");
               }

               _unitOfWork.AdjustReceivedStock(stockAdjustment, adjustment.ReceivedQuantity, User.Identity.Name);
            }
            _stockRepository.Commit();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem adjusting Receieved Quantities."));
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "There was a problem updating the Receieved Quantities");
         }
      }

      [HttpGet]
      public ActionResult StockDeductions([DataSourceRequest] DataSourceRequest request)
      {
         try
         {
            var stockDeduction = _stockAdjustmentRepository.FindAllDeductions();
            var deduction = stockDeduction == null
               ? new List<IndexStockAdjustmentsViewModel>()
               : stockDeduction.Select(Mapper.Map<StockAdjustment, IndexStockAdjustmentsViewModel>);
            return Json(deduction.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem retrieving Stock Deductions.");
            return Json(new DataSourceResult(), JsonRequestBehavior.AllowGet);
         }
      }
   }
}