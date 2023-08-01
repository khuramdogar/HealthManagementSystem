using System;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Oracle
{
    [TestClass]
    public class OracleErrorReportTests
    {
        private const string ValidInput = "1|2|item|4|19/11/14 12:00:00|flag|code|";

        [TestMethod]
        public void Constructor_ValidInput_FeederSystemRequisitionNumberIsAssigned()
        {
            var errorReport = new OraclePurchaseOrderErrorReport(ValidInput);

            Assert.AreEqual(1, errorReport.OrderId);
        }

        [TestMethod]
        public void Constructor_ValidInput_FeederSystemLineNumberIsAssigned()
        {
            var errorReport = new OraclePurchaseOrderErrorReport(ValidInput);

            Assert.AreEqual(2, errorReport.OrderLineId);
        }

        [TestMethod]
        public void Constructor_ValidInput_ItemDescriptionIsAssigned()
        {
            var errorReport = new OraclePurchaseOrderErrorReport(ValidInput);

            Assert.AreEqual("item", errorReport.ItemDescription);
        }

        [TestMethod]
        public void Constructor_ValidInput_RequestIdIsAssigned()
        {
            var errorReport = new OraclePurchaseOrderErrorReport(ValidInput);

            Assert.AreEqual(4, errorReport.RequestId);
        }

        [TestMethod]
        public void Constructor_ValidInput_CreationDateIsAssigned()
        {
            var errorReport = new OraclePurchaseOrderErrorReport(ValidInput);

            Assert.AreEqual(new DateTime(2014, 11, 19, 12, 00, 00), errorReport.CreationDate);
        }

        [TestMethod]
        public void Constructor_ValidInput_InterfaceSourceCodeIsAssigned()
        {
            var errorReport = new OraclePurchaseOrderErrorReport(ValidInput);

            Assert.AreEqual("code", errorReport.InterfaceSourceCode);
        }

        [TestMethod]
        public void Constructor_ValidInput_ProcessFlagIsAssigned()
        {
            var errorReport = new OraclePurchaseOrderErrorReport(ValidInput);

            Assert.AreEqual("flag", errorReport.ProcessFlag);
        }

        [TestMethod]
        public void Constructor_TooFewArguments_ThrowsException()
        {
            try
            {
                var errorReport = new OraclePurchaseOrderErrorReport("1|2|item|");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(
                    "exception=Invalid number of fields in input, inputLine=\"1|2|item|\", delimeter='|', expectedCount=8, actualFieldCount=4",
                    ex.Message);
            }
            
        }

        [TestMethod]
        public void Constructor_InvalidFeederSystemLineNumber_ThrowsException()
        {
            try
            {
                var errorReport = new OraclePurchaseOrderErrorReport("1|x|item|4|19/11/14 12:00:00|flag|code|");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(
                    "exception=Unable to convert to specified type, field=FeederSystemLineNumber, value=\"x\", type=System.Int64",
                    ex.Message);
            }
        }

        public void Equals_ValuesAreEqual_ReturnsTrue()
        {
            var a = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|code|");
            var b = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|code|");

            Assert.AreEqual(a, b);
        }

        public void Equals_FeederSystemRequisitionNumberAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|flag|code|");
            var b = new OraclePurchaseOrderErrorReport("2|2|item|4|19/11/14 12:00:00|flag|code|");

            Assert.AreNotEqual(a, b);
        }

        public void Equals_FeederSystemLineNumberAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|flag|code|");
            var b = new OraclePurchaseOrderErrorReport("1|3|item|4|19/11/14 12:00:00|flag|code|");

            Assert.AreNotEqual(a, b);
        }

        public void Equals_ItemDescriptionAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|flag|code|");
            var b = new OraclePurchaseOrderErrorReport("1|2|item1|4|19/11/14 12:00:00|flag|code|");

            Assert.AreNotEqual(a, b);
        }

        public void Equals_RequestIdAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|flag|code|");
            var b = new OraclePurchaseOrderErrorReport("1|2|item|5|19/11/14 12:00:00|flag|code|");

            Assert.AreNotEqual(a, b);
        }

        public void Equals_CreationDateAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|flag|code|");
            var b = new OraclePurchaseOrderErrorReport("1|2|item|4|20/11/14 12:00:00|flag|code|");

            Assert.AreNotEqual(a, b);
        }

        public void Equals_ProcessFlagAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|flag|code|");
            var b = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|flag2|code|");

            Assert.AreNotEqual(a, b);
        }
        public void Equals_InterfaceSourceCodeAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|flag|code|");
            var b = new OraclePurchaseOrderErrorReport("1|2|item|4|19/11/14 12:00:00|flag|code2|");

            Assert.AreNotEqual(a, b);
        }
    }
}
