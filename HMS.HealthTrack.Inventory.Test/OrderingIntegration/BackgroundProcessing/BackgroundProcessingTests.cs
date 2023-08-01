using System.Collections.Generic;
using System.Configuration;
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
   public class BackgroundProcessingTests
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
      [ExpectedException(typeof(ConfigurationErrorsException))]
      public void EmptyProcessor_NoOutputMethodConfigured()
      {
         var orderSubmissionUnitOfWork = new Mock<IOrderSubmissionUnitOfWork>();
         IMock<IOrderChannelRepository> channelRepo = new Mock<IOrderChannelRepository>();
         orderSubmissionUnitOfWork.Setup(u => u.ConfigRepo).Returns(new Mock<IConfigurationRepository>().Object);//empty config (no "file")
         orderSubmissionUnitOfWork.Setup(u => u.OrderChannelRepo).Returns(channelRepo.Object);

         var orderSubmissionProcessor = new OrderSubmissionProcessor(Logger.Object, PropProvider.Object, orderSubmissionUnitOfWork.Object);
      }

      [TestMethod]
      public void EmptyFileBasedProcessor_NoOutputMethodConfigured()
      {
         var orderSubmissionUnitOfWork = new Mock<IOrderSubmissionUnitOfWork>();
         IMock<IOrderChannelRepository> channelRepo = new Mock<IOrderChannelRepository>();
         orderSubmissionUnitOfWork.Setup(u => u.ConfigRepo).Returns(FileConfigRepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.OrderChannelRepo).Returns(channelRepo.Object);

         var orderSubmissionProcessor = new OrderSubmissionProcessor(Logger.Object, PropProvider.Object, orderSubmissionUnitOfWork.Object);
      }

      [TestMethod]
      public void Process_NoOrders()
      {
         //Arrange
         var orderSubmissionUnitOfWork = new Mock<IOrderSubmissionUnitOfWork>();
         IMock<IOrderChannelRepository> channelRepo = new Mock<IOrderChannelRepository>();
         var orderRepo = new Mock<IOrderRepository>();
         orderRepo.Setup(or => or.FindAll()).Returns(new List<Order>().AsQueryable());
         orderSubmissionUnitOfWork.Setup(u => u.OrderRepo).Returns(orderRepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.OrderChannelRepo).Returns(channelRepo.Object);
         orderSubmissionUnitOfWork.Setup(u => u.ConfigRepo).Returns(FileConfigRepo.Object);
         

         //SUT
         var processor = new OrderSubmissionProcessor(Logger.Object, PropProvider.Object, orderSubmissionUnitOfWork.Object);
         processor.ProcessOrders();
      }
   }
}
