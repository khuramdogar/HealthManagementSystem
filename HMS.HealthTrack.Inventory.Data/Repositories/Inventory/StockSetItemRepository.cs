using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class StockSetItemRepository : IStockSetItemRepository
   {
      private readonly IDbContextInventoryContext _context;

      public StockSetItemRepository(IDbContextInventoryContext context)
      {
         _context = context;
         _context.ObjectContext.ContextOptions.LazyLoadingEnabled = false;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public IQueryable<StockSetItem> FindAll(int stockSetId)
      {
         return _context.StockSetItems.Include(xx => xx.Product).Where(xx => xx.StockSetId == stockSetId);
      }

      public StockSetItem Find(int id)
      {
         return _context.StockSetItems.SingleOrDefault(xx => xx.StockSetItemId == id);
      }

      public void Update(StockSetItem item, int productId, int quantity)
      {
         if (item != null)
         {
            item.Quantity = quantity;
            item.ProductId = productId;
         }
      }

      public void Add(StockSetItem item)
      {
         _context.StockSetItems.Add(item);
      }

      public void Remove(StockSetItem item)
      {
         _context.StockSetItems.Remove(item);
      }

      public void RemoveSetItem(StockSetItem lineItem)
      {
         _context.StockSetItems.Remove(lineItem);
      }
   }

   public interface IStockSetItemRepository
   {
      IQueryable<StockSetItem> FindAll(int stockSetId);
      void Commit();
      StockSetItem Find(int id);
      void Update(StockSetItem item, int productId, int quantity);
      void Add(StockSetItem item);
      void Remove(StockSetItem item);
   }
}