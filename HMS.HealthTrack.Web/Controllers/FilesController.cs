using System.Web.Mvc;

namespace HMS.HealthTrack.Web.Controllers
{
    public class FilesController : Controller
    {
        // GET: Files
        public ActionResult Viewer()
        {
            return View();
        }
    }
}