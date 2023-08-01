using System.Security.Principal;

namespace HMS.HealthTrack.Inventory.Common
{
   public static class IIdentityEx
   {
      public static string ShortName(this IIdentity identity)
      {
         if (identity == null) return string.Empty;
         return identity.Name.Contains(@"\") ? identity.Name.Split(new[] {'\\'})[1] : identity.Name;
      }
   }
}