using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ProductImport;
using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ProductImportController : Controller
   {
      private readonly IProductImportDataRepository _productImportDataRepository;
      private readonly IProductImportRepository _productImportRepository;
      private readonly ICustomLogger _logger;
      private readonly IPropertyProvider _propertyProvider;
      private readonly IProductImportUnitOfWork _unitOfWork;

      public ProductImportController(IProductImportUnitOfWork unitOfWork, ICustomLogger logger, IPropertyProvider propertyProvider)
      {
         _unitOfWork = unitOfWork;
         _productImportDataRepository = unitOfWork.ProductImportDataRepository;
         _logger = logger;
         _productImportRepository = unitOfWork.ProductImportRepository;
         _propertyProvider = propertyProvider;
      }

      public ActionResult Index()
      {
         return View();
      }

      public ActionResult Products(int id)
      {
         try
         {
            var productImport = _productImportDataRepository.Find(id);
            if (productImport == null)
            {
               _logger.Information("Could not find product import {ProductImportId}", id);
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(Mapper.Map<ProductImportData, ProductImportDataViewModel>(productImport));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem fetching the product import {ProductImportId}", id);
            return (View("Error", new HandleErrorInfo(exception, "ProductImport", "Products")));
         }
      }

      public ActionResult ProductImportDetails(int id)
      {
         try
         {
            var productImport = _productImportDataRepository.Find(id);
            if (productImport == null)
            {
               _logger.Information("Could not find product import {ProductImportId}", id);
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = Mapper.Map<ProductImportData, ProductImportDataViewModel>(productImport);
            return PartialView("_ProductImportDetails", model);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem fetching the product import {ProductImportId}", id);
            return PartialView("_ProductImportDetails");
         }
      }

      [HttpGet]
      public ActionResult Edit(int? id)
      {
         if (!id.HasValue)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var productImport = _productImportRepository.Find(id.Value);
         if (productImport == null)
         {
            return HttpNotFound();
         }
         var productLedgerType = _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;
         ViewBag.LedgerType = productLedgerType;

         var productImportModel = Mapper.Map<ProductImport, ProductImportModel>(productImport);

         PopulateTierCodeModels(productImportModel, productLedgerType);
         productImportModel.ValidGlc = productImport.LedgerId.HasValue;

         ViewBag.TierCodeModels = productImportModel.TierCodeModels;
         TryValidateModel(productImportModel);
         return View(productImportModel);
      }

      private void PopulateTierCodeModels(ProductImportModel productImportModel, int productLedgerType)
      {
         if (!string.IsNullOrWhiteSpace(productImportModel.LedgerId))
         {
            var ledgerId = int.Parse(productImportModel.LedgerId);
            // deconstruct ledger code
            productImportModel.TierCodeModels =
               GeneralLedgerHelper.DeconstructGeneralLedgerCode(_unitOfWork.GeneralLedgerRepository,
                  _unitOfWork.GeneralLedgerTierRepository, ledgerId, productLedgerType);
         }
         else
         {
            var codeLedgers =
               (from codes in _productImportRepository.GetProductImportGeneralLedgerCodes(productImportModel.ProductImportId)
                join gl in _unitOfWork.GeneralLedgerRepository.FindAll() on new { codes.TierId, codes.Code } equals
                   new { gl.TierId, gl.Code } into gpgl
                from gl in gpgl.DefaultIfEmpty()
                select new { codes.Code, codes.TierId, LedgerId = gl != null ? gl.LedgerId : (int?)null });

            productImportModel.TierCodeModels =
               (from t in _unitOfWork.GeneralLedgerTierRepository.FindAll().Where(t => t.LedgerType == productLedgerType)
                join cl in codeLedgers on t.TierId equals cl.TierId into gcl
                from cl in gcl.DefaultIfEmpty()
                select new GeneralLedgerTierCodeModel
                {
                   Code = cl != null ? cl.Code : string.Empty,
                   Name = t.Name,
                   LedgerId = cl != null ? cl.LedgerId.ToString() : string.Empty,
                   Tier = t.Tier,
                   TierId = t.TierId
                }).OrderBy(m => m.Tier).ToList();
         }
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit(ProductImportModel productImportModel)
      {
         try
         {
            if (!ModelState.IsValid)
            {
               var productLedgerType = _propertyProvider.LedgerTypes.Single(lt => lt.Name == InventoryConstants.ProductLedgerType).LedgerTypeId;
               PopulateTierCodeModels(productImportModel, productLedgerType);
               return View(productImportModel);
            }


            var existing = _productImportRepository.Find(productImportModel.ProductImportId);
            if (existing == null)
            {
               _logger.Warning("Cannot find existing product to import to update for {ProductImportId}",
                  productImportModel.ProductImportId);
               ModelState.AddModelError("", "Cannot find product to update");
               return View(productImportModel);
            }

            var productImport = Mapper.Map<ProductImportModel, ProductImport>(productImportModel);

            _productImportRepository.Update(existing, productImport);
            _productImportRepository.Commit();
            _productImportDataRepository.UpdateStatus(productImport.ProductImportDataId);
            _productImportDataRepository.Commit();
            return RedirectToAction("Products", new { id = productImportModel.ProductImportDataId });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, "There was a problem editing the product for import with ID {ProductImportId}.", productImportModel.ProductImportId);
            return View("Error", new HandleErrorInfo(exception, "ProductImport", "Edit"));
         }
      }


   }
}