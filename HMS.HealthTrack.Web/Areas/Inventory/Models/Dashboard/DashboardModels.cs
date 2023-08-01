using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Dashboard
{
   [ModelMetaType(typeof(DashboardNotificationMeta))]
   public class DashboardNotificationView : IMapFrom<DashboardNotification>
   {
      public string DashboardNotificationId { get; set; }
      public string Icon { get; set; }
      public string Title { get; set; }
      public string Description { get; set; }
      public string Link { get; set; }
      public string LinkText { get; set; }
      public int Priority { get; set; }
      public bool ShowWhenZero { get; set; }
      public int ItemCount { get; set; }
      public string Area { get; set; }
   }
}
