using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Consumption;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   public class ConsumptionsController : MediatedController
   {
      [HttpGet]
      public async Task<JsonResult<ConsumptionDto>> GetConsumption([FromUri]ConsumptionQuery query)
      {
         var result = await Mediator.Send(query);
         return Json(result);
      }

      [HttpPost]
      public async Task<IHttpActionResult> SubmitConsumption(ConsumptionSubmission consumptionSubmission)
      {
         if (!ModelState.IsValid)
            return BadRequest(ModelState);

         var result = await Mediator.Send(consumptionSubmission);
         return Json(result);
      }

      [HttpDelete]
      public async Task<IHttpActionResult> SubmitConsumption(ConsumptionReversalRequest request)
      {
         if (!ModelState.IsValid)
            return BadRequest(ModelState);

         var result = await Mediator.Send(request);
         return Json(result);
      }
   }
}
