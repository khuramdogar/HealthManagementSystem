using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Consumptions
{

   public class ConsumptionDetailsViewModel : IMapFrom<HealthTrackConsumptionUpdate>, IMapFrom<HealthTrackConsumption>
   {
      [Display(Name = "Consumption ID")]
      public int UsedId { get; set; }
      [Display(Name = "HealthTrack product ID")]
      public int ProductId { get; set; }
      public string SPC { get; set; }
      public string LPC { get; set; }
      public string ScanCode { get; set; }
      public string Name { get; set; }
      [Display(Name = "Serial number")]
      public string SerialNumber { get; set; }
      [Display(Name = "Lot/Batch number")]
      public string LotNumber { get; set; }
      [Required]
      public short? Quantity { get; set; }
      [Required, Range(1, int.MaxValue)]
      public int? Location { get; set; }
      public string LocationName { get; set; }
      [Display(Name = "MRN")]
      public string PatientMRN { get; set; }
      [Display(Name = "Clinical record ID")]
      public long? ContainerId { get; set; }
      public int? PatientId { get; set; }
      public string Manufacturer { get; set; }
      public string GL { get; set; }
      public string RebateCode { get; set; }
      public string Description { get; set; }
      public decimal? Price { get; set; }
      public string BuyCurrency { get; set; }
      public double? BuyCurrencyRate { get; set; }
      public bool? deleted { get; set; }
      public DateTime? deletionDate { get; set; }
      public string deletionUser { get; set; }
      [Display(Name = "Last modified on")]
      public DateTime? LastModifiedOn { get; set; }
      [Display(Name = "Last modified by")]
      public string LastModifiedBy { get; set; }
      [Display(Name = "Created on")]
      public DateTime? dateCreated { get; set; }
      [Display(Name = "Created by")]
      public string userCreated { get; set; }
      [Display(Name = "Processing status")]
      public ConsumptionProcessingStatus? ProcessingStatus { get; set; }
      [Display(Name = "Processing message")]
      public string ProcessingStatusMessage { get; set; }
      [Display(Name = "Consumed on")]
      public DateTime? ConsumedOn { get; set; }
      [Display(Name = "Inventory product")]
      public int? InventoryProductId { get; set; }
      public string InventoryProductDescription { get; set; }
   }
}