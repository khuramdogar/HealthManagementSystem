using HMS.HealthTrack.Web.Data.Model.Security;

namespace HMS.HealthTrack.Web.Data.Repositories.Security
{
   public interface ISecurityUnitOfWork
   {
      IDbContextSecurity Context { get; }
      IUserRepository UserRepository { get; }
      void GrantUserPermission(HealthTrackAuthorisation authorisation);
      void Commit();
      void RemovePermission(HealthTrackAuthorisation authorisation);
   }
}