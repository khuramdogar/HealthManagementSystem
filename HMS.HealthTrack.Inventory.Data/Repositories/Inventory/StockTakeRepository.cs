using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class StockTakeRepository : IStockTakeRepository
   {
      private readonly IDbContextInventoryContext _context;

      public StockTakeRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<StockTake> FetchUserStockTakes()
      {
         return (from s in _context.StockTakes where !s.DeletedOn.HasValue && s.Source == StockTakeSource.User select s).Include(s => s.StockTakeItems);
      }

      public StockTake Fetch(int id)
      {
         return
            _context.StockTakes.Where(s => s.StockTakeId == id && s.DeletedOn == null)
               .Include(s => s.StockTakeItems.Select(sti => sti.Product))
               .Include(s => s.Location)
               .SingleOrDefault();
      }

      public IQueryable<StockTakeItem> FetchItem(int stockTakeItemId)
      {
         return _context.StockTakeItems.Include(sti => sti.Product).Where(item => item.StockTakeItemId == stockTakeItemId);
      }

      public IQueryable<StockTakeItem> FetchItems(int stockTakeId)
      {
         return _context.StockTakeItems.Where(item => item.StockTakeId == stockTakeId && !item.DeletedOn.HasValue);
      }

      public void UpdateStockTakeDetails(int stockTakeId, int stockTakeLocationId, DateTime stockTakeDate, string name, string username)
      {
         var stockTake = Fetch(stockTakeId);
         if (stockTake == null) return;
         stockTake.Name = name;
         stockTake.LocationId = stockTakeLocationId;
         stockTake.StockTakeDate = stockTakeDate;
         stockTake.ModifiedBy = username;
         stockTake.ModifiedOn = DateTime.Now;
      }

      public void Add(StockTake stockTake)
      {
         if (stockTake == null) return;
         stockTake.CreatedOn = DateTime.Now;
         _context.StockTakes.Add(stockTake);
      }

      public void Update(StockTake stockTake)
      {
         var existing = Fetch(stockTake.StockTakeId);
         if (existing == null) return;

         stockTake.ModifiedOn = DateTime.Now;
         _context.Entry(existing).CurrentValues.SetValues(stockTake);
         _context.Entry(existing).Property(p => p.CreatedBy).IsModified = false;
      }

      public void UpdateItem(StockTakeItem stockTakeItem, string username)
      {
         var existing = FetchItem(stockTakeItem.StockTakeItemId).SingleOrDefault();
         if (existing == null) return;

         if (existing.Status == StockTakeItemStatus.Created && stockTakeItem.StockLevel.HasValue)
            stockTakeItem.Status = StockTakeItemStatus.Pending;
         else if (existing.Status == StockTakeItemStatus.Pending && !stockTakeItem.StockLevel.HasValue)
            stockTakeItem.Status = StockTakeItemStatus.Created;
         else if (existing.Status == StockTakeItemStatus.Complete) return;

         stockTakeItem.ModifiedOn = DateTime.Now;
         stockTakeItem.ModifiedBy = username;

         _context.Entry(existing).CurrentValues.SetValues(stockTakeItem);
         _context.Entry(existing).Property(p => p.CreatedOn).IsModified = false;
         _context.Entry(existing).Property(p => p.CreatedBy).IsModified = false;
      }

      public void UpdateItemStockLevels(int stockTakeId, Dictionary<int, int?> items, string username)
      {
         var keys = items.Keys;
         var itemsToUpdate =
            _context.StockTakeItems.Where(sti => sti.StockTakeId == stockTakeId && keys.Contains(sti.StockTakeItemId));

         foreach (var item in itemsToUpdate)
         {
            item.StockLevel = items[item.StockTakeItemId];
            item.ModifiedBy = username;
            item.ModifiedOn = DateTime.Now;
            item.Status = item.StockLevel != null ? StockTakeItemStatus.Pending : StockTakeItemStatus.Created;
         }
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public void AddItem(StockTakeItem stockTakeItem)
      {
         stockTakeItem.CreatedOn = DateTime.Now;
         stockTakeItem.Status = stockTakeItem.StockLevel != null
            ? StockTakeItemStatus.Pending
            : StockTakeItemStatus.Created;
         _context.StockTakeItems.Add(stockTakeItem);
      }

      public void Remove(int id, string username)
      {
         var stockTake = _context.StockTakes.SingleOrDefault(st => st.StockTakeId == id);
         if (stockTake == null) return;

         stockTake.DeletedBy = username;
         stockTake.DeletedOn = DateTime.Now;
         stockTake.ModifiedBy = username;
         stockTake.ModifiedOn = stockTake.DeletedOn;
      }

      public IQueryable<StockTakeItem> FetchStockTakesForProduct(int productId)
      {
         return _context.StockTakeItems.Where(i => i.ProductId == productId).Include(i => i.StockTake);
      }

      public void RemoveItem(int itemId, string username)
      {
         var item = _context.StockTakeItems.SingleOrDefault(i => i.StockTakeItemId == itemId);
         SaveItemDeletion(item, username);
      }

      public bool HasHadStockTake(int productId, int locationId) // has had COMPLETED stock take
      {
         return
            _context.StockTakes.Any(
               st =>
                  st.LocationId == locationId && !st.DeletedOn.HasValue
                                              && st.Status == StockTakeStatus.Complete
                                              && st.StockTakeItems.Any(sti => sti.ProductId == productId));
      }

      public StockTake CreateSingleProductStockTake(int productId, int locationId, int quantity, DateTime stockTakeDateTime, string username, string stockTakeMessage)
      {
         var stockTake = new StockTake
         {
            CreatedBy = username,
            CreatedOn = DateTime.Now,
            LocationId = locationId,
            Message = stockTakeMessage,
            ModifiedBy = username,
            ModifiedOn = DateTime.Now,
            Source = StockTakeSource.User,
            Status = StockTakeStatus.Created,
            StockTakeDate = DateTime.Now,
            SubmittedBy = username,
            SubmittedOn = DateTime.Now
         };

         var stockTakeItem = new StockTakeItem
         {
            CreatedBy = username,
            CreatedOn = DateTime.Now,
            ModifiedBy = username,
            ModifiedOn = DateTime.Now,
            ProductId = productId,
            StockLevel = quantity,
            Status = StockTakeItemStatus.Created
         };
         stockTake.StockTakeItems.Add(stockTakeItem);
         Add(stockTake);
         return stockTake;
      }

      public void AddItem(int stockTakeId, int productId, int stockLevel, string username)
      {
         var stockTakeItem = new StockTakeItem
         {
            CreatedBy = username,
            CreatedOn = DateTime.Now,
            ModifiedBy = username,
            ModifiedOn = DateTime.Now,
            ProductId = productId,
            Status = StockTakeItemStatus.Created,
            StockLevel = stockLevel,
            StockTakeId = stockTakeId
         };
         _context.StockTakeItems.Add(stockTakeItem);
      }

      private void SaveItemDeletion(StockTakeItem item, string username)
      {
         if (item == null) return;
         item.DeletedBy = username;
         item.DeletedOn = DateTime.Now;
      }
   }

   public interface IStockTakeRepository
   {
      IQueryable<StockTake> FetchUserStockTakes();
      StockTake Fetch(int id);
      IQueryable<StockTakeItem> FetchItem(int stockTakeItemId);
      void Add(StockTake stockTake);
      void Update(StockTake product);
      void Commit();
      void AddItem(StockTakeItem stockTakeItem);
      void UpdateItem(StockTakeItem stockTakeItem, string username);
      void RemoveItem(int stockTakeItemId, string username);
      void Remove(int id, string username);
      IQueryable<StockTakeItem> FetchStockTakesForProduct(int productId);
      void UpdateItemStockLevels(int stockTakeId, Dictionary<int, int?> items, string username);
      IQueryable<StockTakeItem> FetchItems(int stockTakeId);
      void UpdateStockTakeDetails(int stockTake, int stockTakeLocationId, DateTime stockTakeDate, string name, string username);
      bool HasHadStockTake(int productId, int locationId);
      StockTake CreateSingleProductStockTake(int productId, int locationId, int quantity, DateTime stockTakeDateTime, string username, string stockTakeMessage);
      void AddItem(int stockTakeId, int productId, int stockLevel, string username);
   }
}