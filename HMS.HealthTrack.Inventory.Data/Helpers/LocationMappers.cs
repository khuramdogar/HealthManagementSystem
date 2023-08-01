using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Data.Helpers
{
   public class HealthTrackLocationMapper
   {
      public int HealthTrackLocationId { get; set; }
      public List<int> StockLocations { get; set; }

      public int? GetMappedStockLocation()
      {
         if (StockLocations != null && StockLocations.Count == 1) return StockLocations[0];

         // Do something to choose a location

         return null;
      }
   }
}