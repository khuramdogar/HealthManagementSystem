using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Products
{
   public class ProductSearchResultDto
   {
      public IEnumerable<ProductDto> Data { get; set; }
      public string ErrorMessage { get; set; }
   }
}