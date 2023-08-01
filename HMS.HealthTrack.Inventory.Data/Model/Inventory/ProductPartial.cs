using System.Linq;
using HMS.HealthTrack.Inventory.Common.Constants;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public partial class Product
   {
      public bool RequiresSerial
      {
         get { return ProductSettings.Any(p => p.SettingId == InventoryConstants.StockSettings.RequiresSerialNumber); }
      }

      public bool RequiresBatch
      {
         get { return ProductSettings.Any(p => p.SettingId == InventoryConstants.StockSettings.RequiresBatchNumber); }
      }

      public bool Unorderable
      {
         get { return ProductSettings.Any(p => p.SettingId == InventoryConstants.StockSettings.Unorderable); }
      }

      public bool HasHadStockTake
      {
         get { return StockTakeItems.Any(sti => !sti.DeletedOn.HasValue && sti.Status == StockTakeItemStatus.Complete); }
      }

      public bool Unclassified => !ProductCategories.Any();

      public bool InStock
      {
         get { return Stocks.Any(s => s.StockStatus == StockStatus.Available && !s.DeletedOn.HasValue && s.Quantity > 0); }
      }
   }
}