using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Data.Model.Clinical
{
   public class PatientDetails
   {
      [Display(Name = "Surname")]
      public string Surname { get; set; }

      [Display(Name = "First name")]
      public string FirstName { get; set; }

      [Display(Name = "Medicare number")]
      public string Medicare { get; set; }

      [Display(Name = "Date of birth", ShortName = "DOB")]
      public DateTime? Dob { get; set; }

      [Display(Name = "Patient ID")]
      public string PatientId { get; set; }

      public string Title { get; set; }

      [Display(Name = "Patient Identifiers")]
      public IEnumerable<ExternalPatientIdentifier> ExternalPatientIds { get; set; }

      [Display(Name = "Payment class")]
      public string PaymentClass { get; set; }
   }
}