using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Orders
{
   public class OrderItemsViewModel : IHaveCustomMappings
   {
      public IndexProductsViewModel Product { get; set; }
      public int OrderItemId { get; set; }
      public int InventoryOrderId { get; set; }
      public string ProductId { get; set; }
      public string ProductDescription { get; set; }
      public int Quantity { get; set; }

      [Required]
      public int? OutstandingQuantity { get; set; }

      public decimal? UnitPrice { get; set; }
      public bool IncludeRebateCode { get; set; }
      public OrderItemStatus Status { get; set; }
      public int? StatusNumber { get; set; }
      public int? ReceivedQuantity { get; set; }
      public string GLC { get; set; }
      public int CurrentStock { get; set; }
      public DateTime? ReceivedOn { get; set; }
      public bool PartiallyReceive { get; set; }
      public int? LedgerId { get; set; }
      public bool HasConsumptionDetails { get; set; }


      public string OrderName { get; set; }
      public bool Cancel { get; set; }
      public string ProductSPC { get; set; }

      public OrderItemAction Action
      {
         get
         {
            if (Status == OrderItemStatus.Ordered || Status == OrderItemStatus.PartiallyReceived)
            {
               return OrderItemAction.KeepOpen;
            }
            if (Status == OrderItemStatus.Cancelled)
            {
               return OrderItemAction.Cancel;
            }
            return OrderItemAction.Complete;
         }
      }


      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<OrderItem, OrderItemsViewModel>()
            .ForMember(dest => dest.StatusNumber,
               opt => opt.MapFrom(src => src.Status != null ? (int)src.Status : (int?)null))
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
            .ForMember(dest => dest.ReceivedQuantity, opt => opt.MapFrom(src => src.Status != OrderItemStatus.Complete ? src.StockAdjustments.Aggregate(0, (i, adjustment) => i + adjustment.Quantity) : (int?)null))
            .ForMember(dest => dest.CurrentStock,
               opt => opt.MapFrom(src => src.Product.Stocks.Count > 0 ? src.Product.Stocks.Sum(ois => ois.Quantity) : 0))
            .ForMember(dest => dest.LedgerId, opt => opt.MapFrom(src => src.Product.LedgerId))
            .ForMember(dest => dest.HasConsumptionDetails, opt => opt.MapFrom(src => src.ConsumptionNotificationManagements.Any()))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Status == OrderItemStatus.Invoiced ? 0 : src.Quantity));
      }
   }
}
