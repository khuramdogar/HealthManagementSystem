using MediatR;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Consumption
{
   public class ConsumptionQuery : IRequest<ConsumptionDto>
   {
      public int ConsumptionId { get; set; }
   }
}
