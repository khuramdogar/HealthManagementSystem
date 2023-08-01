using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Products;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   public class ProductSearchController : MediatedController
    {
      [HttpGet]
      public async Task<JsonResult<ProductSearchResultDto>> GetProductByScanCode([FromUri] ProductSearchQuery query)
      {
         var result = await Mediator.Send(query);
         return Json(result);
      }
   }
}
