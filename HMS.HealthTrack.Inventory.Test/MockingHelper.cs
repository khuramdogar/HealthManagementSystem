using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Moq;

namespace HMS.HealthTrack.Inventory.Test
{
   internal class MockingHelper
   {
      /// <summary>
      /// Creates a mocked DB set for testing. You can use this to check things are getting saved correctly
      /// </summary>
      /// <typeparam name="T">The type of collection, e.g. InventoryProduct</typeparam>
      /// <param name="data">A list of preloaded data that represents the colleciton</param>
      /// <returns>A mocked db set</returns>
      internal static Mock<IDbSet<T>> GetMockedDbSet<T>(IQueryable<T> data) where T : class
      {
         //Setup entity collection
         var dbSetMock = new Mock<IDbSet<T>>();
         dbSetMock.Setup(m => m.Provider).Returns(data.Provider);
         dbSetMock.Setup(m => m.Expression).Returns(data.Expression);
         dbSetMock.Setup(m => m.ElementType).Returns(data.ElementType);
         dbSetMock.Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
         dbSetMock.Setup(m => m.Local).Returns(new ObservableCollection<T>());
         //Mock the data context
         return dbSetMock;
      }

      /// <summary>
      /// Get a mocked context complete with your collection for testing repositories
      /// </summary>
      /// <typeparam name="T">The type of your collection</typeparam>
      /// <typeparam name="T2">The type of your context</typeparam>
      /// <param name="mockedSet">A mock up of your collection</param>
      /// <param name="expression">A Lamda of where to find the collection in it's context</param>
      /// <returns>Mocked database context</returns>
      internal static Mock<T2> GetMockedContext<T, T2>(Mock<IDbSet<T>> mockedSet, Expression<Func<T2, IDbSet<T>>> expression)
         where T : class
         where T2 : DbContext
      {
         //Mock the data context
         var mockContext = new Mock<T2>();
         mockContext.Setup(expression).Returns(mockedSet.Object);
         return mockContext;

      }

      /// <summary>
      /// Get a mocked context complete with your collection for testing repositories
      /// </summary>
      /// <typeparam name="T">The type of your collection</typeparam>
      /// <typeparam name="T2">The type of your context</typeparam>
      /// <param name="data">A mock up of your collection</param>
      /// <param name="expression">A Lamda of where to find the collection in it's context</param>
      /// <returns>Mocked database context</returns>
      internal static Mock<T2> GetMockedContext<T, T2>(IEnumerable<T> data, Expression<Func<T2, IDbSet<T>>> expression)
         where T : class
         where T2 : DbContext
      {
         //Mock the data context
         return GetMockedContext(GetMockedDbSet(data.AsQueryable()), expression);
      }
   }
}
