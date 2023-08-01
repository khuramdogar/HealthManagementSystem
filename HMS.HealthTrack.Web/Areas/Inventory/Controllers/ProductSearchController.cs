using System.Web.Mvc;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class ProductSearchController : Controller
   {
      public ActionResult ProductSearch()
      {
         return PartialView("_ProductSearch");
      }

      public ActionResult ProductSearchWithGrid()
      {
         return PartialView("_ProductSearchWithGrid");
      }
   }
}