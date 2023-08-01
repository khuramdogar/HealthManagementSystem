using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
   /// <summary>
   /// Responsible for converting health track order data into oracle specific format
   /// </summary>
   internal interface IHealthTrackToOracleOrderConverter
   {
      OracleOutboundOrder ConvertOrder(Order order);
   }
}
