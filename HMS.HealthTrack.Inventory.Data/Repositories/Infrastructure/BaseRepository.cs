using System;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HMS.HealthTrack.Web.Data.Repositories.Infrastructure
{
   public class BaseRepository<T> : IEntityRepository<T>, IBaseRepository<T> where T : class
   {
      protected ObjectSet<T> ObjectSet;

      public BaseRepository(IObjectContextAdapter context)
      {
         if (context.ObjectContext != null)
            ObjectSet = context.ObjectContext.CreateObjectSet<T>();
      }

      public virtual Task<ObjectResult<T>> FindAllAync(MergeOption merge = MergeOption.AppendOnly)
      {
         return ObjectSet.ExecuteAsync(merge);
      }

      public virtual IQueryable<T> FindAll()
      {
         return ObjectSet;
      }

      public virtual IQueryable<T> Find(Expression<Func<T, bool>> predicate)
      {
         return ObjectSet.Where(predicate);
      }

      public virtual void Add(T newEntity)
      {
         ObjectSet.AddObject(newEntity);
      }

      public virtual void Remove(T entity)
      {
         ObjectSet.DeleteObject(entity);
      }
   }
}