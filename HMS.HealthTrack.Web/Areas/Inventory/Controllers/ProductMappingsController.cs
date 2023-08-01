using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ProductMappings;
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
   public class ProductMappingsController : Controller
   {
      private readonly IExternalProductMappingRepository _mappingRepository;
      private readonly IProductMappingUnitOfWork _unitOfWork;
      private readonly ICustomLogger _logger;

      public ProductMappingsController(IProductMappingUnitOfWork unitOfWork, ICustomLogger logger)
      {
         _unitOfWork = unitOfWork;
         _mappingRepository = unitOfWork.MappingRepository;
         _logger = logger;
      }

      // GET: Inventory/ProductMappings
      public ActionResult Index()
      {
         return View();
      }

      // GET: Inventory/ProductMappings/Details/5
      public ActionResult Details(int? id)
      {
         try
         {
            if (!id.HasValue)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var externalProductMapping = _mappingRepository.Find(id.Value);
            if (externalProductMapping == null)
               return HttpNotFound();

            var productDetails =
               (from m in _mappingRepository.GetMappingOverviews() where m.MappingId == id.Value select m).SingleOrDefault();

            return View(new MappingDetails { ExternalProductMapping = externalProductMapping, Overview = productDetails });
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem retrieving the Product Mappings with ID '{0}'.",
                  id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "ProductMappings", "Details"));
         }
      }

      // GET: Inventory/ProductMappings/Edit/5
      public ActionResult Edit(int? id)
      {
         try
         {
            if (!id.HasValue)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var externalProductMapping = _mappingRepository.Find(id.Value);
            if (externalProductMapping == null)
               return HttpNotFound();

            var productDetails =
               (from m in _mappingRepository.GetMappingOverviews() where m.MappingId == id.Value select m).SingleOrDefault();

            if (productDetails == null)
               return HttpNotFound();

            var editModel = Mapper.Map<ExternalProductMapping, EditProductMappingModel>(externalProductMapping);

            return View(editModel);
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem retrieving the Product Mappings with ID '{0}'.",
                  id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "ProductMappings", "Edit"));
         }
      }

      // POST: Inventory/ProductMappings/Edit/5
      // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
      // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit(EditProductMappingModel editProductMapping)
      {
         try
         {
            if (!ModelState.IsValid)
               return View(editProductMapping);

            var mapping = _mappingRepository.Find(editProductMapping.ProductMappingId);

            if (mapping == null)
            {
               ModelState.AddModelError("", "Mapping not found");
               return View(editProductMapping);
            }

            mapping.LastModifiedBy = User.Identity.Name;
            mapping.InventoryProductId = editProductMapping.InventoryProductId;
            _mappingRepository.Update(mapping);
            _mappingRepository.Commit();
            return RedirectToAction("Details", new { @id = editProductMapping.ProductMappingId });
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem editingthe Product Mappings with ID '{0}'.",
                  editProductMapping.ProductMappingId));
            return View("Error", new HandleErrorInfo(exception, "ProductMappings", "Edit"));
         }
      }

      // POST: Inventory/ProductMappings/Delete/5
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
         try
         {
            var externalProductMapping = _mappingRepository.Find(id);
            externalProductMapping.DeletedBy = User.Identity.Name;
            _mappingRepository.Remove(externalProductMapping);
            _mappingRepository.Commit();
            return RedirectToAction("Index");
         }
         catch (Exception exception)
         {
            _logger.Error(exception,
               string.Format("There was a problem retrieving the Product Mappings with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "ProductMappings", "Delete"));
         }
      }
   }
}