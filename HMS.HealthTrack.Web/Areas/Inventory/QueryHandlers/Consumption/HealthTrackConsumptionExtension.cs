using System;
using System.Linq.Expressions;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Consumption;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.QueryHandlers.Consumption
{
   public static class HealthTrackConsumptionExtension
   {
      public static Expression<Func<HealthTrackConsumption, ConsumptionDto>> Projection
      {
         get
         {
            return c => new ConsumptionDto()
            {
               ConsumptionId = c.UsedId,
               Name = c.Name,
            };
         }
      }
   }
}