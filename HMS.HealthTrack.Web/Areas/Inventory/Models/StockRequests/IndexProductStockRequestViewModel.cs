using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockRequests
{
   [ModelMetaType(typeof(StockRequestMeta))]
   public class IndexProductStockRequestViewModel : IHaveCustomMappings
   {
      public int StockRequestId { get; set; }
      public int ProductId { get; set; }
      public int RequestedQuantity { get; set; }
      public DateTime CreatedOn { get; set; }
      public string CreatedBy { get; set; }
      public int RequestStatus { get; set; }
      public string Status { get; set; }
      public string Description { get; set; }
      public int ApprovedQuantity { get; set; }
      public bool IsUrgent { get; set; }


      [Display(Name = "Supplier")]
      public string PrimarySupplier { get; set; }
      public int SecondarySupplier { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<ProductStockRequest, IndexProductStockRequestViewModel>()
            .ForMember(dest => dest.Description,
               opt => opt.MapFrom(src => src.Product.Description))
               .ForMember(dest => dest.PrimarySupplier, opt => opt.MapFrom(src => src.Product.PrimarySupplier != null ? src.Product.PrimarySupplierCompany.companyName : string.Empty))
            .ForMember(dest => dest.SecondarySupplier, opt => opt.MapFrom(src => src.Product.SecondarySupplier))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.GetName(typeof(RequestStatus), src.RequestStatus)));

      }
   }
}