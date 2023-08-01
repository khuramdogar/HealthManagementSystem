using System;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public class MappingOverview
   {
      public int MappingId { get; set; }

      [Display(Name = "Product ID")]
      public int InternalId { get; set; }

      [Display(Name = "Description")]
      public string InternalDescription { get; set; }

      [Display(Name = "External ID")]
      public int ExternalId { get; set; }

      [Display(Name = "Description")]
      public string ExternalDescription { get; set; }

      public string SPC { get; set; }
      public DateTime? CreatedOn { get; set; }
      public string CreatedBy { get; set; }
   }
}