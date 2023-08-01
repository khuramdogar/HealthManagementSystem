using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.OrderChannels
{
   public class OrderChannelModel : IMapFrom<OrderChannel>
   {
      public int OrderChannelId { get; set; }
      public string Name { get; set; }
   }
}