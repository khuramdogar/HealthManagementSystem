using System.Threading.Tasks;

namespace HMS.HealthTrack.Inventory.Common
{
   public interface IOrderChannelSubmitter
   {
      string ChannelName { get; }
      Task SendOrder(int orderId);
   }
}
