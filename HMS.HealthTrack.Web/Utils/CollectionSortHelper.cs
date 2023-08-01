using System;
using System.Linq;
using System.Linq.Expressions;
using DataTables.Mvc;

namespace HMS.HealthTrack.Web.Utils
{
   /// <summary>
   /// CollectionSortHelper taken from the following:
   /// https://github.com/ALMMa/datatables.mvc/issues/7
   /// </summary>
   public static class CollectionSortHelper
   {
      public static IOrderedQueryable<TSource> CustomSort<TSource, TKey>(this IQueryable<TSource> items,
         Column.OrderDirection direction, Expression<Func<TSource, TKey>> keySelector)
      {
         if (direction == Column.OrderDirection.Ascendant)
         {
            return items.OrderBy(keySelector);
         }
         return items.OrderByDescending(keySelector);
      }

      public static IOrderedQueryable<TSource> CustomSort<TSource, TKey>(this IOrderedQueryable<TSource> items, Column.OrderDirection direction, Expression<Func<TSource, TKey>> keySelector)
      {
         if (direction == Column.OrderDirection.Ascendant)
            return items.ThenBy(keySelector);

         return items.ThenByDescending(keySelector);
      }
   }
}