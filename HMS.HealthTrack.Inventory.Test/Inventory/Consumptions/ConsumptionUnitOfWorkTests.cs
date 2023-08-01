using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Consumptions
{
   [TestClass]
   public class ConsumptionUnitOfWorkTests
   {
      [TestMethod]
      public void FindOrCreateProductTest()
      {
         //arrange
         var mockedProducts = MockingHelper.GetMockedDbSet(new List<Product> { }.AsQueryable());
         var mockedDb = MockingHelper.GetMockedContext<Product, InventoryContext>(mockedProducts, context => context.Products);

         //sut
         var sut = new ConsumptionUnitOfWork(mockedDb.Object,MockedInventoryItems.Properties.Object,MockedInventoryItems.Logger.Object);

         //act
         sut.FindOrCreateProduct(new Consumption());
      }
   }
}
