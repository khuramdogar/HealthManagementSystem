using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes
{
   public class StockTakeViewModel : IHaveCustomMappings
   {
      public int StockTakeId { get; set; }
      [Display(Name = "Stock take date")]
      public DateTime? StockTakeDate { get; set; }
      public int ItemCount { get; set; }
      public int MissingStockLevel { get; set; }
      public string CreatedBy { get; set; }
      public StockTakeStatus Status { get; set; }
      public string Message { get; set; }
      public int LocationId { get; set; }
      [Display(Name = "Location")]
      public string LocationName { get; set; }
      public DateTime? SubmittedOn { get; set; }
      public string Name { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<StockTake, StockTakeViewModel>()
            .ForMember(dest => dest.StockTakeId, opt => opt.MapFrom(src => src.StockTakeId))
            .ForMember(dest => dest.ItemCount,
               opt => opt.MapFrom(src => src.StockTakeItems.Count(sti => !sti.DeletedOn.HasValue)))
            .ForMember(dest => dest.MissingStockLevel,
               opt => opt.MapFrom(src => src.StockTakeItems.Count(sti => !sti.StockLevel.HasValue)));
      }
   }
}
