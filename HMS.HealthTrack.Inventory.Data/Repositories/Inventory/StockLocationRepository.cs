using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class StockLocationRepository : IStockLocationRepository
   {
      private readonly IDbContextInventoryContext _context;

      public StockLocationRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public StockLocation Find(int id)
      {
         return _context.StockLocations.Include(xx => xx.Address).SingleOrDefault(xx => xx.LocationId == id);
      }

      public IQueryable<StockLocation> FindAll()
      {
         return _context.StockLocations.Where(xx => xx.DeletedOn == null);
      }

      public IQueryable<StockLocation> FindAllWithDeleted()
      {
         return _context.StockLocations;
      }

      public void Remove(StockLocation location)
      {
         var existing = Find(location.LocationId);
         if (existing == null) return;
         location.DeletedOn = DateTime.Now;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public void Update(StockLocation location)
      {
         var existing = Find(location.LocationId);
         if (existing == null) return;

         var existingAddress = _context.Addresses.SingleOrDefault(xx => xx.AddressId == location.AddressId);
         if (existingAddress == null)
         {
            _context.Addresses.Add(location.Address);
            location.AddressId = location.Address.AddressId;
         }
         else
         {
            _context.Entry(existingAddress).CurrentValues.SetValues(location.Address);
         }

         var selectedMappings = location.StockLocationMappings == null
            ? new List<int>()
            : location.StockLocationMappings.Select(slm => slm.HealthTrackLocationId);

         var mappingsToRemove =
            existing.StockLocationMappings.Where(
               slm => !selectedMappings.Contains(slm.HealthTrackLocationId)).ToList();
         foreach (var mapping in mappingsToRemove)
         {
            existing.StockLocationMappings.Remove(mapping);
            _context.Entry(mapping).State = EntityState.Deleted;
         }

         // add mappings which don't exist
         var existingMappings = existing.StockLocationMappings.Select(slm => slm.HealthTrackLocationId);
         foreach (var mapping in selectedMappings.Where(sm => !existingMappings.Contains(sm)))
            existing.StockLocationMappings.Add(new StockLocationMapping
            {
               HealthTrackLocationId = mapping
            });

         location.LastModifiedOn = DateTime.Now;
         _context.Entry(existing).CurrentValues.SetValues(location);
         _context.Entry(existing).Property(p => p.LogoImage).IsModified = false;
         _context.Entry(existing).Property(p => p.CreatedBy).IsModified = false;
      }

      public IQueryable<StockLocation> FindLocations(List<int> ids)
      {
         var locations = FindAll().Where(xx => ids.Contains(xx.LocationId));
         return locations;
      }

      public IQueryable<StockLocation> FindDeliveryLocations()
      {
         var locations = FindAll();
         return locations;
      }

      public void Add(StockLocation location)
      {
         location.LastModifiedOn = DateTime.Now;
         _context.StockLocations.Add(location);
         _context.Addresses.Add(location.Address);
      }

      public HealthTrackLocationMapper GetHealthTrackLocationMapper(int healthTrackLocationId)
      {
         var locations = _context.StockLocationMappings.Where(slm => slm.HealthTrackLocationId == healthTrackLocationId)
            .Select(slm => slm.Location).Where(sl => !sl.DeletedOn.HasValue);
         if (!locations.Any()) return null;

         return new HealthTrackLocationMapper
         {
            HealthTrackLocationId = healthTrackLocationId,
            StockLocations = locations.Select(s => s.LocationId).ToList()
         };
      }
   }

   public interface IStockLocationRepository
   {
      StockLocation Find(int id);
      void Remove(StockLocation location);
      IQueryable<StockLocation> FindAll();
      void Add(StockLocation newEntity);
      void Commit();
      void Update(StockLocation location);
      IQueryable<StockLocation> FindLocations(List<int> ids);
      IQueryable<StockLocation> FindDeliveryLocations();
      IQueryable<StockLocation> FindAllWithDeleted();
      HealthTrackLocationMapper GetHealthTrackLocationMapper(int healthTrackLocationId);
   }
}