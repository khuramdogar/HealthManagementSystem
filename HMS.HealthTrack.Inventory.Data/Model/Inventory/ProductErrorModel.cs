using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public class ProductErrorModel
   {
      public ProductErrorReason Reason { get; set; }
      public Dictionary<int, string> Products { get; set; }
   }
}