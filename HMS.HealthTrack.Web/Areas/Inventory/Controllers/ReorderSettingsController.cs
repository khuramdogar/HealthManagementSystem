using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   public class ReorderSettingsController : Controller
   {
      [HttpGet]
      public JsonResult GetAllAsText()
      {
         var settings = Enum.GetValues(typeof(ReorderSettings)).Cast<ReorderSettings>().Select(rs => new SelectListItem
         {
            Text = HelperMethods.GetEnumDisplayName<ReorderSettings>(rs),
            Value = Enum.GetName(typeof(ReorderSettings), rs)
         }).OrderBy(rs => rs.Text);

         return Json(settings, JsonRequestBehavior.AllowGet);
      }
   }
}