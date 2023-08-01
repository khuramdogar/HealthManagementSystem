using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.Inventory.ReceiveOrders
{
   [TestClass]
   public class OrderStatusTests
   {
      public const int DeliveryLocationId = 6;
      private const string Testname = "OrderStatusTests";

      private OrderItem TestOrderItem
      {
         get
         {
            return new OrderItem
            {
               OrderItemId = 1,
               //OrderItemSources = TestOrderItemSources.ToList(),
               //Product = TestProduct,
               ProductId = 123,
               Quantity = 500,
               Status = OrderItemStatus.Ordered
            };
         }
      }

      private OrderItem TestOrderItem_v2
      {
         get
         {
            return new OrderItem
            {
               OrderItemId = 2,
               //OrderItemSources = TestOrderItemSources.ToList(),
               //Product = TestProduct,
               ProductId = 122,
               Quantity = 502,
               Status = OrderItemStatus.Ordered
            };
         }
      }

      private Order TestOrder
      {
         get
         {
            return new Order
            {
               Items = new List<OrderItem>
               {
                  TestOrderItem,
               },
               Status = OrderStatus.Ordered
            };
         }
      }

      // Order Item Statuses:
      //    - Cancelled
      //    - Complete
      //    - Invoiced
      //    - Ordered
      //    - PartiallyReceived
      //    - Received
      //    - Reversed

      #region AllOrderItemsOneStatus

      [TestMethod]
      public void UpdateOrderStatus_AllItemsCancelled_Cancel()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Cancelled;

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.Cancelled, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_AllItemsComplete_Complete()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Complete;

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.Complete, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_AllItemsInvoiced_Invoiced()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Invoiced;

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.Invoiced, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_AllItemsOrdered_Ordered()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Ordered;

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.Ordered, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_AllItemsReceived_Complete()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Received;

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.Complete, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_AllItemsReversed_Reversed()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Reversed;

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.Reversed, order.Status);
      }

      #endregion

      [TestMethod]
      public void UpdateOrderStatus_AnyItemPartiallyReceived_PartiallyReceived()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.PartiallyReceived;
         var item2 = TestOrderItem_v2;
         item2.Status = OrderItemStatus.Cancelled;
         order.Items.Add(item2);
         var item3 = TestOrderItem_v2;
         item3.Status = OrderItemStatus.Complete;
         order.Items.Add(item3);
         var item4 = TestOrderItem_v2;
         item4.Status = OrderItemStatus.Received;
         order.Items.Add(item4);

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.PartiallyReceived, order.Status);
      }

      /// <summary>
      /// Not a valid state for an order. 
      /// Invoice items should be in an order by themselves. 
      /// </summary>
      [TestMethod]
      [ExpectedException(typeof(OrderException.InvalidOrderStateException))]
      public void UpdateOrderStatus_InvoicedItem_NonInvoicedItem_Exception()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Invoiced;
         order.Items.Add(TestOrderItem_v2); // ordered

         OrderRepository.UpdateOrderStatus(order, Testname);
      }

      /// <summary>
      /// Not a valid state for an order. 
      /// Means that somehow reverse order has failed or something has touched a reversed order.
      /// </summary>
      [TestMethod]
      [ExpectedException(typeof(OrderException.InvalidOrderStateException))]
      public void UpdateOrderStatus_ReversedItem_NonReversedItem_Exception()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Reversed;
         order.Items.Add(TestOrderItem_v2); // ordered

         OrderRepository.UpdateOrderStatus(order, Testname);
      }

      [TestMethod]
      public void UpdateOrderStatus_CancelledAndOrdered_Ordered()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Cancelled;
         order.Items.Add(TestOrderItem_v2); // ordered

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.Ordered, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_CompleteAndOrdered_Ordered()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Complete;
         order.Items.Add(TestOrderItem_v2); // ordered

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.Ordered, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_ReceivedAndOrdered_PartiallyReceived()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Received;
         order.Items.Add(TestOrderItem_v2); // ordered

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.PartiallyReceived, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_PartiallyReceivedAndOrdered_PartiallyReceived()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.PartiallyReceived;
         order.Items.Add(TestOrderItem_v2); // ordered

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.PartiallyReceived, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_CompleteAndCancelled_Complete()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Complete;

         var item2 = TestOrderItem_v2;
         item2.Status = OrderItemStatus.Cancelled;
         order.Items.Add(item2); // ordered

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.Complete, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_ReceivedAndCancelled_Complete()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.Received;

         var item2 = TestOrderItem_v2;
         item2.Status = OrderItemStatus.Cancelled;
         order.Items.Add(item2); // ordered

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.Complete, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_PartiallyReceivedAndCancelled_PartiallyReceived()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.PartiallyReceived;

         var item2 = TestOrderItem_v2;
         item2.Status = OrderItemStatus.Cancelled;
         order.Items.Add(item2); // ordered

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.PartiallyReceived, order.Status);
      }

      [TestMethod]
      public void UpdateOrderStatus_PartiallyReceivedAndComplete_PartiallyReceived()
      {
         var order = TestOrder;
         order.Items.First().Status = OrderItemStatus.PartiallyReceived;

         var item2 = TestOrderItem_v2;
         item2.Status = OrderItemStatus.Complete;
         order.Items.Add(item2);

         OrderRepository.UpdateOrderStatus(order, Testname);

         Assert.AreEqual(OrderStatus.PartiallyReceived, order.Status);
      }
   }
}
