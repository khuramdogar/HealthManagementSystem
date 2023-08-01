using System;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HMS.HealthTrack.Web.Data.Repositories.Infrastructure
{
   public interface IBaseRepository<T> where T : class
   {
      IQueryable<T> FindAll();
      Task<ObjectResult<T>> FindAllAync(MergeOption mergeOption = MergeOption.AppendOnly);
      IQueryable<T> Find(Expression<Func<T, bool>> predicate);
      void Add(T newEntity);
      void Remove(T entity);
   }
}