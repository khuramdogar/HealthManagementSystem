using System.Security.Principal;
using HMS.HealthTrack.Inventory.Common.Constants;

namespace HMS.HealthTrack.Web.Areas.Inventory.Utils
{
   public static class UserUtils
   {
      public static string GetUnconfiguredInventoryPreferencesHandle(IPrincipal user)
      {
         return user == null ? string.Empty : string.Concat(user.Identity.Name, InventoryConstants.UnconfiguredInventoryPrefsSuffix);
      }


   }
}
