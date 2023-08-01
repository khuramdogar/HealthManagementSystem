using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Orders
{
   [ModelMetaType(typeof(OrderMeta))]
   public class IndexOrdersViewModel : IHaveCustomMappings
   {
      public int InventoryOrderId { get; set; }
      public DateTime? DateCreated { get; set; }
      public string CreatedBy { get; set; }
      public string Status { get; set; }
      public int StatusNumber { get; set; }
      public bool PartialShipping { get; set; }
      public string Name { get; set; }
      public string Notes { get; set; }
      public int Items { get; set; }
      public DateTime? NeedBy { get; set; }
      [Display(Name = "Urgency")]
      public string IsUrgent { get; set; }

      public string DeliveryLocationId { get; set; }
      public string DeliveryLocation { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Order, IndexOrdersViewModel>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items.Count))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => HelperMethods.GetEnumDisplayName<OrderStatus>(src.Status)))
            .ForMember(dest => dest.StatusNumber, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IsUrgent, opt => opt.MapFrom(src => src.IsUrgent ? "Urgent" : "Non-Urgent"))
            .ForMember(dest => dest.DeliveryLocation, opt => opt.MapFrom(src => src.DeliveryLocation != null ? src.DeliveryLocation.Name : string.Empty));
      }
   }
}