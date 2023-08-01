using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Model.Security;
using HMS.HealthTrack.Web.Data.Repositories.Infrastructure;

namespace HMS.HealthTrack.Web.Data.Repositories.Security
{
   public class UserRepository : BaseRepository<HealthTrackUser>, IUserRepository
   {
      private readonly IDbContextSecurity _context;

      public UserRepository(IDbContextSecurity context)
         : base(context)
      {
         _context = context;
      }

      public IQueryable<HealthTrackPermission> GetPermissionsForUser(IIdentity userIdentity)
      {
         return GetPermissionsForUser(userIdentity.Name);
      }

      public IQueryable<HealthTrackPermission> GetPermissionsForUser(string username, IList<string> permissionsToCheck = null)
      {
         var permissions = new Dictionary<string, HealthTrackPermission>();
         if (string.IsNullOrEmpty(username)) return null;


         var allAuthorisations = GetAuthorisations(username);

         //Add permissions
         foreach (var authorisation in allAuthorisations)
         {
            if (permissions.ContainsKey(authorisation.Keyword)) continue;
            if (permissionsToCheck != null && !permissionsToCheck.Contains(authorisation.Keyword)) continue;

            permissions.Add(authorisation.Keyword, authorisation.HealthTrackPermission);
         }

         //return the permissions' names
         return (from p in permissions select p.Value).AsQueryable();
      }

      public IQueryable<HealthTrackPermission> AllPermissions => _context.HealthTrackPermissions;

      public IQueryable<HealthTrackUser> AllUsers => _context.HealthTrackUsers;

      public IQueryable<HealthTrackUser> UsersWithInventoryPermissions
      {
         get
         {
            var inventoryPermissions = InventoryPermissions.All();
            return from u in AllUsers
               join a in _context.HealthTrackAuthorisations on u.User_ID equals a.User_ID
               where inventoryPermissions.Contains(a.Keyword)
               select u;
         }
      }

      public bool UserHasPermission(string username, string permission)
      {
         return GetPermissionsForUser(username).Any(p => p.Keyword == permission);
      }

      public IQueryable<HealthTrackAuthorisation> GetAuthorisationsForUser(string username, List<string> permissionsToCheck = null)
      {
         var permissions = new Dictionary<string, HealthTrackAuthorisation>();
         if (string.IsNullOrEmpty(username)) return null;


         var allAuthorisations = GetAuthorisations(username);

         //Add permissions
         foreach (var permission in allAuthorisations)
         {
            if (permissions.ContainsKey(permission.Keyword)) continue;
            if (permissionsToCheck != null && !permissionsToCheck.Contains(permission.Keyword)) continue;

            permissions.Add(permission.Keyword, permission);
         }

         //return the permissions' names
         return (from p in permissions select p.Value).AsQueryable();
      }

      public IQueryable<HealthTrackPermission> GetInventoryPermissionsForUser(string username)
      {
         return GetPermissionsForUser(username, InventoryPermissions.All());
      }

      private IQueryable<HealthTrackAuthorisation> GetAuthorisations(string username)
      {
         //Get all the groups and groups' groups for this user
         var groups = GroupUtils.GetGroupsForUser(username, _context.HealthTrackGroups);

         //Get permissions specifically granted to this user
         var userAuthorisations = (from a in _context.HealthTrackAuthorisations
            where a.User_ID.ToLower() == username.ToLower()
            select a).Include(a => a.HealthTrackPermission);

         //Get the permissions for all their groups
         if (groups == null) return userAuthorisations;

         var groupAuthorisations = (from a in _context.HealthTrackAuthorisations
            where groups.Contains(a.User_ID.ToLower())
            select a).Include(a => a.HealthTrackPermission);

         return userAuthorisations.Union(groupAuthorisations);
      }
   }
}