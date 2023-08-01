using System;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class OrderItemRepository : IOrderItemRepository
   {
      private readonly IDbContextInventoryContext _context;
      private readonly IPropertyProvider _propertyProvider;

      public OrderItemRepository(IDbContextInventoryContext context, IPropertyProvider propertyProvider)
      {
         _context = context;
         _propertyProvider = propertyProvider;
      }

      public OrderItem Find(int orderItemId)
      {
         return
            _context.OrderItems.Include(xx => xx.Order)
               .Include(xx => xx.Product)
               .Include(xx => xx.ConsumptionNotificationManagements)
               .SingleOrDefault(xx => xx.OrderItemId == orderItemId);
      }

      public void Cancel(int orderItemId)
      {
         var item = _context.OrderItems.SingleOrDefault(xx => xx.OrderItemId == orderItemId);
         if (item == null) return;

         item.Status = OrderItemStatus.Cancelled;
      }

      public void Cancel(OrderItem orderItem)
      {
         if (orderItem == null)
            return;
         orderItem.Status = OrderItemStatus.Cancelled;
      }

      public void ReceiveItem(OrderItem item)
      {
         if (item.Product.ManageStock)
         {
            item.ReceivedOn = DateTime.Now;
            item.Status = OrderItemStatus.Received;
         }
         else
         {
            item.Status = OrderItemStatus.Complete;
         }
      }

      public void PartiallyReceive(OrderItem orderItem)
      {
         orderItem.Status = OrderItemStatus.PartiallyReceived;
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public IQueryable<OrderItem> FindItemsForOrder(int orderId)
      {
         var orderItems =
            _context.OrderItems.Include(oi => oi.Product.Stocks)
               .Include(oi => oi.Product.PrimarySupplierCompany)
               .Include(oi => oi.OrderItemSources)
               .Include(oi => oi.ConsumptionNotificationManagements)
               .Include(oi => oi.StockAdjustments)
               .Where(oi => oi.InventoryOrderId == orderId);
         return orderItems;
      }
   }

   public interface IOrderItemRepository
   {
      void Cancel(int orderItemId);
      void ReceiveItem(OrderItem item);
      void PartiallyReceive(OrderItem orderItem);
      OrderItem Find(int orderItemId);
      void Commit();
      IQueryable<OrderItem> FindItemsForOrder(int orderId);
      void Cancel(OrderItem orderItem);
   }
}