using System.Data.Entity;

namespace HMS.HealthTrack.Web.Data.Repositories.Infrastructure
{
   public class UnitOfWork : IUnitOfWork<DbContext>
   {
      public UnitOfWork(DbContext context)
      {
         Context = context;
      }

      public int Save()
      {
         return Context.SaveChanges();
      }

      public DbContext Context { get; }

      public void Dispose()
      {
         Context.Dispose();
      }
   }
}