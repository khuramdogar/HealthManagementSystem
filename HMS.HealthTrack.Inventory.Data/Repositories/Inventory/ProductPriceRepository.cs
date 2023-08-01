using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class ProductPriceRepository : IProductPriceRepository
   {
      private readonly IDbContextInventoryContext _context;

      public ProductPriceRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<ProductPrice> FindByProduct(int productId)
      {
         return _context.ProductPrices.Where(xx => xx.ProductId == productId);
      }

      public List<PriceType> FindAllPriceTypes()
      {
         return _context.PriceTypes.ToList();
      }
   }

   public interface IProductPriceRepository
   {
      IQueryable<ProductPrice> FindByProduct(int productId);
      List<PriceType> FindAllPriceTypes();
   }
}