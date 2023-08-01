using Hangfire;
using Microsoft.Practices.Unity;
using Owin;
using System;
using System.Configuration;
using HMS.HealthTrack.Inventory.Common.Constants;
using Serilog;

namespace HMS.HealthTrack.Web.App_Start
{
   public class HangfireConfig
   {
      public static void ConfigureHangFire(IAppBuilder app, UnityContainer container)
      {
         try
         {
            GlobalConfiguration.Configuration.UseSqlServerStorage("LogConnection");
            GlobalConfiguration.Configuration.UseSerilogLogProvider();
            GlobalConfiguration.Configuration.UseActivator(new UnityJobActivator(container));

            app.UseHangfireServer(GetServerJobSettings());
            app.UseHangfireDashboard();
         }
         catch (Exception ex)
         {
            Log.Fatal(ex, "Exception encountered configuring HangFire");
            throw;
         }
      }

      private static BackgroundJobServerOptions GetServerJobSettings()
      {
         var workerConfig = ConfigurationManager.AppSettings["HangFireWorkerCount"];
         var workerCount = 1;
         if(workerConfig != null)int.TryParse(workerConfig, out workerCount);
         return new BackgroundJobServerOptions {Queues = GetQueues(), WorkerCount = workerCount};
      }

      private static string[] GetQueues()
      {
         var queueConfig = ConfigurationManager.AppSettings["HangfireQueues"];
         return string.IsNullOrWhiteSpace(queueConfig) ? new[] { BackgroundQueues.Default } : queueConfig.Split(',');
      }

      public class UnityJobActivator : JobActivator
      {
         private readonly IUnityContainer _hangfireContainer;

         public UnityJobActivator(IUnityContainer hangfireContainer)
         {
            _hangfireContainer = hangfireContainer;
         }

         public override object ActivateJob(Type type)
         {
            return _hangfireContainer.Resolve(type);
         }
      }
   }
}