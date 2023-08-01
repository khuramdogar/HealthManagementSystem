namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public class PostOrderDTO
   {
      public string OrderName { get; set; }
      public int DeliveryLocationId { get; set; }
      public OrderableItemDTO[] OrderableItems { get; set; }
      public int? LedgerId { get; set; }
   }

   public class OrderableItemDTO : IOrderableItem
   {
      public int[] Ids { get; set; }
      public OrderableItemSource OrderableItemSource { get; set; }
      public int ProductId { get; set; }
      public int Quantity { get; set; }
   }
}