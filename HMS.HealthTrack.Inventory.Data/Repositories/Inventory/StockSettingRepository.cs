using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class StockSettingRepository : IStockSettingRepository
   {
      private readonly IDbContextInventoryContext _context;

      public StockSettingRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<StockSetting> FindAll()
      {
         return _context.StockSettings;
      }

      public IQueryable<StockSetting> Find(string id)
      {
         return from s in _context.StockSettings where s.SettingId == id select s;
      }

      public void Add(StockSetting newEntity)
      {
         _context.StockSettings.Add(newEntity);
      }

      public void Remove(StockSetting entity)
      {
         _context.StockSettings.Remove(entity);
      }
   }

   public interface IStockSettingRepository
   {
      IQueryable<StockSetting> FindAll();
      IQueryable<StockSetting> Find(string id);
      void Add(StockSetting newEntity);
      void Remove(StockSetting entity);
   }
}