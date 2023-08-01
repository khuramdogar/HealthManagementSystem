using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using System;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class MedicareProductsController : Controller
   {
      private readonly IMedicareProductsRepository _medicareProductsRepository;
      private readonly ICustomLogger _logger;

      public MedicareProductsController(IMedicareProductsRepository medicareProductsRepository, ICustomLogger logger)
      {
         _medicareProductsRepository = medicareProductsRepository;
         _logger = logger;
      }

      public JsonResult GetAll(string text)
      {
         var items =
            _medicareProductsRepository.Search(text).Select(p => new SelectListItem() { Value = p.Key, Text = p.Value }).Take(100);
         return Json(items, JsonRequestBehavior.AllowGet);
      }

      public ActionResult MinBenefit(string rebateCode)
      {
         try
         {
            var medicareProduct = _medicareProductsRepository.FindByRebateCode(rebateCode);
            return Content(medicareProduct?.MinBenefit == null ? "no benefit" : $"{medicareProduct.MinBenefit:C}");
         }
         catch (Exception exception)
         {
            _logger.Error(exception, $"There was a problem retrieving the Medicare Product with rebate code '{rebateCode}'.");
            return View("Error", new HandleErrorInfo(exception, "MedicareProducts", "MinBenefit"));
         }
      }

      public ActionResult Details(string rebateCode)
      {
         try
         {
            var medicareProduct = _medicareProductsRepository.FindByRebateCode(rebateCode);
            if (medicareProduct == null)
               return Content("");
            return PartialView("_Details", Mapper.Map<MedicareProduct, DetailsMedicareProductsViewModel>(medicareProduct));
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving the Medicare Product with rebate code '{0}'.", rebateCode));
            return View("Error", new HandleErrorInfo(exception, "MedicareProducts", "Details"));
         }
      }
   }
}