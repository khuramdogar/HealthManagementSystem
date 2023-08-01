using System;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Oracle
{
    [TestClass]
    public class OraclePurchaseOrderTests
    {
        private const string ValidInput = "1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|";

        [TestMethod]
        public void Constructor_ValidInput_FeederSystemRequisitionNumberIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual(1, purchaseOrder.OrderId);
        }

        [TestMethod]
        public void Constructor_ValidInput_FeederSystemLineNumberIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual(2, purchaseOrder.OrderLineId);
        }

        [TestMethod]
        public void Constructor_ValidInput_FmisRequisitionNumberIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual(3, purchaseOrder.FmisRequisitionNumber);
        }

        [TestMethod]
        public void Constructor_ValidInput_FmisRequisitionLineNumberIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual(4, purchaseOrder.FmisRequisitionLineNumber);
        }

        [TestMethod]
        public void Constructor_ValidInput_FmisRequisitionDistributionNumberIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual(5, purchaseOrder.FmisRequisitionDistributionNumber);
        }

        [TestMethod]
        public void Constructor_ValidInput_FmisDistributionIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual("dist", purchaseOrder.FmisDistribution);
        }

        [TestMethod]
        public void Constructor_ValidInput_FmisVendorCodeIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual(6, purchaseOrder.FmisVendorCode);
        }

        [TestMethod]
        public void Constructor_ValidInput_FmisSupplierProductCodeIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual("supplier", purchaseOrder.FmisSupplierProductCode);
        }

        [TestMethod]
        public void Constructor_ValidInput_FmisHospitalProductCodeIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual("hospital", purchaseOrder.FmisHospitalProductCode);
        }

        [TestMethod]
        public void Constructor_ValidInput_FmisPricePerUnitIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual(12.34, purchaseOrder.FmisPricePerUnit);
        }

        [TestMethod]
        public void Constructor_ValidInput_FmisUnitOfMeasureIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual("7", purchaseOrder.FmisUnitOfMeasure);
        }

        [TestMethod]
        public void Constructor_ValidInput_FmisOrgIdIsAssigned()
        {
            var purchaseOrder = new OraclePurchaseOrderReceipt(ValidInput);

            Assert.AreEqual("8", purchaseOrder.FmisOrgId);
        }

        [TestMethod]
        public void Constructor_TooFewArguments_ThrowsException()
        {
            try
            {
                var purchaseOrder = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(
                    "exception=Invalid number of fields in input, inputLine=\"1|2|3|4|5|dist|\", delimeter='|', expectedCount=13, actualFieldCount=7",
                    ex.Message);
            }
        }

        [TestMethod]
        public void Constructor_InvalidFmisRequisitionNumber_ThrowsException()
        {
            try
            {
                var purchaseOrder = new OraclePurchaseOrderReceipt("1|2|x|4|5|dist|6|supplier|hospital|12.34|7|8|");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(
                    "exception=Unable to convert to specified type, field=FmisRequisitionNumber, value=\"x\", type=System.Int64",
                    ex.Message);
            }
        }

        [TestMethod]
        public void Equals_ValuesAreEqual_ReturnsTrue()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");

            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void Equals_FeederSystemLineNumberAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|20|3|4|5|dist|6|supplier|hospital|12.34|7|8|");

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_FmisRequisitionNumberAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|32|4|5|dist|6|supplier|hospital|12.34|7|8|");

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_FmisRequisitionLineNumberAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|3|41|5|dist|6|supplier|hospital|12.34|7|8|");

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_FmisRequisitionDistributionNumberAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|3|4|52|dist|6|supplier|hospital|12.34|7|8|");

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_FmisDistributionAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist1|6|supplier|hospital|12.34|7|8|");

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_FmisVendorCodeAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|62|supplier|hospital|12.34|7|8|");

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_FmisSupplierProductCodeAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier2|hospital|12.34|7|8|");

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_FmisHospitalProductCodeAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital4|12.34|7|8|");

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_FmisPricePerUnitAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|15.34|7|8|");

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_FmisUnitOfMeasureAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|74|8|");

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void Equals_FmisOrgIdAreNotEqual_ReturnsFalse()
        {
            var a = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|8|");
            var b = new OraclePurchaseOrderReceipt("1|2|3|4|5|dist|6|supplier|hospital|12.34|7|87|");

            Assert.AreNotEqual(a, b);
        }
    }
}
