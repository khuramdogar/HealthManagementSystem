using System;
using HMS.HealthTrack.Web.App_Start;
using HMS.HealthTrack.Web.Utils;
using Microsoft.Owin;
using Microsoft.Practices.Unity;
using Owin;
using Serilog;
using System.Security.Claims;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FluentValidation.Attributes;
using FluentValidation.WebApi;
using HMS.HealthTrack.Inventory.Common;
using Unity.WebApi;

[assembly: OwinStartup(typeof(HMS.HealthTrack.Web.Startup))]

namespace HMS.HealthTrack.Web
{
   public partial class Startup
   {
      public void Configuration(IAppBuilder app)
      {
         try
         {
            var config = new HttpConfiguration();

            ConfigureExceptionLogging(config);

            ConfigureAuth(app);

            //Config API (routes etc)
            RouteConfig.RegisterApiRoutes(config);
            config.EnsureInitialized(); // This needs to be called after the above in Web Api 2.0 to ensure attribute routing works

            //Config api dependency injection
            var container = RegisterComponents(config);

            //Configure Web routes
            RouteConfig.RegisterWebRoutes(RouteTable.Routes);

            Log.Information("HealthTrack Web started");

            //Configure script/css bundles
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Format JSON messages
            FormatterConfig.SetupJson(config);

            // Configure Web API for self-host. 
            app.UseWebApi(config);

            //Global Filters
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            AutoMapperConfig.Configure();

            ModelMetadataProviders.Current = new CustomDataAnnotationsModelMetadataProvider();

            JavascriptEnabledEnums.LoadTypes();

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            HangfireConfig.ConfigureHangFire(app, container);

            FluentValidationModelValidatorProvider.Configure(config);

         }
         catch (Exception exception)
         {
            Log.Fatal(exception, "Exception encountered performing Startup.Configuration");
            
         }
      }


      /// <summary>
      /// Register application components with HttpConfiguration IoC container
      /// </summary>
      public static UnityContainer RegisterComponents(HttpConfiguration config)
      {
         var container = new UnityContainer();
         container.RegisterComponents();
         config.DependencyResolver = new UnityDependencyResolver(container);
         return container;
      }

      private void ConfigureExceptionLogging(HttpConfiguration configuration)
      {
         var customLogger = Log.Logger as ICustomLogger;
         if (customLogger == null)
         {
            // create new logger
            Log.Logger = LoggerConfig.CreateLogger();
         }
         //Configure logging
         configuration.Services.Add(typeof(IExceptionLogger), Log.Logger);
         configuration.Services.Add(typeof(IExceptionLogger), new ApiExceptionLogger());
      }
   }
}