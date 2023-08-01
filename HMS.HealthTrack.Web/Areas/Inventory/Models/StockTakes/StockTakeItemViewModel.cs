using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes
{
   public class StockTakeItemViewModel : IHaveCustomMappings
   {
      public int StockTakeId { get; set; }
      public int StockTakeItemId { get; set; }
      public string SPC { get; set; }
      public string Description { get; set; }
      public int? StockLevel { get; set; }
      [Required]
      public int ProductId { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<StockTakeItemViewModel, StockTakeItem>().ForMember(dest => dest.CreatedOn, opt => opt.UseValue(DateTime.MinValue));
         configuration.CreateMap<StockTakeItem, StockTakeItemViewModel>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product != null ? src.Product.Description : string.Empty))
            .ForMember(dest => dest.SPC, opt => opt.MapFrom(src => src.Product != null ? src.Product.SPC : string.Empty));
      }
   }
}