using MediatR;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Products
{
   public class ProductQuery : IRequest<ProductDto>
   {
      public int ProductId { get; set; }
   }
}
