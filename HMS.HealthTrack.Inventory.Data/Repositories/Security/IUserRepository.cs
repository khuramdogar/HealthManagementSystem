using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using HMS.HealthTrack.Web.Data.Model.Security;

namespace HMS.HealthTrack.Web.Data.Repositories.Security
{
   public interface IUserRepository
   {
      IQueryable<HealthTrackPermission> AllPermissions { get; }
      IQueryable<HealthTrackUser> AllUsers { get; }
      IQueryable<HealthTrackUser> UsersWithInventoryPermissions { get; }
      IQueryable<HealthTrackPermission> GetPermissionsForUser(IIdentity userIdentity);
      IQueryable<HealthTrackPermission> GetPermissionsForUser(string username, IList<string> permissionsToCheck = null);
      bool UserHasPermission(string username, string keyword);
      IQueryable<HealthTrackAuthorisation> GetAuthorisationsForUser(string username, List<string> permissionsToCheck = null);
   }
}