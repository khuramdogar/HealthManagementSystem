using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HMS.HealthTrack.Api.OrderingIntegration;
using HMS.HealthTrack.Api.OrderingIntegration.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Api.Test.OrderingIntegration
{
    [TestClass]
    public class OracleInboundFtpWatcherTests
    {
        readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim();

        /// <summary>
        /// This test writes a file to disk, checks that file watcher processes properly and then deletes file.
        /// Requires write permissions for local disk and has a timeout incase there is an issue.
        /// </summary>
        [TestMethod, Timeout(15000)]
        public void Start_WatchLocalDirectory_SuccessfullyReadsNewFile()
        {
            _resetEvent.Reset();

            var fileName = string.Format("PO_{0}.txt", Guid.NewGuid());

            var archivingService = new Mock<IOracleFileArchivingService>();

            var incomingFileService = new Mock<IOracleIncomingFileService>();

            var expectedFile = new OracleInboundFile(fileName, "Line #1\r\nLine #2\r\nLine #3\r\n");

            incomingFileService.Setup(f => f.ProcessFile(expectedFile))
                .Callback((OracleInboundFile file) => _resetEvent.Set());

            var oracleFileWatcher = new OracleInboundFtpWatcher(
                "ftp://192.168.0.174", "testFtp", "ftp",
                archivingService.Object,
                (type) => incomingFileService.Object);

            oracleFileWatcher.Start();

            var request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", "ftp://192.168.0.174", fileName));

            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential("testFtp", "ftp");

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                streamWriter.Write("Line #1\r\nLine #2\r\nLine #3\r\n");

                streamWriter.Flush();

                using (var ftpStream = request.GetRequestStream())
                {
                    ftpStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                }
            }

            _resetEvent.Wait();
        }
    }
}
