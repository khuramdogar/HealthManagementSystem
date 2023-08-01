using System;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Exceptions
{
   public class OrderException : Exception
   {
      public OrderException(string message, params object[] args)
         : base(string.Format(message, args))
      {
      }

      public class InvalidOrderStateException : OrderException
      {
         public InvalidOrderStateException(Order order)
            : base("The Order with ID '{0}' has fallen into an invalid state and cannot have it's status updated. It's current status is {1}", order.InventoryOrderId, order.Status)
         {
         }
      }
   }
}