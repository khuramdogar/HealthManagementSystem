using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using HMS.HealthTrack.Web.Utils;
using System.ComponentModel.DataAnnotations;
using HMS.HealthTrack.Web.Data.Model.Inventory;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models
{
   [ModelMetaType(typeof(UserPreferenceMeta))]
   public class InventoryPreferenceModel : IHaveCustomMappings
   {
      [Display(Name = "User")]
      public string UserId { get; set; }
      [Required]
      public int? LocationId { get; set; }
      public string Location { get; set; }
      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<UserPreference, InventoryPreferenceModel>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User_ID))
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.LocationId.HasValue ? src.Location.Name : string.Empty));

         // create reverse map
         configuration.CreateMap<InventoryPreferenceModel, UserPreference>()
            .ForMember(dest => dest.User_ID, opt => opt.MapFrom(src => src.UserId));
      }
   }
}