using System;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public partial class SupplierModel
   {
      public int? company_ID { get; set; }
      public string CreatedBy { get; set; }
      public DateTime CreatedOn { get; set; }
      public DateTime? LastModifiedOn { get; set; }
      public string LastModifiedBy { get; set; }
      public string Name { get; set; }
      public string Address1 { get; set; }
      public string Address2 { get; set; }
      public string Suburb { get; set; }
      public string State { get; set; }
      public string PostCode { get; set; }
      public string Country { get; set; }
      public string Department { get; set; }
      public string PhoneNumber { get; set; }
      public string FaxNumber { get; set; }
      public string Email { get; set; }
      public string WebSite { get; set; }
      public string ContactTitle { get; set; }
      public string ContactFirstname { get; set; }
      public string ContactSurname { get; set; }
   }
}