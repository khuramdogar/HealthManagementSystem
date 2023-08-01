using System.Linq;
using System.Security.Principal;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Utils
{
   public static class PreferenceHelper
   {
      public static int GetPreferredStockLocation(IPreferenceRepository preferenceRepository,
         IPropertyProvider propertyProvider, IIdentity identity)
      {
         var preferences = preferenceRepository.GetPreferencesForUser(identity).SingleOrDefault();
         var preferredLocation = preferences != null ? preferences.LocationId : propertyProvider.DefaultStockLocationId;
         return preferredLocation ?? 0;
      }
   }
}