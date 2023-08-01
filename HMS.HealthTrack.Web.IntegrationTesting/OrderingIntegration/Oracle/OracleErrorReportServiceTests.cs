using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using HMS.HealthTrack.Inventory.OrderingIntegration;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using NUnit.Framework;

namespace HMS.HealthTrack.Web.IntegrationTesting.OrderingIntegration.Oracle
{
    [TestFixture]
    public class OracleErrorReportServiceTests
    {
        [Test, Explicit]
        public void ProcessFile_RealDatabase_ArchivesAndUpdatesStatusInDatabase()
        {
            var file = new OracleInboundFile("HTRAK_02072010210145_NMW.ERROR", "1|2|item|4|19/11/14 12:00:00|flag|code|");

            var context = new InventoryContext(GetTestConnectionStringSettings());
            var innerRepo = new OrderRepository(context);

            var orderingSubmissionService = new OrderStatusManagementService(innerRepo);
            var archiveRepo = new OraclePurchaseOrderErrorRepository(context);

            var poService = new OracleErrorReportService(orderingSubmissionService, archiveRepo);

            poService.ProcessFile(file);
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
    }
}
