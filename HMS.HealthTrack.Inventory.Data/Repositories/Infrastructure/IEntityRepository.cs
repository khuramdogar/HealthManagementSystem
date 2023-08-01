using System;
using System.Linq;
using System.Linq.Expressions;

namespace HMS.HealthTrack.Web.Data.Repositories.Infrastructure
{
   public interface IEntityRepository<T> where T : class
   {
      IQueryable<T> FindAll();
      IQueryable<T> Find(Expression<Func<T, bool>> predicate);
      void Add(T entity);
      void Remove(T entity);
   }
}