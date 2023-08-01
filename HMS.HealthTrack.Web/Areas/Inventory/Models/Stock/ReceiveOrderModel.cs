using System.Collections.Generic;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Stock
{
   public class ReceiveOrderModel
   {
      public int OrderId { get; set; }
      public int LocationId { get; set; }

      public List<ProcessOrderDTO> OrderItems { get; set; }
   }
}