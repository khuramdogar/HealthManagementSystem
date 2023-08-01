
namespace HMS.HealthTrack.Inventory.OrderingIntegration
{
    /// <summary>
    /// Responsible for managing the status of orders in the repository
    /// </summary>
    public interface IOrderStatusManagementService
    {
        void UpdateOrderToCreated(int orderId);

        void UpdateOrderToRequested(int orderId);

        void UpdateOrderToOrdered(int orderId);

        void UpdateOrderToFailed(int orderId);
    }
}