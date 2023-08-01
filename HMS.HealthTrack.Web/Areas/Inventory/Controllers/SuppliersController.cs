using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Suppliers;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class SuppliersController : Controller
   {
      private readonly ISupplierRepository _supplierRepository;
      private readonly ICustomLogger _logger;

      public SuppliersController(ISupplierRepository supplierRepository, ICustomLogger logger)
      {
         _supplierRepository = supplierRepository;
         _logger = logger;
      }

      // GET: Inventory/Suppliers
      public ActionResult Index()
      {
         return View();
      }

      [HttpGet]
      public ActionResult Create()
      {
         return View();
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Create(SuppliersViewModel newSupplier)
      {
         try
         {
            if (!ModelState.IsValid)
               return View(newSupplier);

            var supplierModel = Mapper.Map<SuppliersViewModel, SupplierModel>(newSupplier);
            _supplierRepository.Add(supplierModel, User.Identity.Name);
            _supplierRepository.Commit();

            return RedirectToAction("Index");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem creating a Supplier with name '{0}'.", newSupplier.Name));
            return View("Error", new HandleErrorInfo(exception, "Suppliers", "Create"));
         }
      }

      [HttpGet]
      public ActionResult Details(int? id)
      {
         try
         {
            if (!id.HasValue)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var supplier = _supplierRepository.Find(id.Value);
            if (supplier == null)
               return HttpNotFound();

            return View(Mapper.Map<Supplier, SuppliersViewModel>(supplier));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retreiving the Supplier with ID '{0}'.", id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "Suppliers", "Details"));
         }
      }

      [HttpGet]
      public ActionResult Edit(int? id)
      {
         try
         {
            if (!id.HasValue)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var supplier = _supplierRepository.Find(id.Value);
            if (supplier == null)
               return HttpNotFound();

            return View(Mapper.Map<Supplier, SuppliersViewModel>(supplier));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retreiving the Supplier with ID '{0}'.", id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "Suppliers", "Edit"));
         }
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit(SuppliersViewModel supplier)
      {
         try
         {
            if (!ModelState.IsValid)
               return View(supplier);

            supplier.LastModifiedBy = User.Identity.Name;
            var supplierModel = Mapper.Map<SuppliersViewModel, SupplierModel>(supplier);

            _supplierRepository.Update(supplierModel);
            _supplierRepository.Commit();
            return RedirectToAction("Details", new { id = supplier.company_ID });
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem editing the Supplier with ID '{0}'.", supplier.company_ID));
            return View("Error", new HandleErrorInfo(exception, "Suppliers", "Edit"));
         }
      }

      [HttpGet]
      public ActionResult Delete(int? id)
      {
         try
         {
            if (!id.HasValue)
               return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var supplier = _supplierRepository.Find(id.Value);
            if (supplier == null)
               return HttpNotFound();

            return View(Mapper.Map<Supplier, SuppliersViewModel>(supplier));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retreiving the Supplier with ID '{0}'.", id.HasValue ? id.Value.ToString() : string.Empty));
            return View("Error", new HandleErrorInfo(exception, "Suppliers", "Delete"));
         }
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Delete(int id)
      {
         try
         {
            _supplierRepository.Remove(id, User.Identity.Name);
            _supplierRepository.Commit();
            return RedirectToAction("Index");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retreiving the Supplier with ID '{0}'.", id));
            return View("Error", new HandleErrorInfo(exception, "Suppliers", "Delete"));
         }
      }

      public JsonResult GetSuppliers(string text)
      {
         var suppliers =
            _supplierRepository.FindAll()
               .Where(s => s.Company.companyName.Contains(text))
               .AsEnumerable()
               .Select(s => new SelectListItem { Text = s.Company.companyName, Value = s.company_ID.ToString() }).OrderBy(sli => sli.Text);

         return Json(suppliers, JsonRequestBehavior.AllowGet);
      }
   }
}