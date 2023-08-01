using HMS.HealthTrack.Web.App_Start;
using HMS.HealthTrack.Web.Controllers;
using Microsoft.Practices.Unity;
using Serilog;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using HMS.HealthTrack.Web.Data.Model.Security;
using HMS.HealthTrack.Web.Data.Repositories.Security;
using Unity.Mvc5;

namespace HMS.HealthTrack.Web
{
   public class WebApiApplication : HttpApplication
   {
      private IUserRepository _userRepo;

      private IUserRepository UserRepo
      {
         get { return _userRepo ?? (_userRepo = new UserRepository(new Security())); }
      }

      /// <summary>
      /// Web specific configuration 
      /// </summary>
      /// <remarks>Application wide config for OwinStartup is in HMS.HealthTrack.Web.Startup.Configuration</remarks>
      protected void Application_Start()
      {
         SetupLogging();
         Log.Information("HealthTrack Web has started");

         //MVC Areas
         AreaRegistration.RegisterAllAreas();

         //MVC Dependency injection
         RegisterUnityComponents();
      }

      private static void RegisterUnityComponents()
      {
         var container = new UnityContainer();
         container.RegisterComponents();
         DependencyResolver.SetResolver(new UnityDependencyResolver(container));
      }

      private static void SetupLogging()
      {
         Log.Logger = LoggerConfig.CreateLogger();
      }

      protected void Application_End()
      {
         Log.Information("HealthTrack Web is closing");
      }

      protected void Application_Error(object sender, EventArgs e)
      {
         var httpContext = ((WebApiApplication)sender).Context;
         var currentController = " ";
         var currentAction = " ";
         var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

         if (currentRouteData != null)
         {
            if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
            {
               currentController = currentRouteData.Values["controller"].ToString();
            }

            if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
            {
               currentAction = currentRouteData.Values["action"].ToString();
            }
         }

         var ex = Server.GetLastError();
         var controller = new ErrorController();
         var routeData = new RouteData();
         var action = "Index";

         if (ex is HttpException)
         {
            var httpEx = ex as HttpException;

            switch (httpEx.GetHttpCode())
            {
               case 404:
                  action = "NotFound";
                  break;

               case 401:
                  action = "AccessDenied";
                  break;
            }
         }


         Log.Error(ex, string.Format("Unhandled exception at {0}/{1}", currentController, currentAction));

         httpContext.ClearError();
         httpContext.Response.Clear();
         httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
         httpContext.Response.TrySkipIisCustomErrors = true;

         routeData.Values["controller"] = "Error";
         routeData.Values["action"] = action;

         controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);

         ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
      }
   }
}