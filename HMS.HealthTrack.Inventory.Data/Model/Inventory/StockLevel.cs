using System.Collections.Generic;
using System.Linq;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public class StockLevel
   {
      public IEnumerable<Stock> Stock { get; set; }

      public int StockCount
      {
         get { return Stock.Where(xx => StorageLocation != null ? xx.StoredAt == StorageLocation.LocationId : xx.StoredAt == null).Sum(xx => xx.Quantity); }
      }

      public int TotalStock
      {
         get { return Stock.Sum(xx => xx.Quantity); }
      }

      public Product Product { get; set; }

      public StockLocation StorageLocation { get; set; }

      public int RequestedStock
      {
         get
         {
            return
               Product.StockRequests.Where(
                  xx =>
                     StorageLocation != null
                        ? xx.RequestLocationId == StorageLocation.LocationId
                        : xx.RequestLocationId == null).Aggregate(0, (current, request) => current + request.RequestedQuantity);
         }
      }

      public bool HasSerial
      {
         get { return Stock.Where(xx => StorageLocation != null ? xx.StoredAt == StorageLocation.LocationId : xx.StoredAt == null).Any(s => !string.IsNullOrWhiteSpace(s.SerialNumber)); }
      }
   }
}