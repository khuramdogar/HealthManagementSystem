using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork
{
   public class LocationUnitOfWork : ILocationUnitOfWork
   {
      private readonly IDbContextInventoryContext _context;
      private readonly ICustomLogger _logger;

      public LocationUnitOfWork(IDbContextInventoryContext context, ICustomLogger logger)
      {
         _context = context;
         _logger = logger;

         StockLocationRepository = new StockLocationRepository(context);
         HealthTrackLocationRepository = new HealthTrackLocationRepository(context);
      }

      public IStockLocationRepository StockLocationRepository { get; }

      public IHealthTrackLocationRepository HealthTrackLocationRepository { get; }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public StockLocationModel GetStockLocation(int id)
      {
         var stockLocation = StockLocationRepository.Find(id);
         var model = new StockLocationModel
         {
            StockLocation = stockLocation
         };

         if (!stockLocation.StockLocationMappings.Any()) return model;
         var htLocationIds = stockLocation.StockLocationMappings.Select(m => m.HealthTrackLocationId).ToList();
         var healthTrackLocations = HealthTrackLocationRepository.FindAll().Where(htl => htLocationIds.Contains(htl.location_ID));
         model.HealthTrackLocations = healthTrackLocations.ToList();

         return model;
      }
   }

   public interface ILocationUnitOfWork
   {
      IStockLocationRepository StockLocationRepository { get; }
      IHealthTrackLocationRepository HealthTrackLocationRepository { get; }
      StockLocationModel GetStockLocation(int id);
      void Commit();
   }
}