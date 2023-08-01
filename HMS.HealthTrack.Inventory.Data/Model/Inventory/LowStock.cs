namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   /// <summary>
   ///    Model to hold low stock and relevant information.
   ///    Requires includes from stock to Product and Product's Stock Requests
   /// </summary>
   public class LowStock
   {
      public int StockCount { get; set; }

      public Product Product { get; set; }

      public int ReorderThreshold { get; set; }

      public int TargetStockLevel { get; set; }
   }
}