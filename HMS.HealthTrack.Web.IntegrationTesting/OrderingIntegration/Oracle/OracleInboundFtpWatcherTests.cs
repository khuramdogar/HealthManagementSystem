using System;
using System.IO;
using System.Net;
using System.Threading;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using HMS.HealthTrack.Web.Data.Model.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;
using Moq;
using NUnit.Framework;

namespace HMS.HealthTrack.Web.IntegrationTesting.OrderingIntegration.Oracle
{
   class OracleInboundFtpWatcherTests
   {
      readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim();

      /// <summary>
      /// This test writes a file to ftp, and checks that ftp watcher processes properly
      /// Has a timeout to indicate failure.
      /// </summary>
      [Test, Timeout(15000), Explicit]
      public void Start_StartWatchingFtpServer_SuccessfullyReadsNewFile()
      {
         _resetEvent.Reset();

         var fileName = Guid.NewGuid().ToString();
         const string fileContents = "Line #1\r\nLine #2\r\nLine #3\r\n";

         const string server = "ftp://192.168.0.174";
         const string user = "testFtp";
         const string password = "ftp";

         var config = new Mock<IConfigurationRepository>();

         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpIncomingServer)).Returns("ftp://192.168.0.174");
         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpIncomingUsername)).Returns("testFtp");
         config.Setup(f => f.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpIncomingPassword)).Returns("ftp");

         var incomingFileService = new Mock<IOracleIncomingFileService>();

         var expectedFile = new OracleInboundFile(fileName, fileContents);
         var cancellationTokenSource = new CancellationTokenSource();

         incomingFileService.Setup(f => f.ProcessFile(expectedFile))
             .Callback((OracleInboundFile file) =>
             {
                cancellationTokenSource.Cancel();
                _resetEvent.Set();
             });

         var logger = new Mock<ICustomLogger>();

         var oracleFileWatcher = new OracleInboundFtpWatcher(
             config.Object,
             (type) => incomingFileService.Object,
             logger.Object);

         oracleFileWatcher.StartMonitoringIncomingFiles(cancellationTokenSource.Token);

         var request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", server, fileName));

         request.Method = WebRequestMethods.Ftp.UploadFile;
         request.Credentials = new NetworkCredential(user, password);

         using (var memoryStream = new MemoryStream())
         using (var streamWriter = new StreamWriter(memoryStream))
         {
            streamWriter.Write(fileContents);

            streamWriter.Flush();

            using (var ftpStream = request.GetRequestStream())
            {
               ftpStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
         }

         try
         {
            _resetEvent.Wait(cancellationTokenSource.Token);
         }
         catch (OperationCanceledException) { }
      }
   }
}
