using System;
using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes
{
   public class IncomingStockTake
   {
      public DateTime CreatedOn { get; set; }
      public List<IncomingStockTakeItem> Items { get; set; }
      public DateTime StockTakeDate { get; set; }
      public int LocationId { get; set; }
      public string CreatedBy { get; set; }
   }
}
