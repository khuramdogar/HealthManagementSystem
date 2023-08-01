using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockSets
{
   public class StockSetModel : IHaveCustomMappings
   {
      public int StockSetId { get; set; }
      public string Name { get; set; }
      public int Count { get; set; }
      public double TotalCost { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<StockSet, StockSetModel>()
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Items.Count()))
            .ForMember(dest => dest.TotalCost,
               opt => opt.MapFrom(src => src.Items.Sum(si => si.Quantity * si.Product.BuyPrice)));
      }
   }
}