using System;
using System.Collections.Generic;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes
{
   public class StockTakeList
   {
      public int StockTakeId { get; set; }
      public DateTime CreatedOn { get; set; }
      public List<StockTakeListItem> ListItems { get; set; }
      public string LocationName { get; set; }
      public DateTime? StockTakeDate { get; set; }
      public StockTakeStatus StockTakeStatus { get; set; }
      public string Name { get; set; }
   }
}