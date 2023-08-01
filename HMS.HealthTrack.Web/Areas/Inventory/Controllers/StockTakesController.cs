using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakeItems;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class StockTakesController : Controller
   {
      private readonly IStockTakeRepository _stockTakeRepository;
      private readonly IProductRepository _productRepository;
      private readonly IStockTakeUnitOfWork _unitOfWork;
      private readonly IPropertyProvider _propertyProvider;
      private readonly ICustomLogger _logger;

      public StockTakesController(IStockTakeUnitOfWork stockTakeUnitOfWork,
         IPropertyProvider propertyProvider,
         ICustomLogger logger)
      {
         _unitOfWork = stockTakeUnitOfWork;
         _stockTakeRepository = stockTakeUnitOfWork.StockTakeRepository;
         _productRepository = stockTakeUnitOfWork.ProductRepository;
         _propertyProvider = propertyProvider;
         _logger = logger;
      }

      // GET: Inventory/StockTake
      public ActionResult Index()
      {
         return View();
      }

      // GET: Inventory/StockTake
      public ActionResult New()
      {
         var userPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_unitOfWork.PreferenceRepository,
            _propertyProvider, User.Identity);
         ViewBag.UserPreferredLocation = userPreferredLocation;

         var newStockTake = new StockTake
         {
            CreatedBy = User.Identity.Name,
            CreatedOn = DateTime.Now,
            LocationId = userPreferredLocation,
            ModifiedBy = User.Identity.Name,
            ModifiedOn = DateTime.Now,
            StockTakeDate = DateTime.Now,
         };
         _stockTakeRepository.Add(newStockTake);

         _unitOfWork.Commit();

         return RedirectToAction("Edit", "StockTakes", new { id = newStockTake.StockTakeId });
      }

      // GET: Inventory/StockTake
      public ActionResult Edit(int? id)
      {
         if (!id.HasValue) return HttpNotFound("Please specify the stock take number");
         ViewBag.UserPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_unitOfWork.PreferenceRepository,
            _propertyProvider, User.Identity);
         var stockTake = _stockTakeRepository.Fetch(id.Value);
         return View(Mapper.Map<StockTake, StockTakeViewModel>(stockTake));
      }

      [HttpGet]
      public ActionResult Submit(int? stockTakeId)
      {
         if (!stockTakeId.HasValue) return HttpNotFound("Please specify a stock take");

         //Show what is going to be processed
         var stockTake = Mapper.Map<StockTake, StockTakeViewModel>(_stockTakeRepository.Fetch(stockTakeId.Value));
         if (stockTake.MissingStockLevel > 0)
         {
            stockTake.Message = stockTake.MissingStockLevel +
                                " item/s are missing a stock level and will not be processed.";
         }
         return View(stockTake);
      }

      [HttpGet]
      public async Task<ActionResult> GetSubmissionConfirmation(int id)
      {
         var stockTake = _stockTakeRepository.Fetch(id);
         return PartialView("_SubmissionConfirmation", Mapper.Map<StockTake, StockTakeViewModel>(stockTake));
      }

      [HttpPost]
      public void Delete(StockTakeViewModel stockTakeViewModel)
      {
         _stockTakeRepository.Remove(stockTakeViewModel.StockTakeId, User.Identity.Name);
         _unitOfWork.Commit();
      }

      [HttpPost]
      public JsonResult GetAll([DataSourceRequest] DataSourceRequest request)
      {
         return
            Json(
               _stockTakeRepository.FetchUserStockTakes()
                  .Select(Mapper.Map<StockTake, StockTakeViewModel>)
                  .ToDataSourceResult(request));
      }

      public JsonResult UpnSearch(string text)
      {
         if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
            return Json(new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "") },
               JsonRequestBehavior.AllowGet);

         List<KeyValuePair<int, string>> results = _productRepository.UpnSearch(text);

         return Json(results, JsonRequestBehavior.AllowGet);
      }

      public JsonResult SpcSearch(string text)
      {
         if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
            return Json(new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(0, "") },
               JsonRequestBehavior.AllowGet);

         List<KeyValuePair<int, string>> results = _productRepository.SpcSearch(text);

         return Json(results, JsonRequestBehavior.AllowGet);
      }

      public string GetDescription(int id)
      {
         return _productRepository.GetDescription(id);
      }

      public JsonResult GetDetails(int id)
      {
         var product = _productRepository.Find(id);
         if (product == null)
         {
            return Json(new { }, JsonRequestBehavior.AllowGet);
         }

         return Json(new { product.Description, product.SPC }, JsonRequestBehavior.AllowGet);
      }

      public ActionResult View(int id)
      {
         var stockTake = _stockTakeRepository.Fetch(id);
         return View(Mapper.Map<StockTake, StockTakeViewModel>(stockTake));
      }

      [HttpGet]
      public ActionResult CreateList()
      {
         var selectList = new List<SelectListItem>();
         foreach (var value in Enum.GetValues(typeof(StockTakeProductFilter)))
         {
            selectList.Add(new SelectListItem
            {
               Text = HelperMethods.GetEnumDisplayName<StockTakeProductFilter>((StockTakeProductFilter)value),
               Value = ((int)(StockTakeProductFilter)value).ToString()
            });
         }
         ViewBag.StockTakeProductFilters = selectList;
         ViewBag.UserPreferredLocation = PreferenceHelper.GetPreferredStockLocation(_unitOfWork.PreferenceRepository,
            _propertyProvider, User.Identity);

         var productLocations = _unitOfWork.StockLocationRepository.FindAll().Select(l => new SelectListItem
         {
            Text = l.Name,
            Value = l.LocationId.ToString()
         }).ToList();
         productLocations.Insert(0, new SelectListItem()
         {
            Text = "All locations",
            Value = "0"
         });

         ViewBag.ProductLocations = productLocations;

         return View();
      }

      [HttpGet]
      public ActionResult PrintStockTakeList(int stockTakeId)
      {
         var stockTake = _stockTakeRepository.Fetch(stockTakeId);
         var stockTakeListItems = stockTake.StockTakeItems.Where(sti => !sti.DeletedOn.HasValue).Select(Mapper.Map<StockTakeItem, StockTakeListItem>).ToList();
         var stockTakeList = new StockTakeList
         {
            CreatedOn = stockTake.CreatedOn,
            ListItems = stockTakeListItems,
            LocationName = stockTake.Location.Name,
            Name = stockTake.Name,
            StockTakeDate = stockTake.StockTakeDate,
            StockTakeId = stockTakeId,
            StockTakeStatus = stockTake.Status
         };
         return View(stockTakeList);
      }

      [HttpPost]
      public ActionResult StockTakeList(int stockTakeId, StockTakeItemDTO[] items)
      {
         if (items == null || items.Length == 0)
         {
            ModelState.AddModelError("", "No stock take items present in request");
            return View("Error",
               new HandleErrorInfo(new Exception("No stock take items present in request"), "StockTakes",
                  "StockTakeList"));
         }
         _logger.Information("Updating stock levels for {StockTakeItemCount} stock take items", items.Length);

         _stockTakeRepository.UpdateItemStockLevels(stockTakeId,
            items.ToDictionary(i => i.StockTakeItemId, i => i.StockLevel), User.Identity.Name);
         _unitOfWork.Commit();

         return RedirectToAction("Submit", new { id = stockTakeId });
      }

      [HttpGet]
      public ActionResult CreateStockTakeListForNegativeStock()
      {
         var negativeStockAtLocations = _unitOfWork.StockRepository.GetNegativeStock().GroupBy(g => g.StoredAt).ToList();
         if (negativeStockAtLocations.Any())
         {
            var stockTakes = negativeStockAtLocations.Select(stockAtLocation => CreateStockTake(stockAtLocation.Select(s => s.ProductId).ToList(), stockAtLocation.Key)).ToList();
            _unitOfWork.Commit();
            var stockTakeLocationIds = stockTakes.Select(s => s.LocationId).ToList();
            var locations =
               _unitOfWork.StockLocationRepository.FindAll()
                  .Where(l => stockTakeLocationIds.Contains(l.LocationId))
                  .ToList();

            var stockTakeLocations = (from st in stockTakes
                                      join l in locations on st.LocationId equals l.LocationId
                                      select new
                                      {
                                         st.StockTakeId,
                                         l.Name
                                      }).ToList();
            return Json(new { stockTakeLocations }, JsonRequestBehavior.AllowGet);
         }
         return Json(new { }, JsonRequestBehavior.AllowGet);
      }

      [HttpPost]
      public ActionResult StockTakeListForProducts(int[] productIds, int locationId)
      {
         _logger.Information("Attempting to create stock take for Location {LocationId} and products {ProductsIds}",
            locationId, productIds);
         var location = _unitOfWork.StockLocationRepository.Find(locationId);
         if (location == null)
         {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Location does not exist", JsonRequestBehavior.AllowGet);
         }

         if (productIds != null && productIds.Length > 0)
         {
            var products = _productRepository.FindAll().Where(p => productIds.Contains(p.ProductId));
            var stockTake = CreateStockTake(products.Select(p => p.ProductId).ToList(), location.LocationId);
            _unitOfWork.Commit();
            return Json(stockTake.StockTakeId, JsonRequestBehavior.AllowGet);
         }

         Response.StatusCode = (int)HttpStatusCode.BadRequest;
         return Json("Invalid products selected", JsonRequestBehavior.AllowGet);
      }

      [HttpPost]
      public ActionResult StockTakeListForProductFilter(StockTakeProductFilter productFilter, int locationId,
         int? filterLocationId)
      {
         _logger.Information(
            "Attempting to create stock take for Location {LocationId} and products filter {ProductFilter}",
            locationId, productFilter);
         var location = _unitOfWork.StockLocationRepository.Find(locationId);
         if (location == null)
         {
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Location does not exist", JsonRequestBehavior.AllowGet);
         }
         var products = _productRepository.GetStockTakeProducts(productFilter, filterLocationId);
         var stockTake = CreateStockTake(products.Select(p => p.ProductId).ToList(), locationId);
         _unitOfWork.Commit();
         return Json(stockTake.StockTakeId, JsonRequestBehavior.AllowGet);
      }

      private StockTake CreateStockTake(IList<int> productIds, int locationId)
      {
         var now = DateTime.Now;
         var user = User.Identity.Name;
         var stockTake = new StockTake()
         {
            CreatedBy = user,
            CreatedOn = now,
            ModifiedBy = user,
            ModifiedOn = now,
            LocationId = locationId,
            StockTakeDate = now,
         };

         stockTake.StockTakeItems.AddRange(productIds.ToList().Select(p => new StockTakeItem
         {
            CreatedBy = user,
            CreatedOn = now,
            ModifiedBy = user,
            ModifiedOn = now,
            ProductId = p,
            Status = StockTakeItemStatus.Created,
         }));

         _stockTakeRepository.Add(stockTake);
         return stockTake;
      }

      [HttpGet]
      public ActionResult StockTakeLocationSelection()
      {
         return PartialView("_LocationSelection");
      }

      [HttpPost]
      public ActionResult UpdateStockTakeDetails(int stockTakeId, int stockTakeLocationId, DateTime stockTakeDate, string name)
      {
         _logger.Information("Attempting to update stock take {StockTakeId} with location {LocationId} and {StockTakeName}", stockTakeId,
            stockTakeLocationId, name);

         var stockTake = _stockTakeRepository.Fetch(stockTakeId);
         string message;
         if (stockTake == null)
         {
            _logger.Warning("Failed to update stock take {StockTakeId} - No stock take exists", stockTakeId);
            message = "Stock take does not exist.";
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(message, JsonRequestBehavior.AllowGet);
         }

         var location = _unitOfWork.StockLocationRepository.Find(stockTakeLocationId);
         if (location == null)
         {
            _logger.Warning("Failed to update stock takes {StockTakeId} - Location {LocationId} does not exist",
               stockTakeId, stockTakeLocationId);
            message = "Location does not exist.";
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(message, JsonRequestBehavior.AllowGet);
         }

         _stockTakeRepository.UpdateStockTakeDetails(stockTakeId, stockTakeLocationId, stockTakeDate, name, User.Identity.Name);
         _unitOfWork.Commit();
         message = "Name, location and stock take date saved successfully.";

         return Json(new { message }, JsonRequestBehavior.AllowGet);
      }

      public ActionResult StockTakeForSingleProduct()
      {
         return View("_StockTakeProduct");
      }
   }
}