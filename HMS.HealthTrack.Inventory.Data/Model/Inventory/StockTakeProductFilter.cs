using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public enum StockTakeProductFilter
   {
      All = 0,
      [Display(Name = "Without stock take")] WithoutStockTake = 1
   }
}