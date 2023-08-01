
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes
{
   public class SingleProductStockTakeDTO
   {
      [Display(Name = "Product to adjust")]
      public int StockTakeProductId { get; set; }
      [Display(Name = "Stock location")]
      public int StockTakeLocationId { get; set; }
      [Display(Name = "Current stock level")]
      public int StockLevel { get; set; }
   }
}