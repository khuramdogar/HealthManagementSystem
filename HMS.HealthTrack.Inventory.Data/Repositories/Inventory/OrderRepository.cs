using System;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Data.Repositories.Inventory
{
   public class OrderRepository : IOrderRepository
   {
      private readonly IDbContextInventoryContext _inventoryContext;

      public OrderRepository(IDbContextInventoryContext inventoryContext)
      {
         _inventoryContext = inventoryContext;
      }

      public void Add(Order order)
      {
         order.LastModifiedOn = DateTime.Now;
         _inventoryContext.Orders.Add(order);
      }

      public Order Find(int id)
      {
         return _inventoryContext.Orders.Where(o => o.InventoryOrderId == id)
            .Include(o => o.Items.Select(oi => oi.Product))
            .Include(o => o.DeliveryLocation)
            .Include(o => o.DeliveryLocation)
            .Include(o => o.Items.Select(i => i.OrderItemSources.Select(ois => ois.ProductStockRequest)))
            .Include(o => o.GeneralLedger)
            .SingleOrDefault();
      }

      public void Update(Order entity)
      {
         var existing = Find(entity.InventoryOrderId);
         if (existing == null) return;

         entity.LastModifiedOn = DateTime.Now;
         _inventoryContext.Entry(existing).CurrentValues.SetValues(entity);
         _inventoryContext.Entry(existing).Property(xx => xx.CreatedBy).IsModified = false;
         _inventoryContext.Entry(existing).Property(xx => xx.DateCreated).IsModified = false;
      }

      public Order FindAsync(int? id)
      {
         return _inventoryContext.Orders.Find(id);
      }

      public Order FindDetails(int? id)
      {
         var order = _inventoryContext.Orders
            .Include(io => io.DeliveryLocation.Address)
            .Include(io => io.Items.Select(ioi => ioi.Product).Select(ioi => ioi.Stocks))
            .Include(io => io.Items.Select(ioi => ioi.Product).Select(ioi => ioi.GeneralLedger))
            .Include(io => io.Items.Select(i => i.ConsumptionNotificationManagements))
            .Include(io => io.Items.Select(ioi => ioi.StockAdjustments))
            .SingleOrDefault(io => io.InventoryOrderId == id);
         return order;
      }

      public void Remove(Order order)
      {
         if (order.Status == OrderStatus.Created)
         {
            var items = _inventoryContext.OrderItems.Where(item => item.InventoryOrderId == order.InventoryOrderId);

            foreach (var orderItem in items) _inventoryContext.OrderItems.Remove(orderItem);

            _inventoryContext.Orders.Remove(order);
         }
         else
         {
            order.DeletedOn = DateTime.Now;
         }
      }

      public OrderItem InvoiceConsumption(int deliveryLocationId, string username, int productId, int consumptionId, string orderName, int? ledgerId, int quantity, decimal? unitPrice)
      {
         var name = string.Format("{0} - {1}", consumptionId, orderName);

         // create 'invoice' with hidden status
         var invoice = new Order
         {
            CreatedBy = username,
            DateCreated = DateTime.Now,
            DeliveryLocationId = deliveryLocationId,
            Name = name,
            LastModifiedBy = username,
            LastModifiedOn = DateTime.Now,
            LedgerId = ledgerId,
            Status = OrderStatus.Invoiced
         };


         var invoiceItem = new OrderItem
         {
            ProductId = productId,
            Quantity = quantity,
            Status = OrderItemStatus.Invoiced,
            UnitPrice = unitPrice
         };

         invoiceItem.Order = invoice;

         return invoiceItem;
      }

      public void Commit()
      {
         _inventoryContext.ObjectContext.SaveChanges();
      }

      public void MarkAsPartiallyReceived(Order order, string username)
      {
         order.LastModifiedOn = DateTime.Now;
         order.LastModifiedBy = username;
         order.Status = OrderStatus.PartiallyReceived;
      }

      public Order FindExistingOrder(int productId, int supplierId)
      {
         return
            _inventoryContext.Orders.Include(order => order.Items)
               .FirstOrDefault(order => order.Status == OrderStatus.Ordered
                                        &&
                                        order.Items.Any(
                                           item =>
                                              item.Product.PrimarySupplier.HasValue &&
                                              item.Product.PrimarySupplier.Value == supplierId));
      }

      public IQueryable<Order> FindAll()
      {
         return _inventoryContext.Orders.Include(o => o.Items).Where(o => o.DeletedOn == null);
      }

      public void Complete(Order order)
      {
         order.LastModifiedOn = DateTime.Now;
         order.Status = OrderStatus.Complete;
      }

      public void Cancel(Order order, string username)
      {
         order.LastModifiedOn = DateTime.Now;
         order.LastModifiedBy = username;
         order.Status = OrderStatus.Cancelled;

         foreach (var orderItem in order.Items) orderItem.Status = OrderItemStatus.Cancelled;
      }

      public Order FindByItem(int orderItemId)
      {
         var orderItem = _inventoryContext.OrderItems.SingleOrDefault(oi => oi.OrderItemId == orderItemId);
         return orderItem != null ? orderItem.Order : null;
      }

      public IQueryable<Order> OrdersForProduct(int productId)
      {
         var orders = FindAll().Include(o => o.DeliveryLocation).Include(o => o.Items)
            .Where(
               o =>
                  o.Items.Any(oi => oi.ProductId == productId));
         return orders;
      }

      public bool OrderNeedsSubmission(int orderId)
      {
         //Check if any items in the order have a channel for submitting
         var result = from i in _inventoryContext.OrderItems
            where i.InventoryOrderId == orderId
                  && i.Product.OrderChannelProducts.Any(ocp => ocp.AutomaticOrder)
            select i;

         return result.Any();
      }

      public static void UpdateOrderStatus(Order order, string username)
      {
         if (order.Items.All(oi => oi.Status == OrderItemStatus.Cancelled))
            order.Status = OrderStatus.Cancelled;
         else if (order.Items.All(oi => oi.Status == OrderItemStatus.Invoiced))
            order.Status = OrderStatus.Invoiced;
         else if (order.Items.Any(oi => oi.Status == OrderItemStatus.Invoiced))
            throw new OrderException.InvalidOrderStateException(order);
         else if (order.Items.All(oi => oi.Status == OrderItemStatus.Reversed))
            order.Status = OrderStatus.Reversed;
         else if (order.Items.Any(oi => oi.Status == OrderItemStatus.Reversed))
            throw new OrderException.InvalidOrderStateException(order);
         else if (order.Items.All(xx => xx.Status == OrderItemStatus.Received || xx.Status == OrderItemStatus.Cancelled || xx.Status == OrderItemStatus.Complete))
            order.Status = OrderStatus.Complete;
         else if (
            order.Items.Any(
               xx => xx.Status == OrderItemStatus.Received || xx.Status == OrderItemStatus.PartiallyReceived))
            order.Status = OrderStatus.PartiallyReceived;
         else if (order.Items.All(xx => xx.Status == OrderItemStatus.Ordered)) order.Status = OrderStatus.Ordered;

         order.LastModifiedOn = DateTime.Now;
         order.LastModifiedBy = username;
      }
   }

   public interface IOrderRepository
   {
      void Commit();
      Order Find(int id);
      void Update(Order entity);
      Order FindAsync(int? id);
      Order FindDetails(int? id);
      void MarkAsPartiallyReceived(Order order, string username);
      Order FindExistingOrder(int productId, int supplierId);
      IQueryable<Order> FindAll();
      void Complete(Order order);
      void Cancel(Order order, string username);
      Order FindByItem(int orderItemId);
      void Add(Order order);
      void Remove(Order inventoryOrder);
      OrderItem InvoiceConsumption(int deliveryLocationId, string username, int productId, int consumptionId, string orderName, int? ledgerId, int quantity, decimal? unitPrice);
      IQueryable<Order> OrdersForProduct(int productId);
      bool OrderNeedsSubmission(int value);
   }
}