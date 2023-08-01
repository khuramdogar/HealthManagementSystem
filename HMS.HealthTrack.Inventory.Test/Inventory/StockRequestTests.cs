using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.Inventory
{
   [TestClass]
   public class StockRequestTests
   {
      [TestMethod]
      public void CloseStockRequests()
      {
         var firstRequest = new ProductStockRequest
         {
            StockRequestId = 1,
            ProductId = 123,
            RequestedQuantity = 2,
            RequestStatus = RequestStatus.Ordered
         };

         var secondRequest = new ProductStockRequest
         {
            StockRequestId = 2,
            ProductId = 123,
            RequestedQuantity = 1,
            RequestStatus = RequestStatus.Ordered
         };

         var orderItem = new OrderItem
           {
              OrderItemId = 1,
              ProductId = 123,
              Quantity = 4,
           };

         var firstSource = new OrderItemSource()
         {
            OrderItem = orderItem,
            OrderItemId = 1,
            ProductStockRequest = firstRequest,
            StockRequestId = 1,
            Quantity = 2
         };

         var secondSource = new OrderItemSource()
         {
            OrderItem = orderItem,
            OrderItemId = 1,
            ProductStockRequest = secondRequest,
            StockRequestId = 2,
            Quantity = 1
         };

         orderItem.OrderItemSources.Add(firstSource);
         orderItem.OrderItemSources.Add(secondSource);

         var mockedRequests = MockingHelper.GetMockedDbSet(new List<ProductStockRequest> { firstRequest, secondRequest }.AsQueryable());
         var mockedConsumptions = MockingHelper.GetMockedDbSet(new List<StockAdjustment>().AsQueryable());
         var mockedOrderItems = MockingHelper.GetMockedDbSet(new List<OrderItem> { orderItem }.AsQueryable());
         var mockedOrderItemSources = MockingHelper.GetMockedDbSet(new List<OrderItemSource>().AsQueryable());

         var mockedContext = MockingHelper.GetMockedContext<ProductStockRequest, InventoryContext>(mockedRequests, context => context.ProductStockRequests);
         mockedContext.Setup(c => c.OrderItems).Returns(mockedOrderItems.Object);
         mockedContext.Setup(c => c.OrderItemSources).Returns(mockedOrderItemSources.Object);
         mockedContext.Setup(c => c.StockAdjustments).Returns(mockedConsumptions.Object);

         //SUT 
         var stockRequestRepo = new StockRequestRepository(mockedContext.Object);
         stockRequestRepo.CloseRequests(orderItem, "Unit Test", true);

         Assert.AreEqual(RequestStatus.Closed, firstRequest.RequestStatus);
         Assert.AreEqual(RequestStatus.Closed, secondRequest.RequestStatus);
      }
   }
}
