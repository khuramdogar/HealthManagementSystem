using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Oracle
{
    [TestClass]
    public class OracleOutboundOrderItemTests
    {
        [TestMethod]
        public void ToPipeSeperated_PopulatedObject_ReturnsCorrectString()
        {
            var outboundOrderItem = new OracleOutboundOrderDistributionLine('D', 123456, 1, 5, "SHS01.N2553.24076.000.000");

            Assert.AreEqual("D|123456|1|5|SHS01.N2553.24076.000.000|", outboundOrderItem.ToPipeSeperated());
        }
    }
}
