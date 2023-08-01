using System;
using System.IO;
using System.Threading;
using HMS.HealthTrack.Inventory.OrderingIntegration.Oracle;
using Moq;
using NUnit.Framework;

namespace HMS.HealthTrack.Web.IntegrationTesting.OrderingIntegration.Oracle
{
    [TestFixture]
    public class OracleInboundFileWatcherTests
    {
        readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim();

        /// <summary>
        /// This test writes a file to disk, checks that file watcher processes properly and then deletes file.
        /// Requires write permissions for local disk and has a timeout incase there is an issue.
        /// </summary>
        [Test, Timeout(2000), Explicit]
        public void StartMonitoringIncomingFiles_WatchLocalDirectory_SuccessfullyReadsNewFile()
        {
            _resetEvent.Reset();

            var fileName = string.Format("PO_{0}.txt", Guid.NewGuid());
            var filePath = string.Format(@"C:\{0}", fileName);

            var incomingFileService = new Mock<IOracleIncomingFileService>();

            var expectedFile = new OracleInboundFile(fileName, "Line #1\r\nLine #2\r\nLine #3\r\n");

            incomingFileService.Setup(f => f.ProcessFile(expectedFile))
                .Callback((OracleInboundFile file) =>
                {
                    File.Delete(filePath);
                    _resetEvent.Set();
                });

            var cancellationTokenSource = new CancellationTokenSource();

            var oracleFileWatcher = new OracleInboundFileWatcher(
                () => new FileSystemWatcher(@"C:\"),
                (type) => incomingFileService.Object);

            oracleFileWatcher.StartMonitoringIncomingFiles(cancellationTokenSource.Token);

            using (var fileWriter = new StreamWriter(filePath))
            {
                fileWriter.WriteLine("Line #1");
                fileWriter.WriteLine("Line #2");
                fileWriter.WriteLine("Line #3");
                fileWriter.Close();
            }

            _resetEvent.Wait();
        }

        /// <summary>
        /// Test to allow manual verification that cancel works
        /// </summary>
        [Test, Timeout(10000), Explicit]
        public void StartMonitoringIncomingFiles_Cancelled_SuccessfullyCancels()
        {
            var incomingFileService = new Mock<IOracleIncomingFileService>();

            var cancellationTokenSource = new CancellationTokenSource();

            var oracleFileWatcher = new OracleInboundFileWatcher(
                () => new FileSystemWatcher(@"C:\"),
                (type) => incomingFileService.Object);

            oracleFileWatcher.StartMonitoringIncomingFiles(cancellationTokenSource.Token);

            cancellationTokenSource.Cancel();

            Thread.Sleep(10 * 1000);
        }
    }
}
