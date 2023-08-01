using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Products;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using MediatR;

namespace HMS.HealthTrack.Web.Areas.Inventory.QueryHandlers.Products
{
   public class ProductQueryHandler : IRequestHandler<ProductQuery, ProductDto>
   {
      private readonly IDbContextInventoryContext _context;

      public ProductQueryHandler(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public async Task<ProductDto> Handle(ProductQuery request, CancellationToken cancellationToken)
      {
         if(request.ProductId != 0)
            return await _context.Products.Where(c => c.ProductId== request.ProductId)
            .Select(HealthTrackProductExtension.Projection)
            .SingleOrDefaultAsync(cancellationToken);

         var query = from c in _context.Products select c;

         return await query
            .Select(HealthTrackProductExtension.Projection)
            .SingleOrDefaultAsync(cancellationToken);
      }
   }
}