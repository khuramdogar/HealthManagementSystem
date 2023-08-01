namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public interface IOrderableItem
   {
      int ProductId { get; set; }
      int Quantity { get; set; }
   }
}