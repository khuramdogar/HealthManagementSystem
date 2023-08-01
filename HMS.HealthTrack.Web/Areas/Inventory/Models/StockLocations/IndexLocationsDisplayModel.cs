using AutoMapper;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.StockLocations
{
   [ModelMetaType(typeof(StockLocationMeta))]
   public class IndexLocationsDisplayModel : IHaveCustomMappings
   {
      public int LocationId { get; set; }
      public string Name { get; set; }
      public bool IsDeleted { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<StockLocation, IndexLocationsDisplayModel>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.DeletedOn.HasValue));
      }
   }
}