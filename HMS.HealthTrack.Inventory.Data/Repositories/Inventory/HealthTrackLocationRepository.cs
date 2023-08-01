using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class HealthTrackLocationRepository : IHealthTrackLocationRepository
   {
      private readonly IDbContextInventoryContext _context;

      public HealthTrackLocationRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public IQueryable<HealthTrackLocation> FindAll()
      {
         return _context.HealthTrackLocations.Where(htl => htl.deleted == false);
      }
   }

   public interface IHealthTrackLocationRepository
   {
      IQueryable<HealthTrackLocation> FindAll();
   }
}