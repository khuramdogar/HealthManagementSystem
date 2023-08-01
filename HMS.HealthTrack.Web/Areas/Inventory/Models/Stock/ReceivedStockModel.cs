using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Linq;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Stock
{
   public class ReceivedStockModel : IHaveCustomMappings
   {
      public string ReceivedBy { get; set; }
      public DateTime ReceivedOn { get; set; }
      public int ReceivedQuantity { get; set; }
      public string Location { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<Data.Model.Inventory.StockAdjustment, ReceivedStockModel>()
            .ForMember(m => m.ReceivedBy, opt => opt.MapFrom(src => src.AdjustedBy))
            .ForMember(m => m.ReceivedOn, opt => opt.MapFrom(src => src.AdjustedOn))
            .ForMember(m => m.ReceivedQuantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(m => m.Location,
               opt => opt.MapFrom(src => src.StockAdjustmentStocks.First().Stock.StorageLocation.Name));
      }
   }
}