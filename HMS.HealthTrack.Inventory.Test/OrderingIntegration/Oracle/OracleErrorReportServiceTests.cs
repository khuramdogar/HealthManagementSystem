using HMS.HealthTrack.Inventory.OrderingIntegration;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Oracle
{
    [TestClass]
    public class OracleErrorReportServiceTests
    {
        [TestMethod]
        public void ProcessFile_MockDatabaseOneInput_UpdatesStatusToFailed()
        {
            var orderingSubmissionService = new Mock<IOrderStatusManagementService>();
            var archiveRepo = new Mock<IOraclePurchaseOrderErrorRepository>();

            var poService = new OracleErrorReportService(orderingSubmissionService.Object, archiveRepo.Object);

            var file = new OracleInboundFile("HTRAK_02072010210145_NMW.ERROR", "1|2|item|4|19/11/14 12:00:00|flag|code|");

            poService.ProcessFile(file);

            orderingSubmissionService.Verify(f => f.UpdateOrderToFailed(1), Times.Once);
        }

        [TestMethod]
        public void ProcessFile_MockDatabaseOneInput_IsArchived()
        {
            var orderingSubmissionService = new Mock<IOrderStatusManagementService>();
            var archiveRepo = new Mock<IOraclePurchaseOrderErrorRepository>();

            var poService = new OracleErrorReportService(orderingSubmissionService.Object, archiveRepo.Object);

            var file = new OracleInboundFile("HTRAK_02072010210145_NMW.ERROR", "1|2|item|4|19/11/14 12:00:00|flag|code|");

            poService.ProcessFile(file);

            archiveRepo.Verify(f => f.Add(It.IsAny<OraclePurchaseOrderError>()), Times.Once);
        }

        [TestMethod]
        public void ProcessFile_MockDatabaseMultipleInputs_UpdatesStatusToFailed()
        {
            var orderingSubmissionService = new Mock<IOrderStatusManagementService>();
            var archiveRepo = new Mock<IOraclePurchaseOrderErrorRepository>();

            var poService = new OracleErrorReportService(orderingSubmissionService.Object, archiveRepo.Object);

            var file = new OracleInboundFile("HTRAK_02072010210145_NMW.ERROR", "1|2|item|4|19/11/14 12:00:00|flag|code|\r\n2|2|item|4|19/11/14 12:00:00|flag|code|\r\n3|2|item|4|19/11/14 12:00:00|flag|code|");

            poService.ProcessFile(file);

            orderingSubmissionService.Verify(f => f.UpdateOrderToFailed(1), Times.Once);
            orderingSubmissionService.Verify(f => f.UpdateOrderToFailed(2), Times.Once);
            orderingSubmissionService.Verify(f => f.UpdateOrderToFailed(3), Times.Once);
        }

        [TestMethod]
        public void ProcessFile_MockDatabaseMultipleInputs_AreArchived()
        {
            var orderingSubmissionService = new Mock<IOrderStatusManagementService>();
            var archiveRepo = new Mock<IOraclePurchaseOrderErrorRepository>();

            var poService = new OracleErrorReportService(orderingSubmissionService.Object, archiveRepo.Object);

            var file = new OracleInboundFile("HTRAK_02072010210145_NMW.ERROR", "1|2|item|4|19/11/14 12:00:00|flag|code|\r\n2|2|item|4|19/11/14 12:00:00|flag|code|\r\n3|2|item|4|19/11/14 12:00:00|flag|code|");

            poService.ProcessFile(file);

            archiveRepo.Verify(f => f.Add(It.IsAny<OraclePurchaseOrderError>()), Times.Exactly(3));
        }
    }
}
