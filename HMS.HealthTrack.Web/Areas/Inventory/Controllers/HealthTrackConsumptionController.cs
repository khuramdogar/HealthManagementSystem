using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.HealthTrackConsumptions;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   public class HealthTrackConsumptionController : Controller
   {
      private readonly IInventoryUnitOfWork _unitOfWork;
      private readonly IPropertyProvider _propertyProvider;
      private readonly ICustomLogger _logger;

      public HealthTrackConsumptionController(IInventoryUnitOfWork unitOfWork, IPropertyProvider propertyProvider, ICustomLogger logger)
      {
         _propertyProvider = propertyProvider;
         _logger = logger;
         _unitOfWork = unitOfWork;
      }

      [HttpGet]
      public ActionResult Create()
      {
         var model = new CreateHealthTrackConsumptionModel
         {
            ConsumedBy = User.Identity.Name,
            ConsumedOn = DateTime.Now,
            Quantity = 1,
         };
         return View(model);
      }

      /// <summary>
      /// Creates a manual consumption. 
      /// 
      /// If no mapped product exists, a new HealthTrack product is created and mapped to the new Inventory product.
      /// 
      /// </summary>
      /// <param name="createModel"></param>
      /// <returns></returns>
      [HttpPost]
      public ActionResult Create(CreateHealthTrackConsumptionModel createModel)
      {
         if (!ModelState.IsValid)
         {
            _logger.Warning("Attempted to create HealthTrack consumption for with invalid data");
            return View(createModel);
         }

         if (!createModel.ProductId.HasValue)
         {
            ModelState.AddModelError("", "No product selected");
            return View(createModel);
         }

         var product = _unitOfWork.ProductRepository.Find(createModel.ProductId.Value);
         if (product == null)
         {
            ModelState.AddModelError("", "Cannot find selected product");
            return View(createModel);
         }

         var newHealthTrackProduct = false;
         // try to find mapped product
         var healthTrackProduct = _unitOfWork.FindHealthTrackProduct(product.ProductId);
         if (healthTrackProduct == null)
         {
            _logger.Warning(
               "Cannot find mapped HealthTrack product for Inventory product with ID {ProductId} and SPC {SPC}",
               createModel.ProductId, product.SPC);

            // create product
            healthTrackProduct = _unitOfWork.InventoryMasterRepository.CreateNewProduct(product.Description, product.SPC,product.ScanCodes?.FirstOrDefault()?.Value, User.Identity.Name);
            newHealthTrackProduct = true;
         }

         int patientId;
         if (createModel.ContainerId.HasValue)
         {
            var container = _unitOfWork.MedicalRecordRepository.GetClinicalRecord(createModel.ContainerId);
            if (container == null)
            {
               _logger.Warning(
                  "Cannot create consumption for HealthTrackProduct {HealthTrackProductId} for invalid Container {ContainerId}",
                  createModel.ContainerId);
               ModelState.AddModelError("", "Cannot not find container for Container ID");
               return View(createModel);
            }
            patientId = container.patient_ID;
         }
         else
         {
            var patientIdFromMrn = _unitOfWork.MedicalRecordRepository.FindPatientIdByMrn(createModel.MRN);
            if (patientIdFromMrn == null)
            {
               _logger.Warning(
                  "Cannot create consumption for HealthTrackProduct {HealthTrackProductId} for invalid MRN {MRN}",
                  createModel.ProductId, createModel.MRN);
               ModelState.AddModelError("", "Cannot find patient for MRN");
               return View(createModel);
            }
            patientId = patientIdFromMrn.Value;
         }

         var consumptionDetails = Mapper.Map<CreateHealthTrackConsumptionModel, ConsumptionDetails>(createModel);
         var inventoryUsed = _unitOfWork.InventoryUsedRepository.CreateConsumption(healthTrackProduct, consumptionDetails, patientId);
         healthTrackProduct.Inventory_Used.Add(inventoryUsed);
         _unitOfWork.Commit();

         // create mapping if necessary
         if (newHealthTrackProduct)
         {
            CreateProductMapping(healthTrackProduct.Inv_ID, createModel.ProductId.Value);
            _unitOfWork.Commit();
         }

         return RedirectToAction("Details", "Consumptions", new { id = inventoryUsed.invUsed_ID });
      }

      private void CreateProductMapping(int healthTrackProductId, int inventoryProductId)
      {
         _unitOfWork.ExternalProductMappingRepository.Add(new ExternalProductMapping
         {
            CreatedBy = User.Identity.Name,
            CreatedOn = DateTime.Now,
            ExternalProductId = healthTrackProductId,
            InventoryProductId = inventoryProductId,
            LastModifiedBy = User.Identity.Name,
            LastModifiedOn = DateTime.Now,
            ProductSource = ProductMappingSource.HealthTrack
         });
      }

      [HttpGet]
      public ActionResult FindMRNByContainer(long containerId)
      {
         var mrn = _unitOfWork.MedicalRecordRepository.GetMrn(containerId);
         if (string.IsNullOrWhiteSpace(mrn))
         {
            _logger.Warning("Could not find matching MRN for HealthTrack container Id {ContainerID}", containerId);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Could not find matching container ID");
         }

         return Json(mrn, JsonRequestBehavior.AllowGet);
      }
   }
}