using HMS.HealthTrack.Api.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Api.Test.OrderingIntegration
{
    [TestClass]
    public class OracleInboundFileTests
    {
        public void FileType_NameStartsWithPO_IsPurchaseOrder()
        {
            var fileA = new OracleInboundFile("PO_FileName", "");

            Assert.AreEqual(OracleIncomingFileType.PurchaseOrder, fileA.IncomingFileType);
        }

        public void FileType_NameDoesNotStartWithPO_IsErrorReport()
        {
            var fileA = new OracleInboundFile("Error_FileName", "");

            Assert.AreEqual(OracleIncomingFileType.ErrorReport, fileA.IncomingFileType);
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
