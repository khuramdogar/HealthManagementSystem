using System.Collections.Generic;
using HMS.HealthTrack.Web.Data.Model.Security;

namespace HMS.HealthTrack.Web.Data.Repositories.Security
{
   public class UserPermissions
   {
      public HealthTrackUser HealthTrackUser { get; set; }
      public IEnumerable<HealthTrackPermission> Permissions { get; set; }
   }
}