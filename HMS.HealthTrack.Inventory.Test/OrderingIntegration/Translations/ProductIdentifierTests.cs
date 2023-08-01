using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.OrderingIntegration.Translations
{
   /// <summary>
   /// The order of preference for product identifier should be, OrderChannel, SPC, UPN, None
   /// </summary>
   [TestClass]
   public class ProductIdentifierTests
   {


      [TestMethod]
      public void TestNone()
      {
         //test data set
         var testProduct = new Product { ProductId = 1 };

         //setup
         var dataContext = new Mock<IDbContextInventoryContext>();
         var propProvider = new Mock<IPropertyProvider>();
         var productsDataSet = MockingHelper.GetMockedDbSet(new List<Product> { testProduct }.AsQueryable());

         dataContext.Setup(c => c.Products).Returns(productsDataSet.Object);

         var sut = new ProductRepository(dataContext.Object, propProvider.Object);

         var result = sut.GetProductOrderReferenceIdentifier(1);

         Assert.AreEqual(ProductIdentifierType.None, result.IdentifierType);
      }

      [TestMethod]
      public void TestSPC_Only()
      {
         //test data set
         var testProduct = new Product {ProductId = 1,SPC = "Test SPC"};

         //setup
         var dataContext = new Mock<IDbContextInventoryContext>();
         var propProvider = new Mock<IPropertyProvider>();
         var productsDataSet = MockingHelper.GetMockedDbSet(new List<Product> {testProduct}.AsQueryable());

         dataContext.Setup(c => c.Products).Returns(productsDataSet.Object);

         var sut = new ProductRepository(dataContext.Object,propProvider.Object);

         var result = sut.GetProductOrderReferenceIdentifier(1);

         Assert.AreEqual("Test SPC", result.Value);
         Assert.AreEqual(ProductIdentifierType.SPC, result.IdentifierType);
      }

      [TestMethod]
      public void TestUPN_Only()
      {
         //test data set
         var testProduct = new Product() { ProductId = 1,ScanCodes = new List<ScanCode> { new ScanCode {Value = "078945648"}} };

         //setup
         var dataContext = new Mock<IDbContextInventoryContext>();
         var propProvider = new Mock<IPropertyProvider>();
         var productsDataSet = MockingHelper.GetMockedDbSet(new List<Product> { testProduct }.AsQueryable());

         dataContext.Setup(c => c.Products).Returns(productsDataSet.Object);

         var sut = new ProductRepository(dataContext.Object, propProvider.Object);

         var result = sut.GetProductOrderReferenceIdentifier(1);

         Assert.AreEqual("078945648", result.Value);
         Assert.AreEqual(ProductIdentifierType.UPN, result.IdentifierType);
      }

      [TestMethod]
      public void TestBoth_UPN_And_SPC()
      {
         //test data set
         var testProduct = new Product() { ProductId = 1,SPC = "Test SPC", ScanCodes = new List<ScanCode> { new ScanCode { Value = "078945648" } } };

         //setup
         var dataContext = new Mock<IDbContextInventoryContext>();
         var propProvider = new Mock<IPropertyProvider>();
         var productsDataSet = MockingHelper.GetMockedDbSet(new List<Product> { testProduct }.AsQueryable());

         dataContext.Setup(c => c.Products).Returns(productsDataSet.Object);

         var sut = new ProductRepository(dataContext.Object, propProvider.Object);

         var result = sut.GetProductOrderReferenceIdentifier(1);

         Assert.AreEqual("Test SPC", result.Value);
         Assert.AreEqual(ProductIdentifierType.SPC, result.IdentifierType);
      }

      [TestMethod]
      public void Test_OrderChannel()
      {
         //test data set
         var testProductOrderChannel = new OrderChannelProduct {ProductId = 1123,OrderChannelId = 9,Reference = "Test OC Ref"};
         var testProduct = new Product() {ProductId = 1123, OrderChannelProducts = new List<OrderChannelProduct> {testProductOrderChannel}};

         //setup
         var dataContext = new Mock<IDbContextInventoryContext>();
         var propProvider = new Mock<IPropertyProvider>();
         var productsDataSet = MockingHelper.GetMockedDbSet(new List<Product> { testProduct }.AsQueryable());

         dataContext.Setup(c => c.Products).Returns(productsDataSet.Object);

         var sut = new ProductRepository(dataContext.Object, propProvider.Object);

         var result = sut.GetProductOrderReferenceIdentifier(1123,9);

         Assert.AreEqual("Test OC Ref", result.Value);
         Assert.AreEqual(ProductIdentifierType.OrderChannel, result.IdentifierType);
      }

      [TestMethod]
      public void Test_OrderChannel_With_SPC_And_UPN()
      {
         //test data set
         var testProductOrderChannel = new OrderChannelProduct { ProductId = 1123, OrderChannelId = 9, Reference = "Test OC Ref" };
         var testProduct = new Product
         {
            ProductId = 1123,
            SPC = "Test SPC",
            ScanCodes = new List<ScanCode> {new ScanCode {Value = "078945648"}},
            OrderChannelProducts = new List<OrderChannelProduct> {testProductOrderChannel}
         };

         //setup
         var dataContext = new Mock<IDbContextInventoryContext>();
         var propProvider = new Mock<IPropertyProvider>();
         var productsDataSet = MockingHelper.GetMockedDbSet(new List<Product> { testProduct }.AsQueryable());

         dataContext.Setup(c => c.Products).Returns(productsDataSet.Object);

         var sut = new ProductRepository(dataContext.Object, propProvider.Object);

         var result = sut.GetProductOrderReferenceIdentifier(1123, 9);

         Assert.AreEqual("Test OC Ref", result.Value);
         Assert.AreEqual(ProductIdentifierType.OrderChannel, result.IdentifierType);
      }

   }
}
