using HMS.HealthTrack.Web.App_Start;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Configuration;
using System.DirectoryServices.AccountManagement;

namespace HMS.HealthTrack.Web
{
   public partial class Startup
   {
      static Startup()
      {
         PublicClientId = "self";
      }

      public static string PublicClientId { get; private set; }

      // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
      public void ConfigureAuth(IAppBuilder app)
      {
         var authenticationMode = ConfigurationManager.AppSettings.Get("AuthenticationMode");
         Serilog.Log.Information("Starting with authentication mode {AuthenticationMode}", authenticationMode);
         switch (authenticationMode)
         {
            case "ActiveDirectory":
               // Enable the application to use a cookie to store information for the signed in user
               // and to use a cookie to temporarily store information about a user logging in with a third party login provider
               var timeoutDuration = int.Parse(ConfigurationManager.AppSettings.Get("CookieTimeoutMinutes"));
               app.UseCookieAuthentication(new CookieAuthenticationOptions
               {
                  AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                  LoginPath = new PathString("/Account/Login"),
                  ExpireTimeSpan = TimeSpan.FromMinutes(timeoutDuration),
                  CookieSecure = CookieSecureOption.Never,
                  LogoutPath = new PathString("/Account/Logout")
               });

               var domain = ConfigurationManager.AppSettings.Get("ActiveDirectoryDomain");
               var container = ConfigurationManager.AppSettings.Get("ActiveDirectoyContainer");
               var username = ConfigurationManager.AppSettings.Get("ActiveDirectoryUsername");
               var password = ConfigurationManager.AppSettings.Get("ActiveDirectoryPassword");
               if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
               {
                  app.CreatePerOwinContext(
                     () => new PrincipalContext(ContextType.Domain, domain, container));
               }
               else
               {
                  app.CreatePerOwinContext(() => new PrincipalContext(ContextType.Domain, domain, container, username, password));
               }

               app.CreatePerOwinContext<SecurityUserManager>(ActiveDirectoryUserManager.Create);
               app.CreatePerOwinContext<SecurityAuthenticationManager>(ActiveDirectoryAuthenticationManager.Create);

               break;
            case "Windows":
               throw new NotImplementedException();
               break;
            case "HealthTrack":
               throw new NotImplementedException();
               break;
            case "None":
               app.CreatePerOwinContext<SecurityUserManager>(DummyUserManager.Create);
               app.CreatePerOwinContext<SecurityAuthenticationManager>(DummyAuthenticationManager.Create);
               break;
            default:
               Serilog.Log.Error("No authentication mode set");
               throw new Exception("Authentication mode not set");
               break;
         }
      }
   }
}
