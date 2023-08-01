using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockRequests;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
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
   public class StockRequestsController : Controller
   {
      private readonly IStockUnitOfWork _unitOfWork;

      private readonly IStockRequestRepository _stockRequestRepository;
      private readonly ICompanyRepository _companyRepository;
      private readonly IProductRepository _productRepository;
      private readonly ICustomLogger _logger;
      private readonly IPreferenceRepository _preferenceRepository;
      private readonly IPropertyProvider _propertyProvider;

      public StockRequestsController(IStockUnitOfWork unitOfWork, ICustomLogger logger,
         IPreferenceRepository preferenceRepository, IPropertyProvider propertyProvider)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
         _preferenceRepository = preferenceRepository;
         _propertyProvider = propertyProvider;
         _stockRequestRepository = _unitOfWork.StockRequestRepository;
         _productRepository = _unitOfWork.ProductRepository;
      }

      // GET: Inventory/StockRequests
      public ActionResult Index()
      {
         return View();
      }

      // GET: Inventory/StockRequests/Details/5
      public ActionResult Details(int? id)
      {
         try
         {
            if (!id.HasValue)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var stockRequest = _stockRequestRepository.Find(id.Value);
            if (stockRequest == null)
               return HttpNotFound();

            return View(Mapper.Map<ProductStockRequest, DetailsProductStockRequestViewModel>(stockRequest));
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem retrieving the Stock Request with ID '{0}'.",
                  id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "StockRequests", "Details"));
         }
      }

      // GET: Inventory/StockRequests/Details/5
      public ActionResult History(int? id)
      {
         try
         {
            if (!id.HasValue)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var stockRequest = _stockRequestRepository.Find(id.Value);
            if (stockRequest == null)
               return HttpNotFound();

            return View(Mapper.Map<ProductStockRequest, DetailsProductStockRequestViewModel>(stockRequest));
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem retrieving the Stock Request with ID '{0}'.",
                  id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "StockRequests", "History"));
         }
      }

      // GET: Inventory/StockRequests/Create
      public ActionResult RequestProduct()
      {
         try
         {
            ViewBag.UserPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_preferenceRepository,
               _propertyProvider, User.Identity);
            return View();
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Stock Sets."));
            return View("Error", new HandleErrorInfo(exception, "StockRequests", "RequestProduct"));
         }
      }

      [HttpPost]
      public ActionResult RequestProduct(CreateStockRequestViewModel stockRequestItem)
      {
         try
         {
            if (!ModelState.IsValid)
               return View();

            if (stockRequestItem == null)
            {
               var message = string.Format("Invalid request. No data specified.");
               ModelState.AddModelError("", message);
               return View();
            }

            var product = _productRepository.Find(stockRequestItem.ProductId);

            if (product == null)
            {
               var message = string.Format("Could not find product {0}", stockRequestItem.ProductId);
               ModelState.AddModelError("", message);
               return View();
            }

            if (product.Unorderable)
            {
               ModelState.AddModelError("", "Cannot request a product marked as unorderable.");
               _logger.Warning("Attempting to request unorderable product {ProductId}", product.ProductId);
               return View();
            }

            if (stockRequestItem.Quantity < 1)
            {
               ModelState.AddModelError("", "Please set a valid quantity greater than zero.");
               return View();
            }

            if (stockRequestItem.Location < 1)
            {
               ModelState.AddModelError("", "Please set a valid location");
               return View();
            }

            _stockRequestRepository.CreateRequest(stockRequestItem.ProductId, stockRequestItem.Quantity,
               stockRequestItem.Location, stockRequestItem.IsUrgent, User.Identity.Name);
            _stockRequestRepository.Commit();

            return RedirectToAction("Index");
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               stockRequestItem == null
                  ? "No data specified for requesting product."
                  : string.Format(
                     "There was a problem requesting a Product with ID '{0}', for the location '{1}' and a quantity of '{2}'.",
                     stockRequestItem.ProductId, stockRequestItem.Location, stockRequestItem.Quantity));

            return View("Error", new HandleErrorInfo(exception, "StockRequests", "RequestProduct"));
         }
      }

      // POST: Inventory/StockRequests/Delete/5
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
         try
         {
            var stockRequest = _stockRequestRepository.Find(id);
            _stockRequestRepository.Remove(stockRequest);
            _stockRequestRepository.Commit();
            return RedirectToAction("Index");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem deleting the Stock Request with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "StockRequests", "Delete"));
         }
      }

      [HttpGet]
      public ActionResult RequestStockSet()
      {
         try
         {
            ViewBag.UserPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_preferenceRepository,
               _propertyProvider, User.Identity);
            return View();
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Stock Sets"));
            return View("Error", new HandleErrorInfo(exception, "StockRequests", "RequestStockSet"));
         }
      }

      public ActionResult Reject(int id)
      {
         try
         {
            var stockRequest = _stockRequestRepository.Find(id);
            if (stockRequest == null)
            {
               ModelState.AddModelError("", "Failed to find Stock Request");
               return RedirectToAction("Index");
            }

            stockRequest.LastModifiedBy = User.Identity.Name;
            stockRequest.LastModifiedOn = DateTime.Now;
            stockRequest.RequestStatus = RequestStatus.Rejected;
            _stockRequestRepository.Commit();

            return RedirectToAction("Index");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem rejecting the Stock Request with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "StockRequests", "Reject"));
         }
      }

      public ActionResult GetRequestStatuses()
      {
         var statusesToDisplay = (RequestStatus[])Enum.GetValues(typeof(RequestStatus));

         var statusList = statusesToDisplay.Select(status => new SelectListItem
         {
            Text = HelperMethods.GetEnumDisplayName<RequestStatus>(status),
            Value = ((int)status).ToString()
         }).ToList().OrderBy(o => o.Text);
         return Json(statusList, JsonRequestBehavior.AllowGet);
      }
   }
}