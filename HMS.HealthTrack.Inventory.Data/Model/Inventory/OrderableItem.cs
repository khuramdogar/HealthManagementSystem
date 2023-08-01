using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public class OrderableItem : IOrderableItem
   {
      public string SPC { get; set; }
      public string Description { get; set; }
      public decimal? BuyPrice { get; set; }
      public string Currency { get; set; }
      public OrderableItemSource Source { get; set; }
      public List<int> RequestIds { get; set; }
      public string Supplier { get; set; }
      public string Category { get; set; }
      public int? ConsumptionId { get; set; }
      public int ProductId { get; set; }
      public int Quantity { get; set; }
   }
}