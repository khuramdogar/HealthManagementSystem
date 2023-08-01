using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Products
{
   public class ProductDto
   {
      public int ProductId { get; set; }
      public string Name { get; set; }
      public string SPC { get; set; }
      public IEnumerable<string> UPCs { get; set; }
      public IEnumerable<ProductPriceDto> Prices { get; set; }
      public string RebateCode { get; set; }
      public string GeneralLedgeCode { get; set; }
      public string Supplier { get; set; }
      public string Manufacturer { get; set; }
   }

   public class ProductPriceDto
   {
      public decimal? SellPrice { get; set; }
      public decimal? BuyPrice { get; set; }
      public int? PriceId { get; set; }
      public string PriceName { get; set; }
      public IEnumerable<string> UsedFor { get; set; }
   }
}