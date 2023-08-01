using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using HMS.HealthTrack.Web.Data.Model.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;
using Moq;
using NUnit.Framework;
using Serilog;

namespace HMS.HealthTrack.Web.IntegrationTesting.OrderingIntegration.Oracle
{
   [TestFixture]
   public class OracleOutgoingFileServiceTests
   {
      private OracleOutboundOrder GenerateTestOrder_SingleItem()
      {
         var dist1 = new OracleOutboundOrderDistributionLine('D', 123456, 1, 5, "SHS01.N2553.24076.000.000");
         var dist2 = new OracleOutboundOrderDistributionLine('D', 123456, 1, 5, "SHS01.N2553.32036.000.000");

         var lineItem = new OracleOutboundOrderItem(123456, 1, 'H', 1448, "PO AUTO RECEIPT", new OracleOutboundOrderItemIdentifier(null, "7309"), 10,
             "PITCAITHLEY, MEG", "SHS01.N2553.24076.000.000", "CRA L1 DAY SURGERY", "PITCAITHLEY, MEG", "CLA", 'N',
             163, "HTRAK", new[] { dist1, dist2 });

         return new OracleOutboundOrder {OrderItems = new []{lineItem}};
      }

      private OracleOutboundOrder GenerateTestOrder_MultipleItems()
      {
         var dist1 = new OracleOutboundOrderDistributionLine('D', 123456, 1, 5, "SHS01.N2553.24076.000.000");
         var dist2 = new OracleOutboundOrderDistributionLine('D', 123456, 1, 5, "SHS01.N2553.32036.000.000");

         var lineItem1 = new OracleOutboundOrderItem(123456, 1, 'H', 1448, "PO AUTO RECEIPT", new OracleOutboundOrderItemIdentifier("SCP1", "7309"), 10,
             "PITCAITHLEY, MEG", "SHS01.N2553.24076.000.000", "CRA L1 DAY SURGERY", "PITCAITHLEY, MEG", "CLA", 'N',
             163, "HTRAK", new[] { dist1, dist2 });

         var dist3 = new OracleOutboundOrderDistributionLine('D', 123456, 2, 5, "SHS01.N2553.24076.000.000");
         var dist4 = new OracleOutboundOrderDistributionLine('D', 123456, 2, 5, "SHS01.N2553.32036.000.000");

         var lineItem2 = new OracleOutboundOrderItem(123456, 2, 'H', 1448, "PO AUTO RECEIPT", new OracleOutboundOrderItemIdentifier("SCP2", "8559"), 10,
             "PITCAITHLEY, MEG", "SHS01.N2553.24076.000.000", "CRA L1 DAY SURGERY", "PITCAITHLEY, MEG", "CLA", 'N',
             163, "HTRAK", new[] { dist3, dist4 });

         return new OracleOutboundOrder { OrderItems = new[] { lineItem1,lineItem2 } };
      }

      [Test, Explicit]
      public void Send_ValidFTPEndPoint_SentSuccessfully()
      {
         var config = new Mock<IConfigurationRepository>();

         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpOutgoingServer)).Returns("distribution.healthtrack.com.au");
         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpOutgoingUsername)).Returns("healthtrackftp");
         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpOutgoingPassword)).Returns("indr4949");

         var outgoingFileService = new OracleOutgoingFtpService(config.Object, new TimeProvider(), new CustomLogger(Log.Logger));

         var file = GenerateTestOrder_SingleItem();

         outgoingFileService.Send(file).Wait();
      }

      [Test, Explicit]
      public void Send_ValidFileSystemEndPoint_SentSuccessfully()
      {
         var config = new Mock<IConfigurationRepository>();
         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.OutgoingFilePath)).Returns(@"C:\Temp");
         
         var outgoingFileService = new OracelOutgoingFileSystemService(config.Object, new TimeProvider(), new CustomLogger(Log.Logger));
         var file = GenerateTestOrder_SingleItem();

         outgoingFileService.Send(file).Wait();
      }

      [Test, Explicit]
      public void Send_ValidFileSystemEndPoint_MultipleItems_SentSuccessfully()
      {
         var config = new Mock<IConfigurationRepository>();
         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.OutgoingFilePath)).Returns(@"C:\Temp");

         var outgoingFileService = new OracelOutgoingFileSystemService(config.Object, new TimeProvider(), new CustomLogger(Log.Logger));
         var file = GenerateTestOrder_MultipleItems();

         outgoingFileService.Send(file).Wait();
      }
   }
}
