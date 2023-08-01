using System.Web.Mvc;

namespace HMS.HealthTrack.Web.Areas.Admin.Controllers
{
   public class DiagnoticsController : Controller
   {
      // GET: Admin/Diagnotics
      public ActionResult Index()
      {
         return View();
      }

      [ValidateAntiForgeryToken]
      [HttpPost]
      public ActionResult TestAntiForgeryToken()
      {
         ViewBag.Message = "Success";
         return View("Index");
      }
   }
}