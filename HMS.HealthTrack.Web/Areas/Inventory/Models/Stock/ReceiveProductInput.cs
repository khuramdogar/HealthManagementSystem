using System;
using System.ComponentModel.DataAnnotations;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Stock
{
   public class ReceiveProductInput
   {
      [Display(Name = "Product")]
      [Required(ErrorMessage = "You must select a product")]
      public int? ProductId { get; set; }

      [Display(Name = "Serial number")]
      public string SerialNumber { get; set; }

      [Display(Name = "Location", Description = "The ID of the items location")]
      [Required(ErrorMessage = "You  must select a location")]
      public string SelectedLocation { get; set; }

      [Required]
      public int? Quantity { get; set; }

      [Display(Name = "Lot/Batch number", Description = "The unique manufacturing batch of the item")]
      public string BatchNumber { get; set; }

      [Display(Name = "Expiry date", Description = "The date the item should not be used past")]
      [DataType(DataType.Date)]
      public DateTime? ExpiresOn { get; set; }

      public bool ContinueReceipt { get; set; }
   }
}