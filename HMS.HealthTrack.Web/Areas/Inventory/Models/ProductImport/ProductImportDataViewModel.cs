using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.ProductImport
{
   public class ProductImportDataViewModel : IHaveCustomMappings
   {
      public int ProductImportDataId { get; set; }
      public string Name { get; set; }
      public int Imported { get; set; }
      [Display(Name = "Imported on")]
      public DateTime? ImportedOn { get; set; }
      public string Status { get; set; }
      public string Count { get; set; }
      [Display(Name = "Uploaded on")]
      public DateTime UploadedOn { get; set; }
      public string Message { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<ProductImportData, ProductImportDataViewModel>()
            .ForMember(vm => vm.Count,
               opt => opt.MapFrom(src => src.ProductImports.Count == 0 ? "-" : src.ProductImports.Count.ToString()))
            .ForMember(vm => vm.Imported, opt => opt.MapFrom(src => src.ProductImports.Count(xx => xx.Processed)))
            .ForMember(vm => vm.UploadedOn, opt => opt.MapFrom(src => src.CreatedOn));
      }
   }
}