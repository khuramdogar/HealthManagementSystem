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
    class OraclePurchaseOrderServiceTests
    {
        [Test, Explicit]
        public void ProcessFile_RealDatabase_ArchivesAndUpdatesStatusInDatabase()
        {
            var file = new OracleInboundFile("PO_02072010210145_NMW.HTRAKFMIS", "1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");

            var context = new InventoryContext(GetTestConnectionStringSettings());
            var innerRepo = new OrderRepository(context);

            var orderingSubmissionService = new OrderStatusManagementService(innerRepo);
            var archiveRepo = new OraclePurchaseOrderReceiptRepository(context);

            var poService = new OraclePurchaseOrderService(orderingSubmissionService, archiveRepo);

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
