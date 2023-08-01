using Microsoft.Practices.Unity;
using Serilog;
using System;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration;
using HMS.HealthTrack.Web.Dependencies;

namespace HMS.HealthTrack.Web
{
   /// <summary>
   /// Class containing extension methods for registering components with Unity IoC container
   /// </summary>
   public static class UnityConfig
   {
      /// <summary>
      /// Register application components with IoC container
      /// </summary>
      public static void RegisterComponents(this UnityContainer container)
      {
         try
         {
            container.ConfigMediator();

            container.RegisterCommonComponents();

            container.RegisterDbContexts();

            container.RegisterRepositories();

            container.RegisterUnitsOfWork();

            // Ordering integration components
            container.RegisterOrderingIntegrationComponents();
         }
         catch (Exception exception)
         {
            Log.Fatal(exception, "Exception encountered registering Unity Components");
            throw;
         }
      }

      /// <summary>
      /// Register common components
      /// </summary>
      /// <param name="container"></param>
      private static void RegisterCommonComponents(this IUnityContainer container)
      {
         container.RegisterInstance(Log.Logger as ICustomLogger);
         container.RegisterType<ITimeProvider, TimeProvider>();
      }
   }
}