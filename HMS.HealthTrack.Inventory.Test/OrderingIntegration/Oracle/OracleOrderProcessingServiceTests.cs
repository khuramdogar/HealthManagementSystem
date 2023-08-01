using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Oracle
{
   [TestClass]
   public class OracleOrderProcessingServiceTests
   {
      private readonly OracleOutboundOrderItem _orderItem = new OracleOutboundOrderItem(1, 1, 'H', 1, "", new OracleOutboundOrderItemIdentifier("1", "1"), 1, "", "", "", "", "", 'N', 1, "", null);
      private OracleOutboundOrder _order;
      private readonly Mock<ICustomLogger> _logger = new Mock<ICustomLogger>();

      [TestInitialize]
      public void Init()
      {
         _order = new OracleOutboundOrder() {OrderItems = new List<OracleOutboundOrderItem>() {_orderItem}};
      }

      [TestMethod]
      public void SubmitOrder_ValidOrder_Succeeds()
      {
         var orderRepo = new Mock<IOrderRepository>();
         orderRepo.Setup(f => f.Find(1)).Returns(new Order());

         var statusManagementService = new Mock<IOrderStatusManagementService>();

         var converter = new Mock<IHealthTrackToOracleOrderConverter>();
         converter.Setup(f => f.ConvertOrder(It.IsAny<Order>())).Returns(_order);
         
         var fileService = new Mock<IOracleOutgoingFileService>();
         fileService.Setup(f => f.Send(It.IsAny<OracleOutboundOrder>())).Returns(Task.Run(() => { }));
         
         var orderProcessingService = new OracleOrderProcessingService(orderRepo.Object, 
            statusManagementService.Object, 
            converter.Object, 
            fileService.Object, 
            _logger.Object);

         var t = orderProcessingService.SubmitOrder(1);
         t.Wait();

         Assert.AreEqual(TaskStatus.RanToCompletion, t.Status);
         fileService.Verify(f => f.Send(It.IsAny<OracleOutboundOrder>()), Times.Once);
      }

      [TestMethod]
      [ExpectedException(typeof(AggregateException))]
      public void SubmitOrder_StatusUpdateFails_ThrowsException()
      {
         var orderRepo = new Mock<IOrderRepository>();
         orderRepo.Setup(f => f.Find(1)).Returns(new Order());

         var statusManagementService = new Mock<IOrderStatusManagementService>();
         statusManagementService.Setup(f => f.UpdateOrderToRequested(1)).Throws(new Exception());

         var converter = new Mock<IHealthTrackToOracleOrderConverter>();
         converter.Setup(f => f.ConvertOrder(It.IsAny<Order>())).Returns(_order);

         var fileService = new Mock<IOracleOutgoingFileService>();
         fileService.Setup(f => f.Send(It.IsAny<OracleOutboundOrder>())).Returns(Task.Run(() => { }));

         var orderProcessingService = new OracleOrderProcessingService(orderRepo.Object, statusManagementService.Object, converter.Object, fileService.Object, _logger.Object);

         var t = orderProcessingService.SubmitOrder(1);
         t.Wait();
      }

      [TestMethod]
      public void SubmitOrder_ConversionFails_CatchesExceptionAndSetsStatusToFailed()
      {
         var orderRepo = new Mock<IOrderRepository>();
         orderRepo.Setup(f => f.Find(1)).Returns(new Order());

         var statusManagementService = new Mock<IOrderStatusManagementService>();

         var converter = new Mock<IHealthTrackToOracleOrderConverter>();
         converter.Setup(f => f.ConvertOrder(It.IsAny<Order>())).Throws(new Exception());

         var fileService = new Mock<IOracleOutgoingFileService>();
         fileService.Setup(f => f.Send(It.IsAny<OracleOutboundOrder>())).Returns(Task.Run(() => { }));

         var logger = new Mock<ICustomLogger>();

         var orderProcessingService = new OracleOrderProcessingService(orderRepo.Object, statusManagementService.Object, converter.Object, fileService.Object, _logger.Object);

         var t = orderProcessingService.SubmitOrder(1);
         t.Wait();

         Assert.AreEqual(TaskStatus.RanToCompletion, t.Status);
         fileService.Verify(f => f.Send(It.IsAny<OracleOutboundOrder>()), Times.Never);
         statusManagementService.Verify(f => f.UpdateOrderToFailed(1), Times.Once);
      }

      [TestMethod]
      public void SubmitOrder_SendFails_CatchesExceptionAndSetsStatusToFailed()
      {
         var orderRepo = new Mock<IOrderRepository>();
         orderRepo.Setup(f => f.Find(1)).Returns(new Order());

         var statusManagementService = new Mock<IOrderStatusManagementService>();

         var converter = new Mock<IHealthTrackToOracleOrderConverter>();
         converter.Setup(f => f.ConvertOrder(It.IsAny<Order>())).Returns(_order);

         var fileService = new Mock<IOracleOutgoingFileService>();
         fileService.Setup(f => f.Send(It.IsAny<OracleOutboundOrder>())).Throws(new Exception());

         var logger = new Mock<ICustomLogger>();

         var orderProcessingService = new OracleOrderProcessingService(orderRepo.Object, statusManagementService.Object, converter.Object, fileService.Object, _logger.Object);

         var t = orderProcessingService.SubmitOrder(1);
         t.Wait();

         Assert.AreEqual(TaskStatus.RanToCompletion, t.Status);
         fileService.Verify(f => f.Send(It.IsAny<OracleOutboundOrder>()), Times.Once);
         statusManagementService.Verify(f => f.UpdateOrderToFailed(1), Times.Once);
      }
   }
}