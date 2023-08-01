namespace HMS.HealthTrack.Web.Data.Model.HealthTrackInventory
{
   public class HealthTrackConsumptionUpdate
   {
      public int UsedId { get; set; }
      public int ProductId { get; set; }
      public string SerialNumber { get; set; }
      public short? Quantity { get; set; }
      public int? Location { get; set; }
      public int? PatientId { get; set; }
      public string LotNumber { get; set; }
   }
}