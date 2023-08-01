using HMS.HealthTrack.Web.Areas.Inventory.Utils;
using HMS.HealthTrack.Web.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Model.Security;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Security;

namespace HMS.HealthTrack.Web
{
   public class HealthTrackAuthorize : AuthorizeAttribute
   {
      private static IUserRepository _userRepo;
      private static IPreferenceRepository _preferenceRepository;

      public static IUserRepository UserRepo
      {
         get { return _userRepo ?? (_userRepo = new UserRepository(new Security())); }
         set { _userRepo = value; }
      }

      public static IPreferenceRepository PreferenceRepository
      {
         get
         {
            return new PreferenceRepository(new InventoryContext());
         }
         set { _preferenceRepository = value; }
      }

      // Custom property
      public string HealthTrackPermission { get; set; }
      public string HealthTrackGroup { get; set; }

      protected override bool AuthorizeCore(HttpContextBase httpContext)
      {
         if (!httpContext.User.Identity.IsAuthenticated)
         {

            return false;
         }

         var isAuthorized = base.AuthorizeCore(httpContext);
         if (!isAuthorized || httpContext.Session == null) return false;

         var identity = (ClaimsIdentity)httpContext.User.Identity;
         var claims = identity.Claims;
         var htUsernameClaim = claims.SingleOrDefault(c => c.Type == InventoryConstants.HealthTrackUsername);
         var htUsername = htUsernameClaim != null ? htUsernameClaim.Value : string.Empty;

         //get permissions
         var rolesHandle = SecurityUtils.GetCachedCredentialsHandle(httpContext.User);
         var healthTrackPermissions = httpContext.Session[rolesHandle] as IEnumerable<HealthTrackPermission> ??
                                      UserRepo.GetPermissionsForUser(htUsername);
         httpContext.Session[rolesHandle] = healthTrackPermissions;

         //get preferences
         var preferencesHandle = UserUtils.GetUnconfiguredInventoryPreferencesHandle(httpContext.User);
         var preferences = PreferenceRepository.HasUnconfiguredPreferences(httpContext.User.Identity);
         if (preferences)
         {
            httpContext.Session[preferencesHandle] = preferences;
         }

         if (healthTrackPermissions == null)
         {
            Log.Fatal("HealthTrack permissions are null for user {UserId}", httpContext.User.Identity.Name);
         }

         var granted = HealthTrackPermission != null && healthTrackPermissions != null && healthTrackPermissions.Any(
            (p => HealthTrackGroup != null && (p.Keyword.ToLower() == HealthTrackGroup.ToLower())
               || (HealthTrackPermission != null) && p.Keyword == HealthTrackPermission));

         if (!granted)
            Log.Warning(string.Format("User {0} not authorized for {1}", httpContext.User.Identity.Name, httpContext.Request.RawUrl));

         return granted;
      }

      protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
      {
         if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
         {
            filterContext.Result = new RedirectToRouteResult("DefaultWeb", new RouteValueDictionary(new { controller = "Account", action = "Login", returnUrl = filterContext.HttpContext.Request.Url.GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped) }));
            return;
         }

         filterContext.Result = new RedirectToRouteResult("DefaultWeb", new RouteValueDictionary(new { controller = "Error", action = "AccessDenied" }));
      }
   }
}