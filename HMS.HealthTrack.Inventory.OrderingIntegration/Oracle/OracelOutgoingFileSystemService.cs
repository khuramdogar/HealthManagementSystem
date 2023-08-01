using System.IO;
using System.Threading.Tasks;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.OrderingIntegration.Common;
using HMS.HealthTrack.Web.Data.Model.Configuration;
using HMS.HealthTrack.Web.Data.Repositories.Configuration;

namespace HMS.HealthTrack.Inventory.OrderingIntegration.Oracle
{
   internal class OracelOutgoingFileSystemService : IOracleOutgoingFileService
   {
      private string _outgoingFilePath;

      private readonly IConfigurationRepository _configurationRepository;
      private readonly ICustomLogger _logger;
      private readonly ITimeProvider _timeProvider;

      public OracelOutgoingFileSystemService(
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
         _outgoingFilePath = _configurationRepository.GetConfigurationValue<string>(ConfigurationPropertyIdentifier.OutgoingFilePath);

         if (string.IsNullOrEmpty(_outgoingFilePath))
            _logger.Warning("exception=Cannot send oracle file as file system path is not configured.");
      }

      public Task Send(OracleOutboundOrder oracleOrderItem)
      {
         return Task.Run(() =>
         {
            //Convert the order object into formatted text lines
            var lines = oracleOrderItem.ToPipeSeperatedLines();

            //Construct the filename
            var fileName = $"PO_{_timeProvider.GetCurrentTime():ddMMyyyyHHmmss}_{FmisAttributes.AgencyCode}.{FmisAttributes.InterfaceType}";

            var fileStream = File.OpenWrite(Path.Combine(_outgoingFilePath, fileName));

            using (var streamWriter = new StreamWriter(fileStream))
            {
               foreach (var l in lines)
               {
                  streamWriter.WriteLine(l);
               }

               streamWriter.Flush();
            }
         });
      }
   }
}
