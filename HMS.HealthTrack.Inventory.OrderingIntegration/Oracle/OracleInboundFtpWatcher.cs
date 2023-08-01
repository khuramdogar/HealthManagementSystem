using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
   internal class OracleInboundFtpWatcher : IInboundOrderReceiptWatcher
   {
      private const int Interval = 10000;

      private readonly string _ftpEndpoint;
      private readonly string _ftpUsername;
      private readonly string _ftpPassword;

      private readonly Func<OracleInboundFileType, IOracleIncomingFileService> _incomingFileServiceFactory;
      private readonly ICustomLogger _customLogger;

      public OracleInboundFtpWatcher(
          IConfigurationRepository configurationRepository,
          Func<OracleInboundFileType, IOracleIncomingFileService> incomingFileServiceFactory,
          ICustomLogger customLogger)
      {
         _ftpEndpoint = configurationRepository.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpIncomingServer);

         if (string.IsNullOrEmpty(_ftpEndpoint))
            throw new ArgumentException("exception=Cannot monitor incoming oracle files as FTP Endpoint is not configured.");

         _ftpUsername = configurationRepository.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpIncomingUsername);

         if (string.IsNullOrEmpty(_ftpUsername))
            throw new ArgumentException("exception=Cannot monitor incoming oracle files as FTP Username is not configured.");

         _ftpPassword = configurationRepository.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpIncomingPassword);

         if (string.IsNullOrEmpty(_ftpPassword))
            throw new ArgumentException("exception=Cannot monitor incoming oracle files as FTP Password is not configured.");

         _incomingFileServiceFactory = incomingFileServiceFactory;
         _customLogger = customLogger;
      }

      public Task StartMonitoringIncomingFiles(CancellationToken cancellationToken)
      {
         _customLogger.Information("msg=OracleInboundFtpWatcher: StartMonitoringIncomingFiles");

         return Task.Run(() =>
         {
            while (!cancellationToken.IsCancellationRequested)
            {
               try
               {
                  ProcessAnyNewFiles();
               }
               catch (Exception ex)
               {
                  _customLogger.Error(string.Format("exception={0}", ex));
               }

               Thread.Sleep(Interval);
            }
         }, cancellationToken);
      }

      private void ProcessAnyNewFiles()
      {
         var request = CreateRequest(WebRequestMethods.Ftp.ListDirectory);

         using (var response = request.GetResponse() as FtpWebResponse)
         using (var responseStream = response.GetResponseStream())
         using (var reader = new StreamReader(responseStream))
         {
            while (!reader.EndOfStream)
            {
               ProcessFile(reader.ReadLine());
            }
         }
      }

      private void ProcessFile(string fileName)
      {
         try
         {
            _customLogger.Information(string.Format("msg=OracleInboundFtpWatcher: ProcessFile, filename={0}", fileName));

            var request = CreateRequest(WebRequestMethods.Ftp.DownloadFile, fileName);

            var response = (FtpWebResponse)request.GetResponse();

            using (var responseStream = response.GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
               var contents = reader.ReadToEnd();

               _customLogger.Information(string.Format("msg=OracleInboundFtpWatcher: ProcessFile, filename={0}, fileContents={1}", fileName, contents));

               var file = new OracleInboundFile(fileName, contents);

               _incomingFileServiceFactory(file.InboundFileType).ProcessFile(file);
            }

            DeleteFile(fileName);
         }
         catch (Exception ex)
         {
            _customLogger.Error(string.Format("exception={0}", ex));
         }
      }

      private void DeleteFile(string fileName)
      {
         var request = CreateRequest(WebRequestMethods.Ftp.DeleteFile, fileName);
         var response = (FtpWebResponse)request.GetResponse();
      }

      private FtpWebRequest CreateRequest(string method, string fileName = null)
      {
         var request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", _ftpEndpoint, fileName));
         request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);

         request.Method = method;

         return request;
      }
   }
}
