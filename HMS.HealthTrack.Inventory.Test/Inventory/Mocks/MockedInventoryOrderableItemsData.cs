using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal sealed class MockedInventoryOrderableItemsData : MockedInventoryData
   {
      public Mock<IDbSet<StockAdjustment>> MockedAdjustments { get; set; }
      public Mock<IDbSet<DeductionRequiringPaymentClass>> MockedConsumptionRequiringPaymentClass { get; set; }
      public Mock<IDbSet<Product>> MockedProducts { get; set; }
      public Mock<IDbSet<OrderItemSource>> MockedOrderItemSources { get; set; }
      public Mock<IDbSet<Order>> MockedOrders { get; set; }
      public Mock<IDbSet<NegativeStock>> MockedNegativeStock { get; set; }
      public Mock<IDbSet<ProductStockRequest>> MockedStockRequests { get; set; }
      public Mock<IDbSet<ConsumptionNotificationManagement>> MockedConsumptionNotificationManagement { get; set; }
      public Mock<IDbSet<HealthTrackConsumption>> MockedHealthTrackConsumption { get; set; }

      public MockedInventoryOrderableItemsData(IEnumerable<StockAdjustment> consumptions,
         IEnumerable<DeductionRequiringPaymentClass> consumptionRequiringPaymentClass, IEnumerable<Product> products,
         IEnumerable<OrderItemSource> orderItemSources, IEnumerable<Order> orders,
         IEnumerable<NegativeStock> negativeStock, IEnumerable<ProductStockRequest> requests,
         IEnumerable<ConsumptionNotificationManagement> cnm, IEnumerable<HealthTrackConsumption> htc)
      {
         MockedAdjustments = MockingHelper.GetMockedDbSet(consumptions.AsQueryable());
         MockedConsumptionRequiringPaymentClass = MockingHelper.GetMockedDbSet(consumptionRequiringPaymentClass.AsQueryable());
         MockedProducts = MockingHelper.GetMockedDbSet(products.AsQueryable());
         MockedOrderItemSources = MockingHelper.GetMockedDbSet(orderItemSources.AsQueryable());
         MockedOrders = MockingHelper.GetMockedDbSet(orders.AsQueryable());
         MockedNegativeStock = MockingHelper.GetMockedDbSet(negativeStock.AsQueryable());
         MockedStockRequests = MockingHelper.GetMockedDbSet(requests.AsQueryable());
         MockedConsumptionNotificationManagement = MockingHelper.GetMockedDbSet(cnm.AsQueryable());
         MockedHealthTrackConsumption = MockingHelper.GetMockedDbSet(htc.AsQueryable());

         MockedContext.Setup(c => c.StockAdjustments).Returns(MockedAdjustments.Object);
         MockedContext.Setup(c => c.DeductionsRequiringPaymentClass)
            .Returns(MockedConsumptionRequiringPaymentClass.Object);
         MockedContext.Setup(c => c.Products).Returns(MockedProducts.Object);
         MockedContext.Setup(c => c.OrderItemSources).Returns(MockedOrderItemSources.Object);
         MockedContext.Setup(c => c.Orders).Returns(MockedOrders.Object);
         MockedContext.Setup(c => c.NegativeStocks).Returns(MockedNegativeStock.Object);
         MockedContext.Setup(c => c.ProductStockRequests).Returns(MockedStockRequests.Object);
         MockedContext.Setup(c => c.ConsumptionNotificationManagements).Returns(MockedConsumptionNotificationManagement.Object);
         MockedContext.Setup(c => c.HealthTrackConsumptions).Returns(MockedHealthTrackConsumption.Object);
      }

      public MockedInventoryOrderableItemsData()
         : this(
            new List<StockAdjustment>(), new List<DeductionRequiringPaymentClass>(), new List<Product>(),
            new List<OrderItemSource>(), new List<Order>(), new List<NegativeStock>(), new List<ProductStockRequest>(),
            new List<ConsumptionNotificationManagement>(), new List<HealthTrackConsumption>())
      {
      }
   }
}