using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Security;
using HMS.HealthTrack.Web.Data.Repositories.Infrastructure;

namespace HMS.HealthTrack.Web.Data.Repositories.Security
{
   public class SecurityUnitOfWork : IUnitOfWork<IDbContextSecurity>, ISecurityUnitOfWork
   {
      public SecurityUnitOfWork(IDbContextSecurity context)
      {
         Context = context;
         UserRepository = new UserRepository(context);
      }

      public IUserRepository UserRepository { get; }

      public void GrantUserPermission(HealthTrackAuthorisation authorisation)
      {
         Context.HealthTrackAuthorisations.Add(authorisation);
      }

      public void Commit()
      {
         Save();
      }

      public void RemovePermission(HealthTrackAuthorisation authorisation)
      {
         var auth = from a in Context.HealthTrackAuthorisations where a.User_ID == authorisation.User_ID && a.Keyword == authorisation.Keyword select a;
         Context.HealthTrackAuthorisations.RemoveRange(auth);
      }

      public int Save()
      {
         return Context.ObjectContext.SaveChanges();
      }

      public IDbContextSecurity Context { get; }


      public void Dispose()
      {
      }
   }
}