using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Oracle
{
    [TestClass]
    public class OracleInboundFileTests
    {
        public void FileType_NameStartsWithPO_IsPurchaseOrder()
        {
            var fileA = new OracleInboundFile("PO_FileName", "");

            Assert.AreEqual(OracleInboundFileType.PurchaseOrder, fileA.InboundFileType);
        }

        public void FileType_NameDoesNotStartWithPO_IsErrorReport()
        {
            var fileA = new OracleInboundFile("Error_FileName", "");

            Assert.AreEqual(OracleInboundFileType.ErrorReport, fileA.InboundFileType);
        }

        [TestMethod]
        public void Equals_ValuesAreEqual_ReturnsTrue()
        {
            var fileA = new OracleInboundFile("FileName", "Contents");
            var fileB = new OracleInboundFile("FileName", "Contents");

            Assert.AreEqual(fileA, fileB);
        }

        [TestMethod]
        public void Equals_NamesAreNotEqual_ReturnsFalse()
        {
            var fileA = new OracleInboundFile("FileNameA", "Contents");
            var fileB = new OracleInboundFile("FileNameB", "Contents");

            Assert.AreNotEqual(fileA, fileB);
        }

        [TestMethod]
        public void Equals_ValuesAreEqual_ReturnsFalse()
        {
            var fileA = new OracleInboundFile("FileName", "ContentsA");
            var fileB = new OracleInboundFile("FileName", "ContentsB");

            Assert.AreNotEqual(fileA, fileB);
        }
    }
}
