using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   [ModelMetaType(typeof(ProductPriceMeta))]
   public class ProductPriceViewModel : IMapFrom<ProductPrice>
   {
      public int PriceId { get; set; }
      public int ProductId { get; set; }
      [Display(Name = "buy price")]
      public decimal? BuyPrice { get; set; }
      [Display(Name = "buy currency")]
      public string BuyCurrency { get; set; }
      [Display(Name = "buy currency rate")]
      public string BuyCurrencyRate { get; set; }
      [Display(Name = "sell price")]
      public decimal? SellPrice { get; set; }
      public int PriceTypeId { get; set; }
      public string PriceTypeName { get; set; }
   }
}