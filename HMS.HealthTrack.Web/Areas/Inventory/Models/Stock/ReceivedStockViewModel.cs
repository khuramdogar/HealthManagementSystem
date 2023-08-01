using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Linq;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Stock
{
   public class ReceivedStockViewModel : IHaveCustomMappings
   {
      public int StockAdjustmentId { get; set; }
      public int ProductId { get; set; }
      public string SPC { get; set; }
      public string Description { get; set; }
      public int Quantity { get; set; }
      public string Location { get; set; }
      public string SerialNumber { get; set; }
      public string BatchNumber { get; set; }
      public DateTime ReceivedOn { get; set; }
      public string ReceivedBy { get; set; }
      public string OrderName { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Data.Model.Inventory.StockAdjustment, ReceivedStockViewModel>()
            .ForMember(m => m.ProductId, opt => opt.MapFrom(src => src.StockAdjustmentStocks.First().Stock.Product.ProductId))
            .ForMember(m => m.SPC, opt => opt.MapFrom(src => src.StockAdjustmentStocks.First().Stock.Product.SPC))
            .ForMember(m => m.Description, opt => opt.MapFrom(src => src.StockAdjustmentStocks.First().Stock.Product.Description))
            .ForMember(m => m.Location, opt => opt.MapFrom(src => src.StockAdjustmentStocks.First().Stock.StorageLocation.Name))
            .ForMember(m => m.SerialNumber, opt => opt.MapFrom(src => src.StockAdjustmentStocks.First().Stock.SerialNumber))
            .ForMember(m => m.BatchNumber, opt => opt.MapFrom(src => src.StockAdjustmentStocks.First().Stock.BatchNumber))
            .ForMember(m => m.ReceivedBy, opt => opt.MapFrom(src => src.AdjustedBy))
            .ForMember(m => m.ReceivedOn, opt => opt.MapFrom(src => src.AdjustedOn))
            .ForMember(m => m.OrderName, opt => opt.MapFrom(src => src.OrderItem != null ? src.OrderItem.Order.Name : string.Empty));
      }
   }
}