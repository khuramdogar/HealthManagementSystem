using System.IO;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;
using Renci.SshNet;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
   internal class OracleOutgoingFtpService : IOracleOutgoingFileService
   {
      private string _ftpEndpoint;
      private string _ftpUsername;
      private string _ftpPassword;

      private readonly IConfigurationRepository _configurationRepository;
      private readonly ICustomLogger _logger;
      private readonly ITimeProvider _timeProvider;

      public OracleOutgoingFtpService(
         IConfigurationRepository configurationRepository,
         ITimeProvider timeProvider, ICustomLogger logger)
      {
         _configurationRepository = configurationRepository;
         _logger = logger;
         _timeProvider = timeProvider ?? new TimeProvider();
         ConfigureService();
      }

      private void ConfigureService()
      {
         _ftpEndpoint = _configurationRepository.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpOutgoingServer);

         if (string.IsNullOrEmpty(_ftpEndpoint))
            _logger.Warning("exception=Cannot send oracle file as FTP Endpoint is not configured.");

         _ftpUsername = _configurationRepository.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpOutgoingUsername);

         if (string.IsNullOrEmpty(_ftpUsername))
            _logger.Warning("exception=Cannot send oracle file as FTP Username is not configured.");

         _ftpPassword = _configurationRepository.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.FtpOutgoingPassword);

         if (string.IsNullOrEmpty(_ftpPassword))
            _logger.Warning("exception=Cannot send oracle file as FTP Password is not configured.");
      }

      public Task Send(OracleOutboundOrder oracleOrderItem)
      {
         return Task.Run(() =>
         {
            var lines = oracleOrderItem.ToPipeSeperatedLines();

            // OracleFileNameProvider?
            var fileName = string.Format("PO_{0:ddMMyyyyHHmmss}_{1}.HTRAK", _timeProvider.GetCurrentTime(), "TST");
            
            using (var client = new SftpClient(_ftpEndpoint, 22, _ftpUsername, _ftpPassword))
            {
               client.Connect();

               client.BufferSize = 4*1024; // bypass Payload error large files
               client.WriteAllLines(Path.GetFileName(fileName), lines);
            }
         });
      }
   }
}