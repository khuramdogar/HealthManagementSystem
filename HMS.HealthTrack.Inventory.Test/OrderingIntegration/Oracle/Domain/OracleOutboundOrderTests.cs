using System;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Oracle
{
    [TestClass]
    public class OracleOutboundOrderTests
    {
        [TestMethod]
        public void ToPipeSeperatedLines_NoExtraInfo_ReturnsCorrectString()
        {
            var file = new OracleOutboundOrderItem(123456, 1, 'H', 1448, "PO AUTO RECEIPT", new OracleOutboundOrderItemIdentifier(null, "7309"), 10,
                "PITCAITHLEY, MEG", "SHS01.N2553.24076.000.000", "CRA L1 DAY SURGERY", "PITCAITHLEY, MEG", "CLA", 'N',
                163, "HTRAK", null);

            Assert.AreEqual("H|1448|PO AUTO RECEIPT||7309|10|||||123456|1|PITCAITHLEY, MEG|SHS01.N2553.24076.000.000|CRA L1 DAY SURGERY|PITCAITHLEY, MEG|CLA||N||||||||||||163|HTRAK|", 
                file.ToPipeSeperatedLines()[0]);
        }

        [TestMethod]
        public void ToPipeSeperatedLines_WithOptionalInfo_ReturnsCorrectString()
        {
            var optionalInfo = new OracleOutboundOrderOptionalInfo(new DateTime(2014, 11, 24), "attribute1", "attribute2", "attribute3", "attribute4",
                "attribute5", "attribute6", "attribute7", "attribute8", "flag", "note");

            var file = new OracleOutboundOrderItem(123456, 1, 'H', 1448, "PO AUTO RECEIPT", new OracleOutboundOrderItemIdentifier(null, "7309"), 10,
                "PITCAITHLEY, MEG", "SHS01.N2553.24076.000.000", "CRA L1 DAY SURGERY", "PITCAITHLEY, MEG", "CLA", 'N',
                163, "HTRAK", null, optionalInfo);

            Assert.AreEqual("H|1448|PO AUTO RECEIPT||7309|10|||||123456|1|PITCAITHLEY, MEG|SHS01.N2553.24076.000.000|CRA L1 DAY SURGERY|PITCAITHLEY, MEG|CLA|24-11-2014|N|attribute1|attribute2|attribute3|attribute4|attribute5|attribute6|attribute7|attribute8|flag||note|163|HTRAK|",
                file.ToPipeSeperatedLines()[0]);
        }

        [TestMethod]
        public void ToPipeSeperatedLines_WithDistributionLines_ReturnsCorrectStrings()
        {
            var item1 = new OracleOutboundOrderDistributionLine('D', 123456, 1, 5, "SHS01.N2553.24076.000.000");
            var item2 = new OracleOutboundOrderDistributionLine('D', 123456, 1, 5, "SHS01.N2553.32036.000.000");

            var file = new OracleOutboundOrderItem(123456, 1, 'H', 1448, "PO AUTO RECEIPT", new OracleOutboundOrderItemIdentifier(null, "7309"), 10,
                "PITCAITHLEY, MEG", "SHS01.N2553.24076.000.000", "CRA L1 DAY SURGERY", "PITCAITHLEY, MEG", "CLA", 'N',
                163, "HTRAK", new[] { item1, item2 });

            Assert.AreEqual("D|123456|1|5|SHS01.N2553.24076.000.000|", file.ToPipeSeperatedLines()[1]);
            Assert.AreEqual("D|123456|1|5|SHS01.N2553.32036.000.000|", file.ToPipeSeperatedLines()[2]);
        }

        [TestMethod]
        public void ToPipeSeperatedLines_WithNonCatalogInfo_ReturnsCorrectString()
        {
            var nonCatalogInfo = new OracleOutboundOrderNonCatalogInfo("description", 101, 12.50, "cm", "FT");

            var file = new OracleOutboundOrderItem(123456, 1, 'H', 1448, "PO AUTO RECEIPT", new OracleOutboundOrderItemIdentifier(null, "7309"), 10,
                "PITCAITHLEY, MEG", "SHS01.N2553.24076.000.000", "CRA L1 DAY SURGERY", "PITCAITHLEY, MEG", "CLA", 'N',
                163, "HTRAK", null, nonCatalogInfo);

            Assert.AreEqual("H|1448|PO AUTO RECEIPT||7309|10|description|101|12.50|cm|123456|1|PITCAITHLEY, MEG|SHS01.N2553.24076.000.000|CRA L1 DAY SURGERY|PITCAITHLEY, MEG|CLA||N||||||||||FT||163|HTRAK|",
               file.ToPipeSeperatedLines()[0]);
        }

        [TestMethod]
        public void ToPipeSeperatedLines_WithNonCatalogAndOptionalInfo_ReturnsCorrectString()
        {
            var optionalInfo = new OracleOutboundOrderOptionalInfo(new DateTime(2014, 11, 24), "attribute1", "attribute2", "attribute3", "attribute4",
                "attribute5", "attribute6", "attribute7", "attribute8", "flag", "note");

            var nonCatalogInfo = new OracleOutboundOrderNonCatalogInfo("description", 101, 12.50, "cm", "FT");

            var file = new OracleOutboundOrderItem(123456, 1, 'H', 1448, "PO AUTO RECEIPT", new OracleOutboundOrderItemIdentifier(null, "7309"), 10,
                "PITCAITHLEY, MEG", "SHS01.N2553.24076.000.000", "CRA L1 DAY SURGERY", "PITCAITHLEY, MEG", "CLA", 'N',
                163, "HTRAK", null, optionalInfo, nonCatalogInfo);

            Assert.AreEqual("H|1448|PO AUTO RECEIPT||7309|10|description|101|12.50|cm|123456|1|PITCAITHLEY, MEG|SHS01.N2553.24076.000.000|CRA L1 DAY SURGERY|PITCAITHLEY, MEG|CLA|24-11-2014|N|attribute1|attribute2|attribute3|attribute4|attribute5|attribute6|attribute7|attribute8|flag|FT|note|163|HTRAK|",
               file.ToPipeSeperatedLines()[0]);
        }
    }
}
