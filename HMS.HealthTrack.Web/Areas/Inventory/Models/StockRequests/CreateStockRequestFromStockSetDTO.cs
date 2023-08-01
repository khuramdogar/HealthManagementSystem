
namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockRequests
{
   public class CreateStockRequestFromStockSetDTO
   {
      public int StockSetId { get; set; }
      public int Location { get; set; }
      public bool Urgent { get; set; }
   }
}