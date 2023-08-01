using NUnit.Framework;
using System;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.OrderingIntegration;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.IntegrationTesting.OrderingIntegration
{
   [TestFixture]
   public class OrderStatusManagementServiceTests
   {
      [Test, Explicit]
      public void SubmitOrder_OrderStatusCreated_IsSuccessful()
      {
         const int orderId = 1;

         InitOrderStatus(orderId, OrderStatus.Created);

         var success = _updateOrderToSubmitted(orderId);

         Assert.IsTrue(success);
      }

      [Test, Explicit]
      public void SubmitOrder_OrderStatusApproved_ThrowsException()
      {
         const int orderId = 1;

         InitOrderStatus(orderId, OrderStatus.Approved);

         var success = _updateOrderToSubmitted(orderId);

         Assert.IsFalse(success);
      }

      [Test, Explicit]
      public void SubmitOrder_MultipleConcurrentSubmissions_OnlyOneSucceeds()
      {
         const int orderId = 1;

         InitOrderStatus(orderId, OrderStatus.Created);

         var tasks = new Task<bool>[50];

         for (var i = 0; i < 50; i++)
         {
            tasks[i] = new Task<bool>(() => _updateOrderToSubmitted(orderId));
         }

         Parallel.ForEach(tasks, task => task.Start());

         Task.WaitAll(tasks);

         var results = tasks.Select(t => t.Result).ToList();

         Assert.AreEqual(1, results.Count(r => r));
         Assert.AreEqual(49, results.Count(r => !r));
      }

      private static ConnectionStringSettings GetTestConnectionStringSettings()
      {
         var ecb = new EntityConnectionStringBuilder
         {
            Metadata = "res://*/Model.Inventory.Inventory.csdl|res://*/Model.Inventory.Inventory.ssdl|res://*/Model.Inventory.Inventory.msl",
            Provider = "System.Data.SqlClient",
            ProviderConnectionString = "data source=localhost;initial catalog=HMS_Net_v2_Web;integrated security=True;multipleactiveresultsets=True;application name=EntityFramework",
         };

         return new ConnectionStringSettings(
             "InventoryContext",
             ecb.ConnectionString);
      }

      private static void InitOrderStatus(int orderId, OrderStatus status)
      {
         using (var context = new InventoryContext(GetTestConnectionStringSettings()))
         {
            var repo = new OrderRepository(context);

            repo.Update(new Order { InventoryOrderId = orderId, Status = status });

            repo.Commit();
         }
      }

      private readonly Func<int, bool> _updateOrderToSubmitted = id =>
      {
         var innerContext = new InventoryContext(GetTestConnectionStringSettings());
         var innerRepo = new OrderRepository(innerContext);

         var orderingSubmissionService = new OrderStatusManagementService(innerRepo);

         try
         {
            orderingSubmissionService.UpdateOrderToRequested(id);
            return true;
         }
         catch (Exception ex)
         {
            Debug.WriteLine(ex.Message);
            return false;
         }
      };
   }
}
