using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Products
{
   public class DetailsMedicareProductsViewModel : IHaveCustomMappings
   {
      [Display(Name = "Product name")]
      public string MedicareProductName { get; set; }

      [Display(Name = "Code")]
      public string MedicareCode { get; set; }

      [Display(Name = "Description")]
      public string MedicareDescription { get; set; }

      [Display(Name = "Size")]
      public string MedicareSize { get; set; }

      [Display(Name = "Minimum benefit")]
      public string MedicareMinBenefit { get; set; }

      [Display(Name = "Maximum benefit")]
      public string MedicareMaxBenefit { get; set; }

      [Display(Name = "Group")]
      public string MedicareGroup { get; set; }

      [Display(Name = "Sub group")]
      public string MedicareSubGroup { get; set; }

      [Display(Name = "Supplier")]
      public string MedicareSupplier { get; set; }

      [Display(Name = "Manufacturer")]
      public string MedicareManufacturer { get; set; }

      [Display(Name = "Suffix")]
      public string MedicareSuffix { get; set; }

      [Display(Name = "Note")]
      public string MedicareNote { get; set; }


      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<MedicareProduct, DetailsMedicareProductsViewModel>()
            .ForMember(dest => dest.MedicareProductName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.MedicareCode, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.MedicareDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.MedicareSize, opt => opt.MapFrom(src => src.Size))
            .ForMember(dest => dest.MedicareMinBenefit, opt => opt.MapFrom(src => src.MinBenefit))
            .ForMember(dest => dest.MedicareMaxBenefit, opt => opt.MapFrom(src => src.MaxBenefit))
            .ForMember(dest => dest.MedicareGroup,
            opt => opt.MapFrom(src => src.ProductGroup.HasValue ? src.MedicareGroup.Name : string.Empty))
            .ForMember(dest => dest.MedicareSubGroup,
            opt => opt.MapFrom(src => src.ProductSubGroup.HasValue ? src.MedicareSubGroup.Name : string.Empty))
            .ForMember(dest => dest.MedicareSupplier,
            opt => opt.MapFrom(src => src.MedicareProductSponsor != null && src.MedicareProductSponsor.Supplier != null ? src.MedicareProductSponsor.Supplier.Company.companyName : string.Empty))
            .ForMember(dest => dest.MedicareManufacturer,
               opt =>
                  opt.MapFrom(src => src.MedicareProductSponsor != null && src.MedicareProductSponsor.Manufacturer != null ? src.MedicareProductSponsor.Manufacturer.Company.companyName : string.Empty))
            .ForMember(dest => dest.MedicareSuffix, opt => opt.MapFrom(src => src.Suffix))
            .ForMember(dest => dest.MedicareNote, opt => opt.MapFrom(src => src.Note));
      }
   }
}