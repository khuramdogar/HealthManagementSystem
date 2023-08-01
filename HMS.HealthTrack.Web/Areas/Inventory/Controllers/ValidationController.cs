using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
   public class ValidationController : Controller
   {
      private readonly ISupplierRepository _supplierRepository;
      private readonly IGeneralLedgerRepository _generalLedgerRepository;


      public ValidationController(ISupplierRepository supplierRepository, IGeneralLedgerRepository generalLedgerRepository)
      {
         _supplierRepository = supplierRepository;
         _generalLedgerRepository = generalLedgerRepository;
      }

      public JsonResult SingleSupplierExists(string supplier, int? supplierId)
      {
         if (supplierId.HasValue)
            return Json(true, JsonRequestBehavior.AllowGet);

         var suppliers =
            _supplierRepository.FindAll()
               .Where(s => s.Company.companyName == supplier);
         var success = suppliers.Count() == 1;
         return success
            ? Json(true, JsonRequestBehavior.AllowGet)
            : Json("Could not match value to an existing supplier. Please select one from the list.", JsonRequestBehavior.AllowGet);
      }

      public JsonResult SingleGLCExists(string glcCode, int? ledgerId)
      {
         if (ledgerId.HasValue)
            return Json(true, JsonRequestBehavior.AllowGet);

         var ledgers = _generalLedgerRepository.FindAll().Where(gl => gl.Code == glcCode);
         var success = ledgers.Count() == 1;
         return success
            ? Json(true, JsonRequestBehavior.AllowGet)
            : Json("Could not match value to an existing general ledger code. Please browse for the correct one.", JsonRequestBehavior.AllowGet);
      }

      public JsonResult ValidReorderSetting(string reorderSetting)
      {
         var invalid = ProductImportStaticValidation.InvalidReordersetting(reorderSetting);
         return invalid
            ? Json("Could not match value to an existing setting. Please select one from the list",
               JsonRequestBehavior.AllowGet)
            : Json(true, JsonRequestBehavior.AllowGet);
      }

      public JsonResult ValidProductSettings(string productSettings)
      {
         var invalid = ProductImportStaticValidation.InvalidProductSettings(productSettings);
         return invalid
            ? Json("Could not match values to existing settings. Please choose from the list.",
               JsonRequestBehavior.AllowGet)
            : Json(true, JsonRequestBehavior.AllowGet);

      }
   }
}