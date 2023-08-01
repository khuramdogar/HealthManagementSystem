using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Areas.Inventory.Models.ExternalApi.Products;
using HMS.HealthTrack.Web.Areas.Inventory.QueryHandlers.Products;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Products
{
   [TestClass]
   public class ProductSearchQueryHandlerTests
   {
      private static Mock<ICustomLogger> Logger => new Mock<ICustomLogger>();

      [TestMethod]
      public void NoProducts()
      {
         //arrange
         var productMocks = MockingHelper.GetMockedDbSet<Product>(new List<Product>().AsQueryable());
         var mockedContext = MockingHelper.GetMockedContext<Product, InventoryContext>(productMocks, context => context.Products);

         var unitOfWork = new Mock<IProductUnitOfWork>();

         //sut
         var queryHandler = new ProductSearchQueryHandler(Logger.Object, unitOfWork.Object, mockedContext.Object);

         //act
         var result = queryHandler.Handle(new ProductSearchQuery(), new CancellationToken());

         //Verify
         Assert.AreEqual(0, result.Result.Data.Count());
      }
   }
}