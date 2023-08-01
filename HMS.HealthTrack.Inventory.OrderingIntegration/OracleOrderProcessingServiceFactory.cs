using System.Configuration;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using HMS.HealthTrack.Web.Data.Model.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Inventory.OrderingIntegration
{
   internal class OracleOrderProcessingServiceFactory
   {
      internal static OracleOrderProcessingService GetOracleOrderProcessingService(IOrderRepository orderRepo,
         IOrderStatusManagementService statusManagementService,
         IConfigurationRepository configurationRepository,
         ICustomLogger customLogger,
         IProductRepository productRepository,
         IOrderChannelRepository channelRepository)
      {
         var channel = channelRepository.GetAvailableChannels().SingleOrDefault(oc => oc.Name == FmisAttributes.FmisOrderChannelName);
         var orderConverter = new HealthTrackToOracleOrderConverter(productRepository, channel);
         var timeProvider = new TimeProvider();
         IOracleOutgoingFileService fileService;
         var method = configurationRepository.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.OrderOutputMethod);

         switch (method)
         {
            case "ftp":
               fileService = new OracleOutgoingFtpService(configurationRepository, timeProvider, customLogger);
               break;
            case "file":
               fileService = new OracelOutgoingFileSystemService(configurationRepository, timeProvider, customLogger);
               break;
            default:
               throw new ConfigurationErrorsException($"Unsupported type {method} for OracleOutgoingFileService");
         }

         return new OracleOrderProcessingService(orderRepo, statusManagementService, orderConverter, fileService, customLogger);
      }
   }
}