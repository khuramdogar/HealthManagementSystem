using AutoMapper;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes
{
   public class IncomingStockTakeItem
   {
      public int? ProductId { get; set; }
      public string Description { get; set; }
      public string SPC { get; set; }
      public int? StockLevel { get; set; }
      public string Message { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<StockTakeItem, IncomingStockTakeItem>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(dest => dest.Product.Description))
            .ForMember(dest => dest.SPC, opt => opt.MapFrom(dest => dest.Product.SPC));

         configuration.CreateMap<IncomingStockTakeItem, StockTakeItem>();
      }
   }
}
