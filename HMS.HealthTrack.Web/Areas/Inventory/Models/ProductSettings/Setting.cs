using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ProductSettings
{
   public class Setting : IMapFrom<StockSetting>
   {
      public string SettingId { get; set; }
      public string Name { get; set; }
   }
}
