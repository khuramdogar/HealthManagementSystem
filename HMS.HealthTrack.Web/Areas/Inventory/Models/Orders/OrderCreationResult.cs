using System.Collections.Generic;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Orders
{
   public class OrderCreationResult
   {
      public bool CreationSuccess { get; set; }
      public int OrderId { get; set; }
      public bool SubmittedToChannel { get; set; }
      public List<string> Errors { get; set; }
      public string AdditionalInfo { get; set; }
   }
}