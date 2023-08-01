using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.PaymentClassMappings
{
   public class PaymentClassMappingModel : IMapFrom<PaymentClassMapping>
   {
      [Required, Display(Name = "Payment Class")]
      public string PaymentClass { get; set; }
      [UIHint("PriceType"), Display(Name = "Price Category")]
      public string PriceTypeId { get; set; }
      [Display(Name = "Price Category")]
      public string PriceTypeName { get; set; }
   }
}