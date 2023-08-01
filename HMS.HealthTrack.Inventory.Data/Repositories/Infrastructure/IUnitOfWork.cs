using System;
using System.Data.Entity.Infrastructure;

namespace HMS.HealthTrack.Web.Data.Repositories.Infrastructure
{
   public interface IUnitOfWork<TContext> : IDisposable where TContext : IObjectContextAdapter
   {
      TContext Context { get; }
      int Save();
   }
}