using System;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Orders
{
   public class OrderableItemConsumptions
   {
      public string PatientId { get; set; }
      public string FirstName { get; set; }
      public string Surname { get; set; }
      public DateTime? Date { get; set; }
      public string Doctor { get; set; }
      public string RebateCode { get; set; }
   }
}