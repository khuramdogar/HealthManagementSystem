using HMS.HealthTrack.Web.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using HMS.HealthTrack.Inventory.Common.Constants;

namespace HMS.HealthTrack.Web.App_Start
{
   public class SecurityAuthenticationManager : SignInManager<ApplicationUser, string>
   {
      public SecurityAuthenticationManager(SecurityUserManager userManager, IAuthenticationManager authenticationManager)
         : base(userManager, authenticationManager)
      {
      }

      public static SecurityAuthenticationManager Create(IdentityFactoryOptions<SecurityAuthenticationManager> options,
         IOwinContext context)
      {
         throw new NotImplementedException();
      }
   }

   public class ActiveDirectoryAuthenticationManager : SecurityAuthenticationManager
   {
      public ActiveDirectoryAuthenticationManager(SecurityUserManager userManager,
         IAuthenticationManager authenticationManager)
         : base(userManager, authenticationManager)
      {
      }

      public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
      {
         return user.GenerateUserIdentityAsync((ActiveDirectoryUserManager)UserManager);
      }

      public new static SecurityAuthenticationManager Create(
         IdentityFactoryOptions<SecurityAuthenticationManager> options, IOwinContext context)
      {
         return new ActiveDirectoryAuthenticationManager(context.GetUserManager<SecurityUserManager>(),
            context.Authentication);
      }
   }

   public class DummyAuthenticationManager : SecurityAuthenticationManager
   {
      public DummyAuthenticationManager(SecurityUserManager userManager, IAuthenticationManager authenticationManager)
         : base(userManager, authenticationManager)
      {
         if (HttpContext.Current != null)
         {
            var alias = ConfigurationManager.AppSettings.Get("AnonymousUserAlias");
            HttpContext.Current.User = string.IsNullOrWhiteSpace(alias)
               ? new GenericPrincipal(new GenericIdentity("Anonymous"), new string[0])
               : new GenericPrincipal(new GenericIdentity(alias), new string[0]);
            var identity = (ClaimsIdentity)HttpContext.Current.User.Identity;
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, HttpContext.Current.User.Identity.Name));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, HttpContext.Current.User.Identity.Name));
            claims.Add(new Claim(InventoryConstants.HealthTrackUsername, HttpContext.Current.User.Identity.Name));
            identity.AddClaims(claims);
         }
      }

      public new static SecurityAuthenticationManager Create(
         IdentityFactoryOptions<SecurityAuthenticationManager> options, IOwinContext context)
      {
         return new DummyAuthenticationManager(context.GetUserManager<SecurityUserManager>(), context.Authentication);
      }
   }
}