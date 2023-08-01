
namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Stock
{
   public class NegativeStockViewModel
   {
      public int ProductId { get; set; }
      public string Description { get; set; }
      public string SPC { get; set; }
      public int StoredAt { get; set; }
      public string Location { get; set; }
      public int Quantity { get; set; }
   }
}