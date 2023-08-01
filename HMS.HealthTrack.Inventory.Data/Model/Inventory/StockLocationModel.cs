using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public class StockLocationModel
   {
      public ICollection<HealthTrackLocation> HealthTrackLocations;
      public StockLocation StockLocation;
   }
}