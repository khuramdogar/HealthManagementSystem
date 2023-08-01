using System;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Exceptions
{
   public class StockException : Exception
   {
      public StockException(string message, params object[] args)
         : base(string.Format(message, args))
      {
      }
   }

   public class ConsumptionProcessingException : Exception
   {
      public ConsumptionProcessingException(string message, params object[] args)
         : base(string.Format(message, args))
      {
      }
   }

   public class MaxStockUsesException : StockException
   {
      public MaxStockUsesException(Stock item)
         : base(
            "Failed to recycle stock item '{0}' of type '{1}' as it has reached it's max uses of '{2}'", item.ProductId,
            item.SerialNumber, item.Product.MaxUses)
      {
      }
   }

   public class InvalidStockStateException : StockException
   {
      public InvalidStockStateException(Stock item, StockStatus expectedItemStatus)
         : base(
            "Failed to update stock '{0}' (Product {1}) due to it being in an invalid state. Expected state: '{2}' Current state:{3}",
            item.StockId, item.AuditDescription, expectedItemStatus, item.StockStatus)
      {
      }
   }

   public class InsufficentStockException : StockException
   {
      public InsufficentStockException(ItemAdjustment item)
         : base(
            "Failed to deduct stock {0} x '{1}' as there are insufficient items in stock to satisfy this deduction",
            item.Quantity, item.ProductId)
      {
      }
   }

   public class InvalidStockAdjustmentLocationException : StockException
   {
      public InvalidStockAdjustmentLocationException(ItemAdjustment item)
         : base(
            "Failed to deduct {0} stock for product {1} at an invalid location {2}", item.Quantity, item.ProductId,
            item.StockLocationId)
      {
      }
   }
}