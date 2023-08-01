using AutoMapper;
using HMS.HealthTrack.Web.Areas.Inventory.Models.Products;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Orders
{
   [ModelMetaType(typeof(OrderMeta))]
   public class DetailsOrdersViewModel : IHaveCustomMappings
   {
      public int InventoryOrderId { get; set; }
      public DateTime DateCreated { get; set; }
      public string CreatedBy { get; set; }
      public DateTime? LastModifiedOn { get; set; }
      public string LastModifiedBy { get; set; }
      public string Status { get; set; }
      public bool PartialShipping { get; set; }
      public string Name { get; set; }
      public string Notes { get; set; }
      [Display(Name = "Delivery location")]
      public string DeliveryLocation { get; set; }
      [DisplayFormat(DataFormatString = "{0:C}")]
      public decimal Total
      {
         get { return Items.Aggregate(0m, (current, item) => current + item.LineTotal); }
      }
      [Display(Name = "Required by")]
      [DataType(DataType.Date)]
      public DateTime? NeedBy { get; set; }
      public bool IsUrgent { get; set; }
      [Display(Name = "Charge account")]
      public string ChargeAccount { get; set; }
      [Display(Name = "Ledger code")]
      public string LedgerCode { get; set; }

      public IEnumerable<DetailsOrderItemViewModel> Items { get; set; }

      public Dictionary<byte, string> OrderItemStatuses
      {
         get
         {
            return Enum.GetValues(typeof(OrderItemStatus)).Cast<object>().ToDictionary(e => (byte)e, e => HelperMethods.GetEnumDisplayName<OrderItemStatus>((OrderItemStatus)e));
         }
      }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Order, DetailsOrdersViewModel>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(xx => xx.Items))
            .ForMember(dest => dest.DeliveryLocation, opt => opt.MapFrom(xx => xx.DeliveryLocation != null ? xx.DeliveryLocation.Name : string.Empty))
            .ForMember(dest => dest.ChargeAccount, opt => opt.MapFrom(xx => xx.LedgerId.HasValue ? xx.GeneralLedger.Name : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => HelperMethods.GetEnumDisplayName<OrderStatus>(src.Status)));
      }
   }

   [ModelMetaType(typeof(OrderItemMeta))]
   public class DetailsOrderItemViewModel : IHaveCustomMappings
   {
      public int OrderItemId { get; set; }
      public int InventoryOrderId { get; set; }
      public int ProductId { get; set; }
      [Display(Name = "Description")]
      public string ProductDescription { get; set; }
      public int Quantity { get; set; }
      public decimal UnitPrice { get; set; }
      public int PriceModelId { get; set; }
      public OrderProductsViewModel Product { get; set; }

      [Display(Name = "Total")]
      public decimal LineTotal
      {
         get { return UnitPrice * Quantity; }
      }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<OrderItem, DetailsOrderItemViewModel>()
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
            .ForMember(dest => dest.UnitPrice,
               opt => opt.MapFrom(src => src.UnitPrice.HasValue ? Math.Round(src.UnitPrice.Value, 2) : 0))
            .ForMember(dest => dest.PriceModelId, opt => opt.MapFrom(src => src.Product.PriceModelId));
      }
   }
}