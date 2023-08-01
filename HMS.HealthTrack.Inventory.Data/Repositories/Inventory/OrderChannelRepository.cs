using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class OrderChannelRepository : IOrderChannelRepository
   {
      private readonly IDbContextInventoryContext _context;

      public OrderChannelRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public OrderChannelProduct AddProductToChannel(int productId, int channelId, string reference, bool automaticOrder = false)
      {
         //Check it isn't already added
         if (_context.OrderChannelProducts.Any(x => x.ProductId == productId && x.OrderChannelId == channelId))
            return _context.OrderChannelProducts.SingleOrDefault(x => x.ProductId == productId && x.OrderChannelId == channelId);

         //Add
         var newOrderChannelProduct = new OrderChannelProduct {ProductId = productId, OrderChannelId = channelId, Reference = reference, AutomaticOrder = automaticOrder};
         _context.OrderChannelProducts.Add(newOrderChannelProduct);

         return newOrderChannelProduct;
      }

      public IQueryable<OrderChannelProduct> GetChannelsForProduct(int productId)
      {
         return _context.OrderChannelProducts.Include(pc => pc.OrderChannel).Where(pc => pc.ProductId == productId);
      }

      public OrderChannelProduct GetOrderChannelAssignment(int orderChannelProductId)
      {
         return (from x in _context.OrderChannelProducts.Include(pc => pc.OrderChannel)
            where x.OrderChannelProductId == orderChannelProductId
            select x).SingleOrDefault();
      }

      public IQueryable<OrderChannel> GetAvailableChannels()
      {
         return _context.OrderChannels;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public void UnassignProductOrderChannel(int productId, int orderChannelId)
      {
         var result = _context.OrderChannelProducts.SingleOrDefault(x => x.ProductId == productId && x.OrderChannelId == orderChannelId);

         if (result != null)
            _context.OrderChannelProducts.Remove(result);
      }
   }

   public interface IOrderChannelRepository
   {
      OrderChannelProduct AddProductToChannel(int productId, int channelId, string reference, bool automaticOrder = false);
      IQueryable<OrderChannelProduct> GetChannelsForProduct(int productId);
      IQueryable<OrderChannel> GetAvailableChannels();
      void Commit();
      void UnassignProductOrderChannel(int productId, int orderChannelId);
      OrderChannelProduct GetOrderChannelAssignment(int orderChannelProductId);
   }
}