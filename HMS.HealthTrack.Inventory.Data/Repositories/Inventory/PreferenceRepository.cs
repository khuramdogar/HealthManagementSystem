using System.Linq;
using System.Security.Principal;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class PreferenceRepository : IPreferenceRepository
   {
      private readonly IDbContextInventoryContext _context;

      public PreferenceRepository(IDbContextInventoryContext context)
      {
         _context = context;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public IQueryable<UserPreference> GetPreferencesForUser(IIdentity identity)
      {
         return GetPreferenceForUsername(identity.Name);
      }

      public bool HasUnconfiguredPreferences(IIdentity identity)
      {
         var preference = GetPreferenceForUsername(identity.Name).SingleOrDefault();
         return preference == null || preference.LocationId == null;
      }

      public void Update(UserPreference model)
      {
         var existing = _context.UserPreferences.SingleOrDefault(up => up.User_ID.Equals(model.User_ID));
         if (existing == null)
         {
            _context.UserPreferences.Add(model);
            return;
         }

         _context.Entry(existing).CurrentValues.SetValues(model);
      }

      private IQueryable<UserPreference> GetPreferenceForUsername(string username)
      {
         return _context.UserPreferences.Where(u => u.User_ID.Equals(username));
      }
   }

   public interface IPreferenceRepository
   {
      IQueryable<UserPreference> GetPreferencesForUser(IIdentity identity);
      bool HasUnconfiguredPreferences(IIdentity identity);
      void Update(UserPreference model);
      void Commit();
   }
}