using System.Data.Entity.Infrastructure;
using HMS.HealthTrack.Api.Common;
using HMS.HealthTrack.Api.OrderingIntegration;
using HMS.HealthTrack.Api.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Api.Test.OrderingIntegration
{
    [TestClass]
    public class OracleOutgoingFileServiceTests
    {
        [TestMethod]
        public void Send_ValidEndPoint_SentSuccessfully()
        {
            var outgoingFileService = new OracleOutgoingFileService("ftp://192.168.0.174", "testFtp", "ftp", "NMW", new TimeProvider());

            var item1 = new OracleOutboundOrderItem('D', 123456, 1, 5, "SHS01.N2553.24076.000.000");
            var item2 = new OracleOutboundOrderItem('D', 123456, 1, 5, "SHS01.N2553.32036.000.000");

            var file = new OracleOutboudOrder(
                'H', 1448, "PO AUTO RECEIPT", null, "7309", 10, 123456, 1,
                "PITCAITHLEY, MEG", "SHS01.N2553.24076.000.000", "CRA L1 DAY SURGERY", "PITCAITHLEY, MEG", "CLA", "N",
                163, "HTRAK", new[] { item1, item2 });

            outgoingFileService.Send(file).Wait();
        }
    }
}
