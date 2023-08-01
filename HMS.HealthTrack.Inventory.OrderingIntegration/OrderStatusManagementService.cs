using System;
using System.Transactions;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Inventory.OrderingIntegration
{
   internal class OrderStatusManagementService : IOrderStatusManagementService
   {
      private readonly IOrderRepository _orderRepo;

      public OrderStatusManagementService(IOrderRepository orderRepo)
      {
         _orderRepo = orderRepo;
      }

      public void UpdateOrderToCreated(int orderId)
      {
         var order = _orderRepo.Find(orderId);

         UpdateStatus(order, OrderStatus.Created);
      }

      public void UpdateOrderToRequested(int orderId)
      {
         using (var scope = new TransactionScope())
         {
            try
            {
               var order = _orderRepo.Find(orderId);

               if (order.OrderSubmissionStatus == OrderSubmissionStatus.Success)
                  throw new Exception(
                      string.Format("exception=Order has already been submitted, orderId={0}, status={1}",
                          order.InventoryOrderId, order.Status));

               UpdateStatus(order, OrderStatus.PendingApproval);
            }
            finally
            {
               scope.Complete();
            }
         }
      }

      public void UpdateOrderToOrdered(int orderId)
      {
         var order = _orderRepo.Find(orderId);

         UpdateStatus(order, OrderStatus.Ordered);
      }

      public void UpdateOrderToFailed(int orderId)
      {
         var order = _orderRepo.Find(orderId);

         UpdateStatus(order, OrderStatus.Failed);
      }

      private void UpdateStatus(Order order, OrderStatus newStatus)
      {
         order.Status = newStatus;

         _orderRepo.Update(order);

         _orderRepo.Commit();
      }
   }
}