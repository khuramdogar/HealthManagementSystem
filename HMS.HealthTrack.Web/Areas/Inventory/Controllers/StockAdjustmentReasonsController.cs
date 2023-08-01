using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockAdjustmentReasons;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   public class StockAdjustmentReasonsController : Controller
   {
      private readonly IStockAdjustmentRepository _stockAdjustmentRepository;
      private static readonly List<string> SystemNames = typeof(InventoryConstants.StockAdjustmentReasons).GetFields().Select(f => f.GetValue(f).ToString().ToUpper()).ToList();

      public StockAdjustmentReasonsController(IStockAdjustmentRepository stockAdjustmentRepository)
      {
         _stockAdjustmentRepository = stockAdjustmentRepository;
      }

      public ActionResult Index()
      {
         return View();
      }

      public ActionResult EditingPopup_Read([DataSourceRequest] DataSourceRequest request)
      {
         var reasons = _stockAdjustmentRepository.GetReasons();
         return
            Json(
               reasons.Select(Mapper.Map<StockAdjustmentReason, StockAdjustmentReasonViewModel>)
                  .ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
      }

      [AcceptVerbs(HttpVerbs.Post)]
      public ActionResult EditingPopup_Create([DataSourceRequest] DataSourceRequest request,
         StockAdjustmentReasonViewModel reason)
      {
         if (reason != null && ModelState.IsValid)
         {
            var newReason = Mapper.Map<StockAdjustmentReasonViewModel, StockAdjustmentReason>(reason);
            var nameExists = _stockAdjustmentRepository.ReasonNameExists(reason.Name);
            if (nameExists)
            {
               ModelState.AddModelError("Name", "The name already exists");
            }
            else
            {
               newReason.CreatedBy = User.Identity.Name;
               _stockAdjustmentRepository.CreateReason(newReason);
               _stockAdjustmentRepository.Commit();
            }

            //return updated model
            reason = Mapper.Map<StockAdjustmentReason, StockAdjustmentReasonViewModel>(newReason);
         }

         return Json(new[] { reason }.ToDataSourceResult(request, ModelState));
      }

      [AcceptVerbs(HttpVerbs.Post)]
      public ActionResult EditingPopup_Update([DataSourceRequest] DataSourceRequest request,
         StockAdjustmentReasonViewModel reason)
      {
         if (reason != null && ModelState.IsValid)
         {
            var name = reason.Name.Trim();
            var nameExists = _stockAdjustmentRepository.ReasonNameExists(name);
            if (nameExists)
            {
               ModelState.AddModelError("Name", "The name already exists");
            }
            else if (reason.IsSystemReason)
            {
               ModelState.AddModelError("", "Cannot update a reason controlled by the system");
            }
            else if (SystemNames.Contains(name.ToUpper()))
            {
               ModelState.AddModelError("Name", "Cannot change a name that is used by the system");
            }
            else
            {
               var updatedReason = Mapper.Map<StockAdjustmentReasonViewModel, StockAdjustmentReason>(reason);
               updatedReason.LastModifiedUser = User.Identity.Name;
               _stockAdjustmentRepository.UpdateReason(updatedReason);
               _stockAdjustmentRepository.Commit();
            }
         }

         return Json(new[] { reason }.ToDataSourceResult(request, ModelState));
      }

      [AcceptVerbs(HttpVerbs.Post)]
      public ActionResult EditingPopup_Destroy([DataSourceRequest] DataSourceRequest request,
         StockAdjustmentReasonViewModel reason)
      {
         if (reason != null)
         {
            if (reason.IsSystemReason)
            {
               ModelState.AddModelError("", "Cannot delete a reason controlled by the system");
            }
            else if (SystemNames.Contains(reason.Name.Trim().ToUpper()))
            {
               ModelState.AddModelError("Name", "Cannot delete a reason used by the system");
            }
            else
            {
               var reasonToDelete = Mapper.Map<StockAdjustmentReasonViewModel, StockAdjustmentReason>(reason);
               reasonToDelete.DeletedBy = User.Identity.Name;
               _stockAdjustmentRepository.DeleteReason(reasonToDelete);
               _stockAdjustmentRepository.Commit();
            }
         }

         return Json(new[] { reason }.ToDataSourceResult(request, ModelState));
      }
   }
}