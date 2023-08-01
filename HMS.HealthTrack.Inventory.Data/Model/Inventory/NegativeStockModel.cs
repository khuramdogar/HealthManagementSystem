namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public class NegativeStockModel
   {
      public int ProductId { get; set; }
      public int StoredAt { get; set; }
      public int NegativeQuantity { get; set; }
   }
}