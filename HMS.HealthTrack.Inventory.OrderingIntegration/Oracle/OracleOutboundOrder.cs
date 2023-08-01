using System.Collections.Generic;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
   internal class OracleOutboundOrder
   {
      public IEnumerable<OracleOutboundOrderItem> OrderItems { get; set; }

      public IList<string> ToPipeSeperatedLines()
      {
         var lines = new List<string>();

         foreach (var orderItem in OrderItems)
         {
            lines.AddRange(orderItem.ToPipeSeperatedLines());
         }

         return lines;
      }
   }
}