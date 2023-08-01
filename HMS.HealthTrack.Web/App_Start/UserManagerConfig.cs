using HMS.HealthTrack.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common.Constants;

namespace HMS.HealthTrack.Web
{
   public class SecurityUserManager : UserManager<ApplicationUser>
   {
      public SecurityUserManager(IUserStore<ApplicationUser> store)
         : base(store)
      {

      }

      public static SecurityUserManager Create(IdentityFactoryOptions<SecurityUserManager> options, IOwinContext context)
      {
         throw new NotImplementedException();
      }
   }

   public class ActiveDirectoryUserManager : SecurityUserManager
   {
      private readonly PrincipalContext _context;
      public ActiveDirectoryUserManager(IUserStore<ApplicationUser> store, PrincipalContext context)
         : base(store)
      {
         _context = context;
      }

      public override async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
      {
         return await Task.FromResult(_context.ValidateCredentials(user.UserName, password, ContextOptions.Negotiate));
      }

      public override async Task<ClaimsIdentity> CreateIdentityAsync(ApplicationUser user, string authenticationType)
      {
         var userPrincipal = UserPrincipal.FindByIdentity(_context, IdentityType.SamAccountName, user.UserName);

         var claims = new List<Claim>();
         if (userPrincipal != null)
         {
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserName));
            var htUsername = user.UserName;
            var domain = ConfigurationManager.AppSettings.Get("ActiveDirectoryDomainName");
            if (!string.IsNullOrWhiteSpace(domain))
               htUsername = string.Format(@"{0}\{1}", domain, htUsername);
            claims.Add(new Claim(InventoryConstants.HealthTrackUsername, htUsername));
         }

         var id = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
         return await Task.FromResult(id);
      }

      public new static SecurityUserManager Create(IdentityFactoryOptions<SecurityUserManager> options, IOwinContext context)
      {
         var manager = new ActiveDirectoryUserManager(new UserStore<ApplicationUser>(), context.Get<PrincipalContext>());
         // Configure validation logic for usernames
         manager.UserValidator = new UserValidator<ApplicationUser>(manager)
         {
            AllowOnlyAlphanumericUserNames = false,
            RequireUniqueEmail = true
         };

         // Configure user lockout defaults
         manager.UserLockoutEnabledByDefault = true;
         manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
         manager.MaxFailedAccessAttemptsBeforeLockout = 5;

         var dataProtectionProvider = options.DataProtectionProvider;
         if (dataProtectionProvider != null)
         {
            manager.UserTokenProvider =
                new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
         }
         return manager;
      }

   }

   public class DummyUserManager : SecurityUserManager
   {
      public DummyUserManager()
         : base(new UserStore<ApplicationUser>())
      {
      }

      public new static SecurityUserManager Create(IdentityFactoryOptions<SecurityUserManager> options, IOwinContext context)
      {
         return new DummyUserManager();
      }
   }
}
