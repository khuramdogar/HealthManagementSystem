using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Stock
{
   public class DeductStock : IHaveCustomMappings
   {
      [Display(Name = "Product ID", Description = "Product ID")]
      [Range(1, Int32.MaxValue, ErrorMessage = "Please select a product or enter an id")]
      [Required]
      public int? ProductId { get; set; }

      [Display(Name = "Serial number")]
      public string SerialNumber { get; set; }

      [Display(Name = "Lot/Batch number")]
      public string BatchNumber { get; set; }

      [Display(Name = "Used by")]
      public string UsedBy { get; set; }

      [Display(Name = "Used on")]
      [DataType(DataType.DateTime)]
      public DateTime UsedOn { get; set; }

      [Display(Name = "Use location"), Required(ErrorMessage = "Please specify a location")]
      public int StockLocationId { get; set; }

      [Display(Name = "Quantity")]
      [Required]
      public int? Quantity { get; set; }

      public bool OverrideLocation { get; set; }

      [Display(Name = "Reason")]
      public int? Reason { get; set; }

      [Display(Name = "General Ledger")]
      public int? LedgerId { get; set; }

      [Display(Name = "General ledger code")]
      public string GLC { get; set; }

      public string Note { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<DeductStock, ItemAdjustment>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.ReasonId, opt => opt.MapFrom(src => src.Reason));
      }
   }

   public class OrderOverview
   {
      public int OrderId { get; set; }
      public DateTime? DateCreated { get; set; }
      public string Name { get; set; }
   }
}