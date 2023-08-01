using System.Linq;
using System.Web.Mvc;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   public class HealthTrackLocationController : Controller
   {
      private IHealthTrackLocationRepository _repository;
      private ICustomLogger _logger;

      public HealthTrackLocationController(IHealthTrackLocationRepository repository, ICustomLogger logger)
      {
         _repository = repository;
         _logger = logger;
      }

      public JsonResult GetAll()
      {
         var locations = _repository.FindAll().Select(l => new
         {
            Value = l.location_ID,
            Text = l.name
         }).OrderBy(l => l.Text).ToList();
         return Json(locations, JsonRequestBehavior.AllowGet);
      }
   }
}