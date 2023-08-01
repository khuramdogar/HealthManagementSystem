using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public class ProductForExport
   {
      public ProductsWithConsumptionForExport ProductWithConsumption { get; set; }
      public IEnumerable<ProductPrice> Prices { get; set; }
      public IEnumerable<Category> Categories { get; set; }
      public Product Product { get; set; }
      public bool HadStockTake { get; set; }
   }
}