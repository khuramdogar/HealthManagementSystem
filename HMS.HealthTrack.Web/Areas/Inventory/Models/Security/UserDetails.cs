using AutoMapper;
using HMS.HealthTrack.Web.Infrastructure;
using System;
using System.Web;
using HMS.HealthTrack.Web.Data.Model.Security;

namespace HMS.HealthTrack.Web.Areas.Inventory.Models.Security
{
   public class UserDetails : HealthTrackUser, IHaveCustomMappings
   {
      public UserDetails()
      {
         UserId = Guid.NewGuid().ToString();
      }

      /// <summary>
      /// A Unique id that contains only web safe characters
      /// </summary>
      public string UserId { get; set; }

      /// <summary>
      /// HealthTrack User_ID encoded to make illegal charaters safe for URLs (like \ or @)
      /// </summary>
      public string EncodedUserId
      {
         get { return HttpContext.Current.Server.UrlEncode(User_ID); }
      }

      public void CreateMappings(IConfiguration configuration)
      {
         configuration.CreateMap<HealthTrackUser, UserDetails>();
      }
   }
}
