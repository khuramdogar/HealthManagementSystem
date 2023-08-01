using System.Web.Mvc;

namespace HMS.HealthTrack.Web.Controllers
{
	public class ErrorController : Controller
	{
		public ActionResult Index()
		{
			return View("Error",ViewData.Model);
		}

		public ActionResult NotFound()
		{
			return View();
		}

		public ActionResult AccessDenied()
		{
			return View(ViewData.Model);
		}
	}
}