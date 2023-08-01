using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Inventory.OrderingIntegration;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OraclePurchaseOrderReceipt = HMS.HealthTrack.Web.Data.Model.Inventory.OraclePurchaseOrderReceipt;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Oracle
{
    [TestClass]
    public class OraclePurchaseOrderServiceTests
    {
        [TestMethod]
        public void ProcessFile_MockDatabaseOneInput_UpdatesStatusToConfirmed()
        {
            var orderingSubmissionService = new Mock<IOrderStatusManagementService>();
            var archiveRepo = new Mock<IOraclePurchaseOrderReceiptRepository>();

            var poService = new OraclePurchaseOrderService(orderingSubmissionService.Object, archiveRepo.Object);

            var file = new OracleInboundFile("PO_02072010210145_NMW.HTRAKFMIS", "1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");

            poService.ProcessFile(file);

            orderingSubmissionService.Verify(f => f.UpdateOrderToOrdered(1), Times.Once);
        }

        [TestMethod]
        public void ProcessFile_MockDatabaseOneInput_IsArchived()
        {
            var orderingSubmissionService = new Mock<IOrderStatusManagementService>();
            var archiveRepo = new Mock<IOraclePurchaseOrderReceiptRepository>();

            var poService = new OraclePurchaseOrderService(orderingSubmissionService.Object, archiveRepo.Object);

            var file = new OracleInboundFile("PO_02072010210145_NMW.HTRAKFMIS", "1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");

            poService.ProcessFile(file);

            archiveRepo.Verify(f => f.Add(It.IsAny<OraclePurchaseOrderReceipt>()), Times.Once);
        }

        [TestMethod]
        public void ProcessFile_MockDatabaseMultipleInputs_UpdatesStatusToConfirmed()
        {
            var orderingSubmissionService = new Mock<IOrderStatusManagementService>();
            var archiveRepo = new Mock<IOraclePurchaseOrderReceiptRepository>();

            var poService = new OraclePurchaseOrderService(orderingSubmissionService.Object, archiveRepo.Object);

            var file = new OracleInboundFile("PO_02072010210145_NMW.HTRAKFMIS", "1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|\r\n2|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|\r\n3|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");

            poService.ProcessFile(file);

            orderingSubmissionService.Verify(f => f.UpdateOrderToOrdered(1), Times.Once);
            orderingSubmissionService.Verify(f => f.UpdateOrderToOrdered(2), Times.Once);
            orderingSubmissionService.Verify(f => f.UpdateOrderToOrdered(3), Times.Once);
        }

        [TestMethod]
        public void ProcessFile_MockDatabaseMultipleInputs_AreArchived()
        {
            var orderingSubmissionService = new Mock<IOrderStatusManagementService>();
            var archiveRepo = new Mock<IOraclePurchaseOrderReceiptRepository>();

            var poService = new OraclePurchaseOrderService(orderingSubmissionService.Object, archiveRepo.Object);

            var file = new OracleInboundFile("PO_02072010210145_NMW.HTRAKFMIS", "1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|\r\n2|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|\r\n3|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");

            poService.ProcessFile(file);

            archiveRepo.Verify(f => f.Add(It.IsAny<OraclePurchaseOrderReceipt>()), Times.Exactly(3));
        }
    }
}
