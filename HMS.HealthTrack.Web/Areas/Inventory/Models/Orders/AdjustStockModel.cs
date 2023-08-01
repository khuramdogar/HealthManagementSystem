using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Orders
{
   public class AdjustStockModel : IHaveCustomMappings
   {
      public int StockAdjustmentId { get; set; }
      public int OrderItemId { get; set; }
      public DateTime ReceivedOn { get; set; }
      public int Quantity { get; set; }
      public int ReceivedQuantity { get; set; }
      public string Adjustable { get; set; }
      public string Location { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Data.Model.Inventory.StockAdjustment, AdjustStockModel>()
            .ForMember(m => m.ReceivedOn, opt => opt.MapFrom(src => src.AdjustedOn))
            .ForMember(m => m.Quantity, opt => opt.MapFrom(src => src.StockAdjustmentStocks.Sum(sas => sas.Stock.Quantity)))
            .ForMember(m => m.ReceivedQuantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(m => m.Location, opt => opt.MapFrom(src => src.StockAdjustmentStocks.First().Stock.StorageLocation.Name))
            .ForMember(m => m.Adjustable, opt => opt.MapFrom(src => src.StockAdjustmentStocks.Any(s => s.Stock.StockStatus == StockStatus.Available)));
      }
   }
}