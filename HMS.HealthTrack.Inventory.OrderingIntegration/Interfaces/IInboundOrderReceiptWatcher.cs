using System.Threading;
using System.Threading.Tasks;

namespace HMS.HealthTrack.Inventory.OrderingIntegration
{
    /// <summary>
    /// Class responsible for monitoring incoming order receipts
    /// </summary>
    public interface IInboundOrderReceiptWatcher
    {
        Task StartMonitoringIncomingFiles(CancellationToken cancellationToken);
    }
}
