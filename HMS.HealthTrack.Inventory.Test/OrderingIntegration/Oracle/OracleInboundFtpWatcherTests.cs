using System;
using System.Threading;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using HMS.HealthTrack.Web.Data.Model.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Oracle
{
   [TestClass]
   public class OracleInboundFtpWatcherTests
   {
      [TestMethod, Timeout(2000)]
      public void Start_StartWatchingFtpServer_CancelFinishesTask()
      {
         var incomingFileService = new Mock<IOracleIncomingFileService>();

         var cancellationTokenSource = new CancellationTokenSource();

         var config = new Mock<IConfigurationRepository>();

         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpIncomingServer)).Returns("ftp://192.168.0.174");
         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpIncomingUsername)).Returns("testFtp");
         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpIncomingPassword)).Returns("ftp");

         var logger = new Mock<ICustomLogger>();

         var oracleFileWatcher = new OracleInboundFtpWatcher(
             config.Object,
             (type) => incomingFileService.Object,
             logger.Object);

         var t = oracleFileWatcher.StartMonitoringIncomingFiles(cancellationTokenSource.Token);

         cancellationTokenSource.Cancel();

         try
         {
            t.Wait(cancellationTokenSource.Token);
         }
         catch (Exception ex)
         {
            Assert.IsInstanceOfType(ex, typeof(OperationCanceledException));
         }
      }
   }
}
