using System;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using Microsoft.Practices.Unity;
using Serilog;

namespace HMS.HealthTrack.Inventory.OrderingIntegration
{
   public static class OrderingIntegrationUnityConfig
   {
      public static void RegisterOrderingIntegrationComponents(this UnityContainer container)
      {
         // Register Order Processing Service components
         container.RegisterType<IOrderStatusManagementService, OrderStatusManagementService>();
         container.RegisterType<IHealthTrackToOracleOrderConverter, HealthTrackToOracleOrderConverter>();

         container.RegisterType<IOracleOutgoingFileService, OracleOutgoingFtpService>();

         container.RegisterType<IOrderProcessingService, OracleOrderProcessingService>();

         container.RegisterType<ICustomLogger, CustomLogger>(new InjectionConstructor(Log.Logger));
         container.RegisterType<IConfigurationRepository, ConfigurationRepository>();

         container.RegisterType<OraclePurchaseOrderService>(new InjectionConstructor(
             container.Resolve<IOrderStatusManagementService>(),
             container.Resolve<IOraclePurchaseOrderReceiptRepository>()));

         container.RegisterType<OracleErrorReportService>(new InjectionConstructor(
             container.Resolve<IOrderStatusManagementService>(),
             container.Resolve<IOraclePurchaseOrderErrorRepository>()));

         var fileServiceFactory = new Func<OracleInboundFileType, IOracleIncomingFileService>(fileType =>
         {
            switch (fileType)
            {
               case OracleInboundFileType.PurchaseOrder:
                  return container.Resolve<OraclePurchaseOrderService>();

               case OracleInboundFileType.ErrorReport:
                  return container.Resolve<OracleErrorReportService>();

               default:
                  throw new Exception(string.Format("exception: Unable to resolve IOracleIncomingFileService for file type. fileType={0}", fileType));
            }
         });

         container.RegisterType<IInboundOrderReceiptWatcher, OracleInboundFtpWatcher>(new InjectionConstructor(
             container.Resolve<IConfigurationRepository>(),
             fileServiceFactory,
             container.Resolve<ICustomLogger>()));
      }
   }
}
