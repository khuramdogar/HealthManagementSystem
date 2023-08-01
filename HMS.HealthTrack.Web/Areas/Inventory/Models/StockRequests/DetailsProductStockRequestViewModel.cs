using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockRequests
{
   [ModelMetaType(typeof(StockRequestMeta))]
   public class DetailsProductStockRequestViewModel : IHaveCustomMappings
   {
      public int StockRequestId { get; set; }
      public int ProductId { get; set; }
      [Display(Name = "Description")]
      public string ProductDescription { get; set; }
      public int RequestedQuantity { get; set; }
      public DateTime CreatedOn { get; set; }
      public string CreatedBy { get; set; }
      public string RequestStatus { get; set; }
      public bool Fulfilled { get; set; }
      public int? ConsumptionId { get; set; }
      public string Location { get; set; }
      public bool IsUrgent { get; set; }
      [Display(Name = "Order")]
      public int? OrderId { get; set; }
      public string OrderName { get; set; }
      public DateTime? LastModifiedOn { get; set; }
      public string LastModifiedBy { get; set; }
      public int ApprovedQuantity { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<ProductStockRequest, DetailsProductStockRequestViewModel>()
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location == null ? string.Empty : src.Location.Name))
            .ForMember(dest => dest.RequestStatus, opt => opt.MapFrom(src => Enum.GetName(typeof(RequestStatus), src.RequestStatus)))
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderItemSources.FirstOrDefault() != null ? src.OrderItemSources.First().OrderItem.InventoryOrderId : (int?)null))
            .ForMember(dest => dest.OrderName, opt => opt.MapFrom(src => src.OrderItemSources.FirstOrDefault() != null ? src.OrderItemSources.First().OrderItem.Order.Name : string.Empty));
      }
   }
}