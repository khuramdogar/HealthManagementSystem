using System.Collections;
using System.Collections.Generic;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Areas.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Web.IntegrationTesting.OrderingIntegration
{
   [TestClass]
   public class BackgroundProcessingTests
   {
      public IMock<ICustomLogger> Logger = new Mock<ICustomLogger>();
      public IMock<IPropertyProvider> PropProvider = new Mock<IPropertyProvider>();

      [TestMethod,Ignore]
      public void EmptyProcessor()
      {
         var orderSubmissionUnitOfWork = new Mock<IOrderSubmissionUnitOfWork>();
         var channelProcessors = new List<IOrderChannelSubmitter> {new Mock<IOrderChannelSubmitter>().Object};
         var processor = new OrderSubmissionProcessor(Logger.Object, PropProvider.Object, orderSubmissionUnitOfWork.Object);
         processor.ProcessOrders();
      }
   }
}