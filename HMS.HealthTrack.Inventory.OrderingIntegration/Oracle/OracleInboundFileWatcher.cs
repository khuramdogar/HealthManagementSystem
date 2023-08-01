using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
    internal class OracleInboundFileWatcher : IInboundOrderReceiptWatcher
    {
        private const int FileReadRetryAttempts = 3;
        private const int DelayBetweenReadAttemptsSeconds = 2;

        private readonly Func<FileSystemWatcher> _fileSystemWatcherFactory;
        private readonly Func<OracleInboundFileType, IOracleIncomingFileService> _incomingFileServiceFactory;

        public OracleInboundFileWatcher(
            Func<FileSystemWatcher> fileSystemWatcherFactory,
            Func<OracleInboundFileType, IOracleIncomingFileService> incomingFileServiceFactory)
        {
            _fileSystemWatcherFactory = fileSystemWatcherFactory;
            _incomingFileServiceFactory = incomingFileServiceFactory;
        }

        public Task StartMonitoringIncomingFiles(CancellationToken cancellationToken)
        {
            var fileWatcher = _fileSystemWatcherFactory();

            var t = Task.Run(() =>
            {
                fileWatcher.Created += HandleNewFile;

                fileWatcher.EnableRaisingEvents = true;
            }, cancellationToken);

            cancellationToken.Register(() =>
            {
                fileWatcher.Created -= HandleNewFile;
                fileWatcher.EnableRaisingEvents = false;
                fileWatcher.Dispose();
            });

            return t;
        }

        private void HandleNewFile(object sender, FileSystemEventArgs e)
        {
            try
            {
                var fileName = e.Name;
                var contents = TryRead(() => File.ReadAllText(e.FullPath), 0, FileReadRetryAttempts);

                var file = new OracleInboundFile(fileName, contents);

                _incomingFileServiceFactory(file.InboundFileType).ProcessFile(file);
            }
            catch (Exception ex)
            {
                // What to do?
            }
        }

        private static string TryRead(Func<string> readContents, int retryNumber, int maxRetryAttempts)
        {
            string fileContents;

            try
            {
                fileContents = readContents();
            }
            catch (IOException ex)
            {
                if (!ex.Message.Contains("being used by another process") || retryNumber > maxRetryAttempts) 
                    throw;

                Thread.Sleep(DelayBetweenReadAttemptsSeconds * 1000);
                retryNumber++;
                return TryRead(readContents, retryNumber, maxRetryAttempts);
            }

            return fileContents;
        }
    }
}