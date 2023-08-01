using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration.Common;
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
   public class OrderSubmissionTests
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

      [TestMethod]
      public void NoSubmission()
      {
         //Arrange
         var orderSubmissionUnitOfWork = new Mock<IOrderSubmissionUnitOfWork>();
         var channelSubmitter = new Mock<IOrderChannelSubmitter>();
         var orderRepo = new Mock<IOrderRepository>();
         var channelrepo = new Mock<IOrderChannelRepository>();
         orderRepo.Setup(or => or.FindAll()).Returns(new List<Order>().AsQueryable());
         orderSubmissionUnitOfWork.Setup(u => u.OrderRepo).Returns(orderRepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.OrderChannelRepo).Returns(channelrepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.ConfigRepo).Returns(FileConfigRepo.Object);

         //SUT
         var processor = new OrderSubmissionProcessor(Logger.Object, PropProvider.Object, orderSubmissionUnitOfWork.Object);
         processor.SubmitOrders(new List<Order>().AsQueryable());
      }

      /// <summary>
      /// An empty order that should result in an errored submission
      /// </summary>
      [TestMethod]
      public void SubmitOrder_EmptyOrder()
      {
         //Test data set
         var order = new Order();
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
         var submission = processor.SubmitOrder(order);
         Assert.AreEqual(SubmissionStatus.Error,submission.SubmissionStatus,"Empty orders should be erroneous");
      }

      /// <summary>
      /// An empty order that should result in an errored submission
      /// </summary>
      [TestMethod]
      public void SubmitOrder_Order_WithFmisItem()
      {
         //Test data set
         var order = new Order
         {
            Items = new List<OrderItem>
            {
               new OrderItem {Product = new Product {OrderChannelProducts = new List<OrderChannelProduct> {new OrderChannelProduct { AutomaticOrder = true,OrderChannel = new OrderChannel { Name = FmisAttributes.FmisOrderChannelName} } }}}
            }
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
         var submission = processor.SubmitOrder(order);
         Assert.AreEqual(SubmissionStatus.Queued, submission.SubmissionStatus, "Submissions are queued to a new thread for submission");
      }
   }
}
