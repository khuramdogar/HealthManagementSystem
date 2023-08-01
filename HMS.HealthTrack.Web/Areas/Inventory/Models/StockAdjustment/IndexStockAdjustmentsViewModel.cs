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
   public class IndexStockAdjustmentsViewModel : IHaveCustomMappings
   {
      public int StockAdjustmentId { get; set; }

      public string SPC { get; set; }

      [Display(Name = "Serial")]
      public string InventoryStockSerialNumber { get; set; }

      [Display(Name = "Product")]
      public string ProductName { get; set; }

      [Display(Name = "PID", Description = "Product ID")]
      public string ProductId { get; set; }

      [Display(Name = "Patient")]
      public string PatientId { get; set; }

      [Display(Name = "Batch")]
      public string InventoryStockBatchNumber { get; set; }

      [Display(Name = "Adjusted on")]
      public DateTime AdjustedOn { get; set; }

      [Display(Name = "Adjusted by")]
      public string AdjustedBy { get; set; }

      [Display(Name = "Source")]
      public AdjustmentSource Source { get; set; }

      [Display(Name = "Deducted")]
      public int Quantity { get; set; }

      [Display(Name = "Clinical record")]
      public long? ClinicalRecordId { get; set; }

      public string Status { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Data.Model.Inventory.StockAdjustment, IndexStockAdjustmentsViewModel>()
            .ForMember(m => m.SPC, opt => opt.MapFrom(source => source.StockAdjustmentStocks.First().Stock.Product.SPC))
            .ForMember(m => m.InventoryStockSerialNumber, opt => opt.MapFrom(source => source.StockAdjustmentStocks.First().Stock.SerialNumber))
            .ForMember(m => m.InventoryStockBatchNumber, opt => opt.MapFrom(source => source.StockAdjustmentStocks.First().Stock.BatchNumber))
            .ForMember(m => m.ProductName, opt => opt.MapFrom(source => source.StockAdjustmentStocks.First().Stock.Product.Description))
            .ForMember(m => m.ProductId, opt => opt.MapFrom(source => source.StockAdjustmentStocks.First().Stock.ProductId));
      }
   }
}