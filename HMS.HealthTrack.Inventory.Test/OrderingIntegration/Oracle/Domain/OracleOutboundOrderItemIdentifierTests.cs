using System;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Oracle
{
    [TestClass]
    public class OracleOutboundOrderItemIdentifierTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_NullArguments_ThrowsException()
        {
            var x = new OracleOutboundOrderItemIdentifier(null, null);
        }
    }
}
