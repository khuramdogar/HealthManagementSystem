
using System.Threading.Tasks;

namespace HMS.HealthTrack.Inventory.OrderingIntegration
{
    /// <summary>
    /// Responsible for submitting orders to 3rd party providers
    /// </summary>
    public interface IOrderProcessingService
    {
        Task SubmitOrder(int healthTrackOrderId);
    }
}
