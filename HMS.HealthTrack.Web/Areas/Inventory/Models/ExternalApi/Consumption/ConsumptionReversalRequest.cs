using MediatR;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Consumption
{
   public class ConsumptionReversalRequest : IRequest<HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Consumption.ConsumptionDeletionResult>
   {
      public int ConsumptionReference { get; set; }
      public string Requester { get; set; }
   }
}