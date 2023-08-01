
namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Stock
{
   public class StockAtLocationModel
   {
      public int LocationId { get; set; }
      public string Location { get; set; }
      public int? Quantity { get; set; }
   }
}