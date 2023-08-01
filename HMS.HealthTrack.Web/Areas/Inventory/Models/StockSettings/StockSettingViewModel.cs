using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockSettings
{
   public class StockSettingViewModel : IMapFrom<StockSetting>
   {
      public string SettingId { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public bool Enabled { get; set; }
   }
}