using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockLocations;
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
   public class StockLocationsController : Controller
   {
      private readonly ILocationUnitOfWork _unitOfWork;
      private readonly IStockLocationRepository _stockLocationRepository;
      private readonly ICustomLogger _logger;
      private readonly IPreferenceRepository _preferenceRepository;
      private readonly IPropertyProvider _propertyProvider;

      public StockLocationsController(ILocationUnitOfWork unitOfWork, ICustomLogger logger, IPreferenceRepository preferenceRepository, IPropertyProvider propertyProvider)
      {
         _unitOfWork = unitOfWork;
         _stockLocationRepository = unitOfWork.StockLocationRepository;
         _logger = logger;
         _preferenceRepository = preferenceRepository;
         _propertyProvider = propertyProvider;
      }

      // GET: Inventory/Inventory_Location
      public ActionResult Index()
      {
         return View();
      }

      // GET: Inventory/Inventory_Location/Details/5
      public ActionResult Details(int? id)
      {
         try
         {
            if (id == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var location = _unitOfWork.GetStockLocation(id.Value);
            if (location == null)
            {
               return HttpNotFound();
            }
            var stockLocation = Mapper.Map<StockLocation, StockLocationViewModel>(location.StockLocation);
            stockLocation.SetMappedLocationNames(location.HealthTrackLocations);

            return View(stockLocation);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Location with ID '{0}'.", id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "StockLocations", "Details"));
         }
      }

      // GET: Inventory/Inventory_Location/Create
      public ActionResult Create()
      {
         return View();
      }

      // POST: Inventory/Inventory_Location/Create
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Create(StockLocationViewModel location)
      {
         try
         {
            if (ModelState.IsValid)
            {
               var stockLocation = Mapper.Map<StockLocationViewModel, StockLocation>(location);
               stockLocation.CreatedBy = User.Identity.Name;
               stockLocation.LastModifiedUser = User.Identity.Name;
               if (location.HealthTrackLocations != null && location.HealthTrackLocations.Any())
               {
                  foreach (var htId in location.HealthTrackLocations)
                  {
                     stockLocation.StockLocationMappings.Add(new StockLocationMapping
                     {
                        HealthTrackLocationId = htId
                     });
                  }
               }

               _stockLocationRepository.Add(stockLocation);
               _unitOfWork.Commit();
               return RedirectToAction("Index");
            }

            return View(location);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem creating a Location with the name '{0}'.", location.Name));
            return View("Error", new HandleErrorInfo(exception, "StockLocations", "Create"));
         }
      }

      // GET: Inventory/Inventory_Location/Edit/5
      public ActionResult Edit(int? id)
      {
         try
         {
            if (id == null)
            {
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //var location = _stockLocationRepository.Find(id.Value);
            var location = _unitOfWork.GetStockLocation(id.Value);
            if (location == null)
            {
               return HttpNotFound();
            }

            var model = Mapper.Map<StockLocation, StockLocationViewModel>(location.StockLocation);
            model.SetMappedLocationNames(location.HealthTrackLocations);
            return View(model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Location with ID '{0}'.", id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "StockLocations", "Edit"));
         }
      }

      // POST: Inventory/Inventory_Location/Edit/5
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit(StockLocationViewModel location)
      {
         try
         {
            if (ModelState.IsValid)
            {
               var stockLocation = Mapper.Map<StockLocationViewModel, StockLocation>(location);
               if (location.HealthTrackLocations != null && location.HealthTrackLocations.Any())
               {
                  foreach (var htId in location.HealthTrackLocations)
                  {
                     stockLocation.StockLocationMappings.Add(new StockLocationMapping
                     {
                        HealthTrackLocationId = htId
                     });
                  }
               }

               stockLocation.LastModifiedUser = User.Identity.Name;

               _stockLocationRepository.Update(stockLocation);
               _unitOfWork.Commit();
               return RedirectToAction("Index");
            }
            return View(location);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem editing the Location with ID {0}.", location.LocationId));
            return View("Error", new HandleErrorInfo(exception, "StockLocations", "Edit"));
         }
      }

      // POST: Inventory/Inventory_Location/Delete/5
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
         try
         {
            var location = _stockLocationRepository.Find(id);
            location.DeletedBy = User.Identity.Name;
            _stockLocationRepository.Remove(location);
            _unitOfWork.Commit();
            return RedirectToAction("Index");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem deleting the Location with ID {0}.", id));
            return View("Error", new HandleErrorInfo(exception, "StockLocations", "Delete"));
         }
      }

      public JsonResult GetStockLocations(string text)
      {
         try
         {
            var locations =
               _stockLocationRepository.FindAll();
            if (!string.IsNullOrWhiteSpace(text))
            {
               locations = locations.Where(sl => sl.Name.Contains(text));
            }
            var result = locations.AsEnumerable()
                  .Select(sl => new SelectListItem { Value = sl.LocationId.ToString(), Text = sl.Name }).OrderBy(sl => sl.Text);
            return Json(result, JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Stock Locations with the text '{0}'.", text));
            return Json(null);
         }
      }

      public JsonResult GetDeliveryLocations(string text)
      {
         try
         {
            var locations =
               _stockLocationRepository.FindDeliveryLocations().Where(sl => sl.Name.Contains(text)).Take(10)
                  .AsEnumerable()
                  .Select(sl => new SelectListItem { Value = sl.LocationId.ToString(), Text = sl.Name });
            return Json(locations, JsonRequestBehavior.AllowGet);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving Stock Locations with the text '{0}'.", text));
            return Json(null);
         }
      }
   }
}
