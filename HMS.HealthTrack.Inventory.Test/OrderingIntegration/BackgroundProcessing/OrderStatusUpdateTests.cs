using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Areas.Inventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.BackgroundProcessing
{
   [TestClass]
   public class OrderStatusUpdateTests
   {
      public IMock<ICustomLogger> Logger = new Mock<ICustomLogger>();
      public IMock<IPropertyProvider> PropProvider = new Mock<IPropertyProvider>();
      public IMock<IConfigurationRepository> FileConfigRepo
      {
         get
         {
            var fileConfig = new Mock<IConfigurationRepository>();
            fileConfig.Setup(c => c.GetConfigurationValue<string>(It.IsAny<string>())).Returns("file");
            return fileConfig;
         }
      }

      /// <summary>
      /// Orders with not items are not applicable for submission
      /// </summary>
      [TestMethod]
      public void PrepareUnknownOrders_Order_NoItems()
      {
         //Data set
         var order = new Order
         {
            OrderSubmissionStatus = OrderSubmissionStatus.Unknown
         };

         //Arrange
         var orderSubmissionUnitOfWork = new Mock<IOrderSubmissionUnitOfWork>();
         var channelSubmitter = new Mock<IOrderChannelSubmitter>();
         var orderRepo = new Mock<IOrderRepository>();
         var channelrepo = new Mock<IOrderChannelRepository>();

         orderRepo.Setup(or => or.FindAll()).Returns(new List<Order> { order }.AsQueryable());
         orderSubmissionUnitOfWork.Setup(u => u.OrderRepo).Returns(orderRepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.OrderChannelRepo).Returns(channelrepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.ConfigRepo).Returns(FileConfigRepo.Object);

         //SUT
         var processor = new OrderSubmissionProcessor(Logger.Object, PropProvider.Object, orderSubmissionUnitOfWork.Object);
         processor.PrepareUnknownOrders(new List<Order> {order}.AsQueryable());

         Assert.AreEqual(OrderSubmissionStatus.NotApplicable, order.OrderSubmissionStatus, "Orders with not items are not applicable for submission");
      }

      /// <summary>
      /// Orders with not items are not applicable for submission
      /// </summary>
      [TestMethod]
      public void PrepareUnknownOrders_Order_ItemWithoutChannel()
      {
         //Data set
         var order = new Order
         {
            OrderSubmissionStatus = OrderSubmissionStatus.Unknown,
            Items = new List<OrderItem> {new OrderItem {Product = new Product()}}
         };

         //Arrange
         var orderSubmissionUnitOfWork = new Mock<IOrderSubmissionUnitOfWork>();
         var channelSubmitter = new Mock<IOrderChannelSubmitter>();
         var orderRepo = new Mock<IOrderRepository>();
         var channelrepo = new Mock<IOrderChannelRepository>();

         orderRepo.Setup(or => or.FindAll()).Returns(new List<Order> { order }.AsQueryable());
         orderSubmissionUnitOfWork.Setup(u => u.OrderRepo).Returns(orderRepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.OrderChannelRepo).Returns(channelrepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.ConfigRepo).Returns(FileConfigRepo.Object);

         //SUT
         var processor = new OrderSubmissionProcessor(Logger.Object, PropProvider.Object, orderSubmissionUnitOfWork.Object);
         processor.PrepareUnknownOrders(new List<Order> {order}.AsQueryable());

         Assert.AreEqual(OrderSubmissionStatus.NotApplicable, order.OrderSubmissionStatus, "Orders with not items are not applicable for submission");
      }

      [TestMethod]
      public void PrepareUnknownOrders_Order_ItemWithChannel_NotAutoOrder()
      {
         //Data set
         var order = new Order
         {
            OrderSubmissionStatus = OrderSubmissionStatus.Unknown,
            Items = new List<OrderItem> {new OrderItem {Product = new Product {OrderChannelProducts = new List<OrderChannelProduct> {new OrderChannelProduct {AutomaticOrder = false}}}}}
         };

         //Arrange
         var orderSubmissionUnitOfWork = new Mock<IOrderSubmissionUnitOfWork>();
         var channelSubmitter = new Mock<IOrderChannelSubmitter>();
         var orderRepo = new Mock<IOrderRepository>();
         var channelrepo = new Mock<IOrderChannelRepository>();

         orderRepo.Setup(or => or.FindAll()).Returns(new List<Order> { order }.AsQueryable());
         orderSubmissionUnitOfWork.Setup(u => u.OrderRepo).Returns(orderRepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.OrderChannelRepo).Returns(channelrepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.ConfigRepo).Returns(FileConfigRepo.Object);

         //SUT
         var processor = new OrderSubmissionProcessor(Logger.Object, PropProvider.Object, orderSubmissionUnitOfWork.Object);
         processor.PrepareUnknownOrders(new List<Order> {order}.AsQueryable());

         Assert.AreEqual(OrderSubmissionStatus.NotApplicable, order.OrderSubmissionStatus, "Orders with not items are not applicable for submission");
      }

      [TestMethod]
      public void PrepareUnknownOrders_Order_ItemWithChannel_IsAutoOrder()
      {
         //Data set
         var order = new Order
         {
            OrderSubmissionStatus = OrderSubmissionStatus.Unknown,
            Items = new List<OrderItem> {new OrderItem {Product = new Product {OrderChannelProducts = new List<OrderChannelProduct> {new OrderChannelProduct {AutomaticOrder = true}}}}}
         };

         //Arrange
         var orderSubmissionUnitOfWork = new Mock<IOrderSubmissionUnitOfWork>();
         var channelSubmitter = new Mock<IOrderChannelSubmitter>();
         var orderRepo = new Mock<IOrderRepository>();
         var channelrepo = new Mock<IOrderChannelRepository>();
         
         orderRepo.Setup(or => or.FindAll()).Returns(new List<Order> {order}.AsQueryable());
         orderSubmissionUnitOfWork.Setup(u => u.OrderRepo).Returns(orderRepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.OrderChannelRepo).Returns(channelrepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.ConfigRepo).Returns(FileConfigRepo.Object);

         //SUT
         var processor = new OrderSubmissionProcessor(Logger.Object, PropProvider.Object, orderSubmissionUnitOfWork.Object);
         processor.PrepareUnknownOrders(new List<Order> {order}.AsQueryable());

         Assert.AreEqual(OrderSubmissionStatus.AwaitingSubmission, order.OrderSubmissionStatus, "Orders with not items are not applicable for submission");
      }
   }
}