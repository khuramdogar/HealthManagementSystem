using System;

namespace HMS.HealthTrack.Web.Data.Model
{
   public enum ConsumptionProcessingStatus
   {
      Unprocessed = 0,
      Processed = 1,
      Error = 2,
      [Obsolete] MissingMapping = 3,
      [Obsolete] Archived = 4,
      [Obsolete] NoStockTake = 5,
      Ignored = 6,
      [Obsolete] Override = 7
   }
}