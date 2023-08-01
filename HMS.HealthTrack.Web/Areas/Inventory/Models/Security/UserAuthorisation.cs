using AutoMapper;
using HMS.HealthTrack.Web.Data.Model.Security;
using HMS.HealthTrack.Web.Infrastructure;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Security
{
   public class UserAuthorisation : IHaveCustomMappings
   {
      public string User_ID { get; set; }
      public string Keyword { get; set; }
      public string Description { get; set; }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<HealthTrackAuthorisation, UserAuthorisation>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.HealthTrackPermission == null ? string.Empty : src.HealthTrackPermission.Description));
      }
   }
}