
using AutoMapper;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes
{
   public class StockTakeListItem : IHaveCustomMappings
   {
      public int StockTakeItemId { get; set; }
      public int StockTakeId { get; set; }
      public int ProductId { get; set; }
      public string Description { get; set; }
      public string SPC { get; set; }
      public int? StockLevel { get; set; }
      public StockTakeItemStatus Status { get; set; }
      public string Message { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<StockTakeItem, StockTakeListItem>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(dest => dest.Product.Description))
            .ForMember(dest => dest.SPC, opt => opt.MapFrom(dest => dest.Product.SPC));

         configuration.CreateMap<StockTakeListItem, StockTakeItem>();
      }
   }
}