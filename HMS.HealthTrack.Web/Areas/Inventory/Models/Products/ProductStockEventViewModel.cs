using System;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class ProductStockEventViewModel
   {
      public int ProductId { get; set; }
      public StockEvent StockEvent { get; set; }
      public DateTime EventDate { get; set; }
      public string Creator { get; set; }
      public int Quantity { get; set; }
      public string Location { get; set; }
      public string Status { get; set; }
      public string Source { get; set; }
      public long EventId { get; set; }
   }
}