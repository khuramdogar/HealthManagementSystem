namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public class ProcessOrderDTO
   {
      public int OrderItemId { get; set; }
      public int Quantity { get; set; }
      public OrderItemAction Action { get; set; }
   }
}