using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Inventory.OrderingIntegration
{
   public class OracleOrderProcessor : IOrderChannelSubmitter
   {
      private readonly OracleOrderProcessingService _service;
      public string ChannelName => FmisAttributes.FmisOrderChannelName;

      public OracleOrderProcessor(IOrderRepository orderRepo,
         IConfigurationRepository configurationRepository,
         ICustomLogger customLogger, IProductRepository productRepository,IOrderChannelRepository channelRepository)
      {
         _service = OracleOrderProcessingServiceFactory.GetOracleOrderProcessingService(orderRepo,
            new OrderStatusManagementService(orderRepo), 
            configurationRepository,
            customLogger, productRepository,channelRepository);
      }

      public Task SendOrder(int orderId)
      {
         return _service.SubmitOrder(orderId);
      }
   }

}