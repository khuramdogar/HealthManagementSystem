using System;
using System.Web.Http;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.Clinical;
using HMS.HealthTrack.Web.Data.Repositories.Clinical;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers.API
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class PatientDetailsController : ApiController
    {

       private readonly IMedicalRecordRepository _medicalRecordRepository;
       private readonly ICustomLogger _logger;

       public PatientDetailsController(IMedicalRecordRepository medicalRecordRepository, ICustomLogger logger)
       {
          _medicalRecordRepository = medicalRecordRepository;
          _logger = logger;
       }

      [HttpGet]
       public PatientDetails GetPatientDetails(int? clinicalRecordId)
       {
          try
          {
             return clinicalRecordId.HasValue ? _medicalRecordRepository.GetPatientFromContainer(clinicalRecordId.Value) : null;
          }
          catch (Exception exception)
          {
             _logger.Error(exception,
                $"There was a problem retrieving Patient Details for the clinical record with ID '{(clinicalRecordId.HasValue ? clinicalRecordId.Value.ToString() : string.Empty)}'.");
             return null;
          }
       }
   }
}
