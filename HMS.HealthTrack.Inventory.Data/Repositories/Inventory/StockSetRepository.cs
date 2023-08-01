using System;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class StockSetRepository : IStockSetRepository
   {
      private readonly IDbContextInventoryContext _context;

      public StockSetRepository(IDbContextInventoryContext context)
      {
         _context = context;
         if (_context.ObjectContext != null) // will be null when mocked in unit tests
            _context.ObjectContext.ContextOptions.LazyLoadingEnabled = false;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public StockSet Find(int id)
      {
         return
            (from s in _context.StockSets
               where
                  s.StockSetId == id
                  && s.DeletedOn == null
               select s).Include(
               s => s.Items.Select(si => si.Product)).SingleOrDefault();
      }

      public IQueryable<StockSet> FindAll()
      {
         return _context.StockSets.Include(ss => ss.Items.Select(i => i.Product)).Where(s => s.DeletedOn == null);
      }

      public void Add(StockSet stockSet)
      {
         _context.StockSets.Add(stockSet);
      }

      public void Update(StockSet stockSet)
      {
         var existing = Find(stockSet.StockSetId);
         if (existing == null) return;

         _context.Entry(existing).CurrentValues.SetValues(stockSet);
      }

      public StockSetItem GetSetItem(int stockSetId, int productId)
      {
         return
            (from i in _context.StockSetItems where i.StockSetId == stockSetId && i.ProductId == productId select i)
            .SingleOrDefault();
      }

      public void RemoveSetItem(StockSetItem lineItem)
      {
         _context.StockSetItems.Remove(lineItem);
      }

      public void Remove(StockSet entity, string username)
      {
         var existing = Find(entity.StockSetId);
         if (existing == null) return;
         entity.DeletedBy = username;
         entity.DeletedOn = DateTime.Now;
      }
   }

   public interface IStockSetRepository
   {
      void Commit();
      StockSet Find(int id);
      void Update(StockSet stockSet);
      StockSetItem GetSetItem(int id, int productId);
      void RemoveSetItem(StockSetItem lineItem);
      void Remove(StockSet entity, string username);
      IQueryable<StockSet> FindAll();
      void Add(StockSet stockSet);
   }
}