using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Clinical;

namespace HMS.HealthTrack.Web.Data.Repositories.Clinical
{
   public class MedicalRecordRepository : IMedicalRecordRepository
   {
      private readonly IDbContextClinicalContext _context;

      public MedicalRecordRepository(IDbContextClinicalContext context)
      {
         _context = context;
      }

      public PatientDetails GetPatientFromContainer(long containerId)
      {
         var medicalRecord = from r in _context.MR_Container
            join pcm in _context.PatientContainerMRNs on r.container_ID equals pcm.container_ID into mrns
            from mrn in mrns.DefaultIfEmpty()
            join cpc in _context.ContainerPaymentClasses on r.container_ID equals cpc.container_ID into pc
            from paymentClass in pc.DefaultIfEmpty()
            where r.container_ID == containerId
            select new PatientDetails
            {
               Title = r.Patient.title,
               Surname = r.Patient.surname,
               FirstName = r.Patient.firstName,
               Medicare = r.Patient.medicare,
               Dob = r.Patient.dob,
               PatientId = mrn.RemotePatient_ID ?? "HT" + r.patient_ID,
               ExternalPatientIds = r.Patient.HL7_PatientMapping.Select(m => new ExternalPatientIdentifier {Context = m.ExternalFeed.service_Name, Number = m.RemotePatient_ID}),
               PaymentClass = paymentClass.ItemValue
            };
         return medicalRecord.SingleOrDefault();
      }

      public IList<ExternalPatientIdentifier> GetExternalPatientIdsFromContainer(long containerId)
      {
         var mrns = from c in _context.MR_Container
            where c.container_ID == containerId
            join m in _context.PatientMappings on c.patient_ID equals m.LocalPatient_ID
            select new ExternalPatientIdentifier {Context = m.ExternalFeed.service_Name, Number = m.RemotePatient_ID};

         return mrns.ToList();
      }

      public void Commit()
      {
         _context.ObjectContext.SaveChanges();
      }

      public MR_Container GetClinicalRecord(long? clinicalRecordId)
      {
         return _context.MR_Container.Find(clinicalRecordId);
      }

      public PatientDetails GetPatientDetailsFromConsumption(long containerId)
      {
         var patientDetails = from mr in _context.MR_Container
            join pcm in _context.PatientContainerMRNs on mr.container_ID equals pcm.container_ID into mrns
            from mrn in mrns.DefaultIfEmpty()
            where mr.container_ID == containerId
            select new PatientDetails
            {
               Title = mr.Patient.title,
               Surname = mr.Patient.surname,
               FirstName = mr.Patient.firstName,
               Medicare = mr.Patient.medicare,
               Dob = mr.Patient.dob,
               PatientId = mrn.RemotePatient_ID ?? "HT" + mr.patient_ID,
               ExternalPatientIds =
                  mr.Patient.HL7_PatientMapping.Select(
                     m =>
                        new ExternalPatientIdentifier
                        {
                           Context = m.ExternalFeed.service_Name,
                           Number = m.RemotePatient_ID
                        })
            };
         return patientDetails.First();
      }

      public string GetMrn(long containerId)
      {
         var patientContainer = _context.PatientContainerMRNs.SingleOrDefault(pcm => pcm.container_ID == containerId);
         return patientContainer == null ? null : patientContainer.RemotePatient_ID;
      }

      public int? FindPatientIdByMrn(string mrn)
      {
         var mapping = _context.PatientMappings.SingleOrDefault(pm => pm.RemotePatient_ID == mrn);
         return mapping != null ? mapping.LocalPatient_ID : null;
      }
   }

   public interface IMedicalRecordRepository
   {
      PatientDetails GetPatientFromContainer(long containerId);
      IList<ExternalPatientIdentifier> GetExternalPatientIdsFromContainer(long containerId);
      void Commit();
      MR_Container GetClinicalRecord(long? clinicalRecordId);
      PatientDetails GetPatientDetailsFromConsumption(long containerId);
      string GetMrn(long containerId);
      int? FindPatientIdByMrn(string mrn);
   }
}