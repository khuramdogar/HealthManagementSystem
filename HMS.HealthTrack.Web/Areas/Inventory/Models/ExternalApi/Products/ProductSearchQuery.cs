using MediatR;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Products
{
   public class ProductSearchQuery : IRequest<ProductSearchResultDto>
   {
      public string ScannedCode { get; set; }
      public string UPC { get; set; }
      public string SPC { get; set; }
      public string SearchTerm { get; set; }
   }
}
