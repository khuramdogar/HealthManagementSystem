using System;
using System.IO;
using System.Threading;
using HMS.HealthTrack.Api.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Api.Test.OrderingIntegration
{
    [TestClass]
    public class OracleInboundFileWatcherTests
    {
        readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim();

        /// <summary>
        /// This test writes a file to disk, checks that file watcher processes properly and then deletes file.
        /// Requires write permissions for local disk and has a timeout incase there is an issue.
        /// </summary>
        [TestMethod, Timeout(2000), Ignore]
        public void Start_WatchLocalDirectory_SuccessfullyReadsNewFile()
        {
            _resetEvent.Reset();

            var fileName = string.Format("PO_{0}.txt", Guid.NewGuid());
            var filePath = string.Format(@"C:\{0}", fileName);

            var archivingService = new Mock<IOracleFileArchivingService>();

            var incomingFileService = new Mock<IOracleIncomingFileService>();

            var expectedFile = new OracleInboundFile(fileName, "Line #1\r\nLine #2\r\nLine #3\r\n");

            incomingFileService.Setup(f => f.ProcessFile(expectedFile))
                .Callback((OracleInboundFile file) =>
                {
                    File.Delete(filePath);
                    _resetEvent.Set();
                });

            var oracleFileWatcher = new OracleInboundFileWatcher(
                () => new FileSystemWatcher(@"C:\"),
                archivingService.Object,
                (type) => incomingFileService.Object);

            oracleFileWatcher.Start();

            using (var fileWriter = new StreamWriter(filePath))
            {
                fileWriter.WriteLine("Line #1");
                fileWriter.WriteLine("Line #2");
                fileWriter.WriteLine("Line #3");
                fileWriter.Close();
            }

            _resetEvent.Wait();
        }
    }
}
