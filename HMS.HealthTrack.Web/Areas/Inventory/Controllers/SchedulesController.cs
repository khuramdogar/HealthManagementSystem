using Hangfire;
using Hangfire.Storage;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Schedules;
using System.Linq;
using System.Web.Mvc;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class SchedulesController : Controller
   {
      // GET: Inventory/Schedules
      [HttpGet]
      public ActionResult Index()
      {
         ViewBag.Status = GetStatus();
         return View();
      }

      [HttpPost]
      public ActionResult Index(ScheduleIndexViewModel model)
      {
         string newSchedule;

         switch (model.SchedulerPeriod)
         {
            case SchedulerPeriods.Never:
               RecurringJob.RemoveIfExists(ScheduleTasks.ConsumptionProcessing);
               ViewBag.Status = GetStatus();
               return View();
            case SchedulerPeriods.Minute:
               newSchedule = string.Format("*/{0} * * * *", model.Times);
               break;
            case SchedulerPeriods.Hour:
               newSchedule = string.Format("0 */{0} * * *", model.Times);
               break;
            case SchedulerPeriods.Day:
               newSchedule = string.Format("0 0 */{0} * *", model.Times);
               break;
            default:
               ModelState.AddModelError("", "Invalid schedule");
               return View(model);
         }
         RecurringJob.AddOrUpdate<HealthTrackProductConsumptionProcessor>(ScheduleTasks.ConsumptionProcessing, x => x.ProcessAllInventoryUsed("HealthTrackConsumptionProcessor"), newSchedule);
         return View();
      }

      private string GetStatus()
      {
         using (var connection = JobStorage.Current.GetConnection())
         {
            var recurring = connection.GetRecurringJobs().FirstOrDefault(p => p.Id == ScheduleTasks.ConsumptionProcessing);

            if (recurring == null)
            {
               // recurring job not found
               return "Job has not been created yet.";
            }
            return recurring.NextExecution.HasValue ?
               string.Format("Job is scheduled to execute at {0}.", recurring.NextExecution.Value.ToLocalTime())
               : ("Job has not been scheduled yet. Check again later.");
         }
      }
   }
}