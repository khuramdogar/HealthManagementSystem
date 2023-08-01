using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Data.Model.HealthTrackInventory;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Consumptions
{
   [TestClass]
   public class TestConsumptionUnitOfWorkFindMatchingProduct
   {
      [TestMethod]
      public void No_Products()
      {
         var inventoryContextMock = new Mock<IDbContextInventoryContext>();
         var propertiesMock = new Mock<IPropertyProvider>();
         var loggerMock = new Mock<ICustomLogger>();

         var consumption = new Inventory_Used {Inventory_Master = new Inventory_Master()};
         var products = new List<Product>();
         var mockedProducts = MockingHelper.GetMockedDbSet(products.AsQueryable());

         inventoryContextMock.Setup(c => c.Products).Returns(mockedProducts.Object);

         var sut = new ConsumptionUnitOfWork(inventoryContextMock.Object,propertiesMock.Object,loggerMock.Object);

         //act
         var product = sut.FindOrCreateProduct(consumption);

         Assert.IsNull(product);
      }
      [TestMethod]
      public void Product_MultipleScanCodes()
      {
         var inventoryContextMock = new Mock<IDbContextInventoryContext>();
         var propertiesMock = new Mock<IPropertyProvider>();
         var loggerMock = new Mock<ICustomLogger>();

         var consumption = new Inventory_Used { Inventory_Master = new Inventory_Master {Inv_UPN = "UPN2"} };
         var products = new List<Product> {new Product
         {
            ScanCodes = new List<ScanCode>
            {
               new ScanCode {Value = "UPN1"},
               new ScanCode {Value = "UPN2"}
            }
         }};

         var mockedProducts = MockingHelper.GetMockedDbSet(products.AsQueryable());

         inventoryContextMock.Setup(c => c.Products).Returns(mockedProducts.Object);

         var sut = new ConsumptionUnitOfWork(inventoryContextMock.Object, propertiesMock.Object, loggerMock.Object);

         //act
         var product = sut.FindOrCreateProduct(consumption);

         Assert.IsNotNull(product);
         Assert.AreEqual(2,product.ScanCodes.Count);
         Assert.IsTrue(product.ScanCodes.Any(p=>p.Value == "UPN1"));
         Assert.IsTrue(product.ScanCodes.Any(p=>p.Value == "UPN2"));
      }
   }
}
