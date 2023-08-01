using System;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
   internal class OracleOrderProcessingService : IOrderProcessingService
   {
      private readonly IOrderRepository _orderRepo;
      private readonly IOrderStatusManagementService _statusManagementService;
      private readonly IHealthTrackToOracleOrderConverter _healthTrackToOracleOrderConverter;
      private readonly IOracleOutgoingFileService _outgoingFileService;
      private readonly ICustomLogger _customLogger;

      public OracleOrderProcessingService(
          IOrderRepository orderRepo,
          IOrderStatusManagementService statusManagementService,
          IHealthTrackToOracleOrderConverter healthTrackToOracleOrderConverter,
          IOracleOutgoingFileService outgoingFileService,
          ICustomLogger customLogger)
      {
         _orderRepo = orderRepo;
         _statusManagementService = statusManagementService;
         _healthTrackToOracleOrderConverter = healthTrackToOracleOrderConverter;
         _outgoingFileService = outgoingFileService;
         _customLogger = customLogger;
      }

      public Task SubmitOrder(int healthTrackOrderId)
      {
         _customLogger.Information(string.Format("msg=OracleOrderProcessingService: Submit Order, orderId={0}", healthTrackOrderId));

         return Task.Run(() =>
         {
            _statusManagementService.UpdateOrderToRequested(healthTrackOrderId);

            _customLogger.Information(string.Format("msg=OracleOrderProcessingService: Order status updated to submitted, orderId={0}", healthTrackOrderId));

            try
            {
               var oracleOrder = _healthTrackToOracleOrderConverter.ConvertOrder(_orderRepo.Find(healthTrackOrderId));

               _outgoingFileService.Send(oracleOrder).Wait();

               _customLogger.Information(string.Format("msg=OracleOrderProcessingService: Order sent to oracle, orderId={0}", healthTrackOrderId));
            }
            catch (Exception ex)
            {
               _statusManagementService.UpdateOrderToFailed(healthTrackOrderId);
               _customLogger.Error(ex.Message);
            }
         });
      }
   }
}
