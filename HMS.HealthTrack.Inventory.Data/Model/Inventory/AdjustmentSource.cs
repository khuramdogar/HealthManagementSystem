using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public enum AdjustmentSource
   {
      Unknown = 0,
      HealthTrack = 1,
      [Display(Name = "User")] Web = 2,
      [Display(Name = "Stock take")] StockTake = 3,
      Order = 4,
      System = 5
   }
}