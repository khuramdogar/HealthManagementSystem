using System.Security.Principal;
using HMS.HealthTrack.Inventory.Common.Constants;

namespace HMS.HealthTrack.Web.Utils
{
   public static class SecurityUtils
   {
      public static string GetCachedCredentialsHandle(IPrincipal user)
      {
         return user == null ? string.Empty : string.Concat(user.Identity.Name, SecurityConstants.PermissionCacheSuffix);
      }
   }
}
