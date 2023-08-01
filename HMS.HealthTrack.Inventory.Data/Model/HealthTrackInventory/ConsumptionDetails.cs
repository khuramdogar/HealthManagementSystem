using System;

namespace HMS.HealthTrack.Web.Data.Model.HealthTrackInventory
{
   public class ConsumptionDetails
   {
      public long? ContainerId { get; set; }
      public string MRN { get; set; }
      public string SerialNumber { get; set; }
      public string LotNumber { get; set; }
      public int Quantity { get; set; }
      public DateTime ConsumedOn { get; set; }
      public string ConsumedBy { get; set; }
      public int? LocationId { get; set; }
      public int? HTLocationId { get; set; }
   }
}