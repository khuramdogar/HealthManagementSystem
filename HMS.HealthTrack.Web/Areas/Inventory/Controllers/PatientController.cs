using System;
using System.Web.Mvc;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Repositories.Clinical;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Controllers
{
   [HealthTrackAuthorize(HealthTrackPermission = ("Inventory"))]
   public class PatientController : Controller
   {
      private readonly IMedicalRecordRepository _medicalRecordRepository;
      private readonly IHealthTrackConsumptionRepository _consumptionRepository;
      private readonly ICustomLogger _logger;

      public PatientController(IMedicalRecordRepository medicalRecordRepository, IHealthTrackConsumptionRepository consumptionRepository, ICustomLogger logger)
      {
         _medicalRecordRepository = medicalRecordRepository;
         _consumptionRepository = consumptionRepository;
         _logger = logger;
      }

      // GET: Inventory/Patient
      public ActionResult PatientDetails(long containerId)
      {
         try
         {
            var patientDetails = _medicalRecordRepository.GetPatientFromContainer(containerId);
            return View("PatientDetails", patientDetails);
         }
         catch (Exception exception)
         {
            _logger.Error(exception, string.Format("There was a problem retrieving patient details for the container with ID '{0}'.", containerId));
            return View("PatientDetails", null);
         }
      }

      public ActionResult ConsumptionPatientDetails(int consumptionId)
      {
         var consumption = (from c in _consumptionRepository.FindHealthTrackConsumptions() where c.UsedId == consumptionId select c).SingleOrDefault();
         if (consumption == null || !consumption.ContainerId.HasValue)
         {
            _logger.Warning("There was a problem retrieving patient details for the consumption with ID '{ConsumptionId}'.", consumptionId);
            return View("PatientDetails", null);
         }

         return PatientDetails(consumption.ContainerId.Value);
      }
   }
}