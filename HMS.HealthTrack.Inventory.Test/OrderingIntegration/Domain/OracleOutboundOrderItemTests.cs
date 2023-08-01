using HMS.HealthTrack.Api.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Api.Test.OrderingIntegration
{
    [TestClass]
    public class OracleOutboundOrderItemTests
    {
        [TestMethod]
        public void ToPipeSeperated_PopulatedObject_ReturnsCorrectString()
        {
            var outboundOrderItem = new OracleOutboundOrderItem('D', 123456, 1, 5, "SHS01.N2553.24076.000.000");

            Assert.AreEqual("D|123456|1|5|SHS01.N2553.24076.000.000|", outboundOrderItem.ToPipeSeperated());
        }
    }
}
