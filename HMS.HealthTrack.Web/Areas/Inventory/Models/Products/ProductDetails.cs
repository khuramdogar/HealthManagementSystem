namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class ProductDetails
   {
      public int ProductId { get; set; }
      public string LPC { get; set; }
      public string UPN { get; set; }
      public string Description { get; set; }

      public string RebateCode { get; set; }
      public decimal UnitPrice { get; set; }
      public string SPC { get; set; }
      public string Manufacturer { get; set; }
      public string Group { get; set; }
      public string SubGroup { get; set; }
      public string GLC { get; set; }
      public string PrimarySupplier { get; set; }
   }
}