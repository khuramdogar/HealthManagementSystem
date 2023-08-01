using System;
using System.Configuration;
using HMS.HealthTrack.Web.Data.Model.Clinical;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Model.Security;
using Microsoft.Practices.Unity;
using Serilog;

namespace HMS.HealthTrack.Web.Dependencies
{
   internal static class DataContexts
   {
      /// <summary>
      /// Register the Db Contexts with the IoC container
      /// </summary>
      internal static void RegisterDbContexts(this IUnityContainer container)
      {
         try
         {
            // Inventory Context
            var inventoryConnectionString = ConfigurationManager.ConnectionStrings["InventoryContext"];

            if (inventoryConnectionString == null)
               throw new ApplicationException("Connection string InventoryContext not found");

            container.RegisterType<IDbContextInventoryContext, InventoryContext>(new InjectionConstructor(inventoryConnectionString));

            // Security Context
            var securityConnectionString = ConfigurationManager.ConnectionStrings["Security"];

            if (securityConnectionString == null)
               throw new ApplicationException("Connection string Security not found");

            container.RegisterType<IDbContextSecurity, Security>(new InjectionConstructor(securityConnectionString));

            // Clinical Context
            var clinicalConnectionString = ConfigurationManager.ConnectionStrings["ClinicalContext"];

            if (clinicalConnectionString == null)
               throw new ApplicationException("Connection string ClinicalContext not found");

            container.RegisterType<IDbContextClinicalContext, ClinicalContext>(new InjectionConstructor(clinicalConnectionString));

            var healthTrackInventoryConnectionString = ConfigurationManager.ConnectionStrings["HealthTrackInventoryContext"];
            if (healthTrackInventoryConnectionString == null)
               throw new ApplicationException("Connection string HealthTrackInventoryContext not found");

            container.RegisterType<IHealthTrackInventoryContext, HealthTrackInventoryContext>(
               new InjectionConstructor(healthTrackInventoryConnectionString.ConnectionString)); // pass in connection string
         }
         catch (Exception exception)
         {
            Log.Fatal(exception, "Exception encountered RegisteringDbContexts");

            // Don't rethrow exceptions here
            // i.e. if Web team has decided to add another connection string
            // The API installations shouldn't be stopped from initialising.
         }
      }
   }
}