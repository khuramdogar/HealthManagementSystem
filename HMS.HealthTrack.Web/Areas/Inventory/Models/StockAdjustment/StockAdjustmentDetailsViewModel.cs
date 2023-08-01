using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockAdjustment
{
   [ModelMetaType(typeof(StockAdjustmentMeta))]
   public class StockAdjustmentDetailsViewModel : IHaveCustomMappings
   {
      [Display(Name = "Serial number")]
      public string InventoryStockSerialNumber { get; set; }

      [Display(Name = "Product")]
      public string ProductName { get; set; }

      [Display(Name = "PID", Description = "Product ID")]
      public string ProductId { get; set; }

      [Display(Name = "Patient")]
      public string PatientId { get; set; }

      [Display(Name = "Lot/Batch number")]
      public string InventoryStockBatchNumber { get; set; }

      [Display(Name = "Adjusted on")]
      public DateTime AdjustedOn { get; set; }

      [Display(Name = "Adjusted by")]
      public string AdjustedBy { get; set; }

      [Display(Name = "Source")]
      public AdjustmentSource Source { get; set; }

      [Display(Name = "Quantity")]
      public int Quantity { get; set; }

      [Display(Name = "Clinical record")]
      public long? ClinicalRecordId { get; set; }

      public int StockAdjustmentId { get; set; }
      public int StockId { get; set; }
      public string CreatedBy { get; set; }
      public DateTime CreatedOn { get; set; }
      public DateTime? DeletedOn { get; set; }
      public string DeletedBy { get; set; }
      public string DeletionReason { get; set; }
      public string LastModifiedBy { get; set; }
      public DateTime? LastModifiedOn { get; set; }
      public string Status { get; set; }
      public string Location { get; set; }
      public string Reason { get; set; }
      [Display(Name = "General ledger code")]
      public string GeneralLedgerCode { get; set; }
      public string Note { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Data.Model.Inventory.StockAdjustment, StockAdjustmentDetailsViewModel>()
            .ForMember(m => m.InventoryStockSerialNumber, opt => opt.MapFrom(source => source.StockAdjustmentStocks.First().Stock.SerialNumber))
            .ForMember(m => m.InventoryStockBatchNumber, opt => opt.MapFrom(source => source.StockAdjustmentStocks.First().Stock.BatchNumber))
            .ForMember(m => m.Location, opt => opt.MapFrom(source => source.StockAdjustmentStocks.First().Stock.StorageLocation.Name))
            .ForMember(m => m.ProductName, opt => opt.MapFrom(source => source.StockAdjustmentStocks.First().Stock.Product.Description))
            .ForMember(m => m.ProductId, opt => opt.MapFrom(source => source.StockAdjustmentStocks.First().Stock.ProductId))
            .ForMember(m => m.Reason, opt => opt.MapFrom(source => source.StockAdjustmentReason == null ? string.Empty : source.StockAdjustmentReason.Name))
            .ForMember(m => m.GeneralLedgerCode, opt => opt.MapFrom(source => source.GeneralLedgerId.HasValue ? source.GeneralLedger.Code : string.Empty));
      }
   }
}