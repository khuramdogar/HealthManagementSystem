using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Stock
{
   [ModelMetaType(typeof(ProductMeta))]
   public class IndexStockViewModel : IHaveCustomMappings
   {
      public int ProductId { get; set; }
      public string Description { get; set; }
      public string SPC { get; set; }
      public string RebateCode { get; set; }
      public int? PrimarySupplier { get; set; }
      public string Manufacturer { get; set; }

      public string Status { get; set; }

      public bool InStock
      {
         get { return true; }
      }

      public bool Unclassified { get; set; }
      public bool HasHadStockTake { set; get; }
      public bool IsConsignment { get; set; }
      public bool PendingConsumedProducts { get; set; }
      public bool InError { get; set; }
      public bool ManageStock { get; set; }

      [Display(Name = "Items")]
      public int StockCount { get; set; }

      [Display(Name = "Total")]
      public int TotalStock { get; set; }

      [Display(Name = "Low")]
      public int? LowQuantityThreshold { get; set; }

      public int? ThresholdMargin { get; set; }

      [Display(Name = "Location")]
      public string StorageLocation { get; set; }

      public int LocationId { get; set; }
      public bool HasSerial { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<StockLevel, IndexStockViewModel>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Product.ProductId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product.Description))
            .ForMember(dest => dest.SPC, opt => opt.MapFrom(src => src.Product.SPC))
            .ForMember(dest => dest.RebateCode, opt => opt.MapFrom(src => src.Product.RebateCode))
            .ForMember(dest => dest.PrimarySupplier, opt => opt.MapFrom(src => src.Product.PrimarySupplier))
            .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Product.Manufacturer))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Product.ProductStatus))
            .ForMember(dest => dest.Unclassified, opt => opt.MapFrom(src => src.Product.Unclassified))
            .ForMember(dest => dest.HasHadStockTake, opt => opt.MapFrom(src => src.Product.HasHadStockTake))
            .ForMember(dest => dest.IsConsignment, opt => opt.MapFrom(src => src.Product.IsConsignment))
            .ForMember(dest => dest.InError, opt => opt.MapFrom(src => src.Product.InError))
            .ForMember(dest => dest.LowQuantityThreshold, opt => opt.MapFrom(src => src.Product.ReorderThreshold))
            .ForMember(dest => dest.StorageLocation,
               opt => opt.MapFrom(src => src.StorageLocation != null ? src.StorageLocation.Name : string.Empty))
            .ForMember(dest => dest.LocationId,
               opt => opt.MapFrom(src => src.StorageLocation != null ? src.StorageLocation.LocationId : (int?)null))
               .ForMember(dest => dest.ManageStock, opt => opt.MapFrom(src => src.Product.ManageStock));
      }
   }
}