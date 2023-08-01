using System.Threading.Tasks;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal interface IOracleOutgoingFileService
    {
        Task Send(OracleOutboundOrder oracleOrderItem);
    }
}
