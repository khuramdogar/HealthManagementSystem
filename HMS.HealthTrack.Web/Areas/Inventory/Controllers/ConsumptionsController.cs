using AutoMapper;
using Hangfire;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Consumptions;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ConsumptionsController : Controller
   {
      private readonly IConsumptionUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;

      private readonly IInventoryUsedRepository _inventoryUsedRepository;
      private readonly IHealthTrackConsumptionRepository _healthTrackConsumptionRepository;

      private const string ConsumptionErrors = "ConsumptionErrors";
      private const string SuccessMessage = "SuccessMessage";

      public ConsumptionsController(IConsumptionUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider, IInventoryUsedRepository inventoryUsedRepository)
      {
         _unitOfWork = unitOfWork;
         _logger = logger;
         _propertyProvider = propertyProvider;
         _inventoryUsedRepository = inventoryUsedRepository;

         _healthTrackConsumptionRepository = unitOfWork.HealthTrackConsumptionRepository;
      }

      [HttpGet]
      public ActionResult ConsumptionReports()
      {
         return View();
      }
      [HttpGet]
      public ActionResult ConsumptionByPatient()
      {
         return View();
      }

      [HttpGet]
      public ActionResult Index()
      {
         return View();
      }

      // GET: Inventory/Consumption/Details/5
      public ActionResult Details(int id)
      {
         try
         {
            ViewBag.SuccessMessage = TempData[SuccessMessage];

            var consumption = _healthTrackConsumptionRepository.Find(id);
            if (consumption == null)
            {
               return HttpNotFound();
            }
            
            var model = Mapper.Map<HealthTrackConsumption, ConsumptionDetailsViewModel>(consumption);

            var mapping = _unitOfWork.ExternalProductMappingRepository.GetHealthTrackProductMapping(consumption.ProductId);
            if (mapping != null)
            {
               model.InventoryProductId = mapping.InventoryProductId;
               model.InventoryProductDescription = mapping.Product.Description;
            }

            return View(model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, $"There was a problem retrieving the Consumption Notification with ID '{id}'.");
            return View("Error", new HandleErrorInfo(exception, "Consumptions", "Details"));
         }
      }

      // GET: Inventory/Consumption/Edit/5
      public ActionResult Edit(int id)
      {
         try
         {
            if (TempData[ConsumptionErrors] is List<string> consumptionErrors && consumptionErrors.Any())
            {
               ViewBag.ConsumptionErrors = "Could not process this consumption due to the following errors";
               foreach (var consumptionError in consumptionErrors)
               {
                  ModelState.AddModelError("", consumptionError);
               }
            }
            var consumption = _healthTrackConsumptionRepository.Find(id);
            if (consumption == null)
               return new HttpNotFoundResult();

            return View(Mapper.Map<HealthTrackConsumption, ConsumptionDetailsViewModel>(consumption));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, $"There was a problem retrieving the Consumption Notification with ID '{id}'.");
            return View("Error", new HandleErrorInfo(exception, "Consumptions", "Edit"));
         }
      }

      [HttpPost]
      public ActionResult Edit(ConsumptionDetailsViewModel consumption)
      {
         try
         {
            if (ModelState.IsValid)
            {
               var consumptionUpdate = Mapper.Map<ConsumptionDetailsViewModel, HealthTrackConsumptionUpdate>(consumption);
               _inventoryUsedRepository.Update(consumptionUpdate, User.Identity.Name);
               _inventoryUsedRepository.Commit();

               var consumptionManagement = _healthTrackConsumptionRepository.FindConsumptionNotification(consumption.UsedId);
               consumptionManagement.ProcessingStatus = ConsumptionProcessingStatus.Unprocessed;
               consumptionManagement.ProcessingStatusMessage = string.Empty;
               _healthTrackConsumptionRepository.Commit();

               return RedirectToAction("Details", new { id = consumption.UsedId });
            }

            return View(consumption);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, $"There was a problem editing the Consumption Notification with ID '{consumption.UsedId}'");
            return View("Error", new HandleErrorInfo(exception, "Consumptions", "Edit"));
         }
      }

      public ActionResult AdditionalDetails(int consumptionId)
      {
         var consumption = _healthTrackConsumptionRepository.Find(consumptionId);
         var consumptionDetails = Mapper.Map<HealthTrackConsumption, ConsumptionDetailsViewModel>(consumption);
         if (consumption != null)
         {
            var mapping = _unitOfWork.ExternalProductMappingRepository.GetHealthTrackProductMapping(consumption.ProductId);

            //use product details where mapped
            if (mapping != null)
            {
               var product = _unitOfWork.ProductRepository.Find(mapping.InventoryProductId);

               consumptionDetails.InventoryProductDescription = product == null ? consumption.Description : product.Description;
               consumptionDetails.InventoryProductId = product?.ProductId;
            }
            else
            {
               consumptionDetails.InventoryProductDescription = consumption.Description;
            }
         }
         return View("AdditionalDetails", consumptionDetails);
      }

      // GET: Inventory/Consumption/Errors
      public ActionResult Errors()
      {
         ViewBag.Message = "Consumption processing errors";
         ViewBag.SuccessMessage = TempData[SuccessMessage];
         return View("Errors");
      }

      // GET: Inventory/Consumption/Unprocessed
      public ActionResult Unprocessed()
      {
         ViewBag.Message = "Consumption for products that have not been processing into stock yet";
         ViewBag.SuccessMessage = TempData[SuccessMessage];
         return View("Unprocessed");
      }

      #region API calls

      [HttpPost]
      public JsonResult GetAll([DataSourceRequest] DataSourceRequest request, List<int> categoryIds)
      {
         using (new CodeTimer("Consumptions.GetAll"))
         {
            IQueryable<HealthTrackConsumption> items;
            if (categoryIds == null || (categoryIds.Count == 1 && categoryIds.Single() == 0)) //Check for an invalid entry that javascript puts in for #noFilter
               items = _healthTrackConsumptionRepository.FindHealthTrackConsumptions();
            else
               items = _healthTrackConsumptionRepository.FindHealthTrackConsumptions(categoryIds);

            return Json(items.AsEnumerable().Select(Mapper.Map<HealthTrackConsumption, IndexConsumptionsViewModel>).ToDataSourceResult(request));
         }
      }

      [HttpPost]
      public JsonResult GetUnreported([DataSourceRequest] DataSourceRequest request, List<int> categoryIds)
      {
         IQueryable<HealthTrackConsumption> items;
         if (categoryIds == null || (categoryIds.Count == 1 && categoryIds.Single() == 0)) //Check for an invalid entry that javascript puts in for #noFilter
            items = _healthTrackConsumptionRepository.FindHealthTrackConsumptions();
         else
            items = _healthTrackConsumptionRepository.FindHealthTrackConsumptions(categoryIds);
         var results = from c in items.AsEnumerable().Select(Mapper.Map<HealthTrackConsumption, IndexConsumptionsViewModel>) where c.Reported == false select c;
         return Json(results.ToDataSourceResult(request));
      }

      [HttpPost]
      public JsonResult GetConsumptions([DataSourceRequest] DataSourceRequest request, ConsumptionProcessingStatus status)
      {
         var items = _healthTrackConsumptionRepository.FindHealthTrackConsumptions(status);
         var results = items.AsEnumerable().Select(Mapper.Map<HealthTrackConsumption, IndexConsumptionsViewModel>);
         return Json(results.ToDataSourceResult(request));
      }

      [HttpPost]
      public JsonResult GetConsumptionsForProcessing([DataSourceRequest] DataSourceRequest request)
      {
         var items =
            _healthTrackConsumptionRepository.FindHealthTrackConsumptions()
               .Where(
                  c =>
                     c.ProcessingStatus == ConsumptionProcessingStatus.Unprocessed);
         var results = items.AsEnumerable().Select(Mapper.Map<HealthTrackConsumption, IndexConsumptionsViewModel>);
         return Json(results.ToDataSourceResult(request));
      }

      [HttpPost]
      public JsonResult GetErroredConsumptions([DataSourceRequest] DataSourceRequest request)
      {
         var items =
            _healthTrackConsumptionRepository.FindHealthTrackConsumptions()
               .Where(
                  c =>
                     c.ProcessingStatus == ConsumptionProcessingStatus.Error);
         var results = items.AsEnumerable().Select(Mapper.Map<HealthTrackConsumption, IndexConsumptionsViewModel>);
         return Json(results.ToDataSourceResult(request));
      }

      [HttpPost]
      public void Reported(IndexConsumptionsViewModel model)
      {
         _healthTrackConsumptionRepository.MarkAsReported(model.ConsumptionId, User.Identity.Name);
         _healthTrackConsumptionRepository.Commit();
      }

      [HttpPost]
      public ActionResult IgnoreConsumption(IgnoreConsumptionModel consumption)
      {
         try
         {
            var stockConsumption = _healthTrackConsumptionRepository.Find(consumption.ConsumptionId);
            
            if (stockConsumption == null)
               return HttpNotFound();

            if (stockConsumption.ProcessingStatus == ConsumptionProcessingStatus.Processed)
               return Json("Can only ignore consumptions that have not been processed.");

            _healthTrackConsumptionRepository.IgnoreUnprocessed(consumption.ConsumptionId, User.Identity.Name);
            _healthTrackConsumptionRepository.Commit();

            return Json(
               new
               {
                  stockConsumption.Name,
                  stockConsumption.UsedId,
                  Status = Enum.GetName(typeof(ConsumptionProcessingStatus), stockConsumption.ProcessingStatus ?? throw new InvalidOperationException("Stock consumption processing status missing"))
               });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, $"There was a problem ignoring the Stock Consumption with ID '{consumption.ConsumptionId}'.");
            throw;
         }
      }


      [HttpGet]
      public JsonResult ProcessAllInventoryUsed()
      {
         //Kick off processing
         var jobToken = BackgroundJob.Enqueue<HealthTrackProductConsumptionProcessor>(x => x.ProcessAllInventoryUsed(User.Identity.Name));
         return Json(jobToken, JsonRequestBehavior.AllowGet);
      }

      #endregion

      public void Update(HealthTrackConsumptionUpdate model)
      {
         if (!ModelState.IsValid || model == null) return;

         try
         {
            _inventoryUsedRepository.Update(model, User.Identity.Name);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, $"There was a problem updating Stock Consumption with ID '{model.UsedId}'.");
            throw;
         }
      }

      public ActionResult ProcessConsumptionNotification(int id)
      {
         try
         {
            var errors = _inventoryUsedRepository.CheckConsumptionForErrors(id);
            if (errors.Any())
            {
               TempData[ConsumptionErrors] = errors;
               return RedirectToAction("Edit", new { id });
            }

            var stockProcessor = new HealthTrackProductConsumptionProcessor(_unitOfWork, _logger, _propertyProvider, _inventoryUsedRepository);
            stockProcessor.ProcessConsumptionNotification(id, User.Identity.Name);
            _unitOfWork.Commit();

            TempData[SuccessMessage] = $"Consumption {id} has been processed. Please check the status for any issues.";
            return RedirectToAction("Details", new { id });

         }
         catch (Exception exception)
         {
            _logger.Error(exception, $"There was a problem processing the Consumption Notification with the ID '{id}'.");
            return View("Error", new HandleErrorInfo(exception, "Consumptions", "ProcessConsumptionNotification"));
         }
      }

      // GET: Inventory/Consumption
      public ActionResult ClearErrors()
      {
         try
         {
            _healthTrackConsumptionRepository.ClearAllErrors(User.Identity.Name);
            _healthTrackConsumptionRepository.Commit();
            ViewBag.Message = "Errors cleared. Unprocessed records";

            return RedirectToAction("Errors");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem Consumption Notification processing errors.");
            return View("Error", new HandleErrorInfo(exception, "Consumptions", "ClearErrors"));
         }
      }

      public ActionResult ConsumptionByCategory()
      {
         return View();
      }
   }
}