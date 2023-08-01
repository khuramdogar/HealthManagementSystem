using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Utils;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockRequests
{
   [ModelMetaType(typeof(StockRequestMeta))]
   public class CreateStockRequestViewModel
   {
      public int ProductId { get; set; }
      public int Quantity { get; set; }
      public int Location { get; set; }
      public bool IsUrgent { get; set; }
   }
}