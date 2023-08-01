using System.Web.Mvc;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class AdminController : Controller
   {
      // GET: Admin
      public ActionResult Index()
      {
         return View();
      }

      public ActionResult Settings()
      {
         return View();
      }

      public ActionResult UserPermissions()
      {
         return View();
      }
   }
}