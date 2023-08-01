using HMS.HealthTrack.Web.Utils;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class ProductPriceModels
   {
      [Ignore]
      public int PriceId { get; set; }
      [Ignore]
      public int ProductId { get; set; }
      [Display(Name = "Buy price")]
      public decimal? BuyPrice { get; set; }
      [Display(Name = "Buy currency")]
      public string BuyCurrency { get; set; }
      [Display(Name = "Buy currency rate")]
      public string BuyCurrencyRate { get; set; }
      [Display(Name = "Sell price")]
      public decimal? SellPrice { get; set; }
      [Ignore]
      public int PriceTypeId { get; set; }
   }
}