using System.Collections.Generic;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Products
{
   [TestClass]
   public class ProductTests
   {
      private const string Username = "UnitTest";

      private Product Product
      {
         get
         {
            return new Product
            {
               AutoReorderSetting = ReorderSettings.SpecifyLevels,
               Description = "Test Product",
               ManageStock = true,
               SPC = "SPC",
               PrimarySupplier = 1,
               PrimarySupplierCompany = new Company
               {
                  companyName = "TestCompany",
                  company_ID = 1,
               },
               ProductId = 123,
               ReorderThreshold = 5,
               TargetStockLevel = 10,
            };
         }
      }

      private Stock Stock
      {
         get
         {
            return new Stock
            {
               ProductId = 123,
               Quantity = 4,
               ReceivedQuantity = 4,
               StockId = 1,
               StockStatus = StockStatus.Available,
               StoredAt = 1
            };
         }
      }

      #region ManageStock

      [TestMethod]
      public void ProductValidation_ManageStock_SpecifyLevels_CorrectLevels()
      {
         var product = new Product()
         {
            AutoReorderSetting = ReorderSettings.SpecifyLevels,
            ManageStock = true,
            ReorderThreshold = 10,
            TargetStockLevel = 20
         };

         var validation = ProductRepository.ValidStockHandling(product);

         Assert.IsTrue(validation);
      }

      [TestMethod]
      public void ProductValidation_ManageStock_SpecifyLevels_ThresholdAboveTarget()
      {
         var product = new Product()
         {
            AutoReorderSetting = ReorderSettings.SpecifyLevels,
            ManageStock = true,
            ReorderThreshold = 20,
            TargetStockLevel = 10
         };

         var validation = ProductRepository.ValidStockHandling(product);

         Assert.IsFalse(validation);
      }

      [TestMethod]
      public void ProductValidation_ManageStock_SpecifyLevels_ThresholdEqualTarget()
      {
         var product = new Product()
         {
            AutoReorderSetting = ReorderSettings.SpecifyLevels,
            ManageStock = true,
            ReorderThreshold = 20,
            TargetStockLevel = 20
         };

         var validation = ProductRepository.ValidStockHandling(product);

         Assert.IsTrue(validation);
      }

      [TestMethod]
      public void ProductValidation_ManageStock_SpecifyLevels_NoTarget()
      {
         var product = new Product()
         {
            AutoReorderSetting = ReorderSettings.SpecifyLevels,
            ManageStock = true,
            ReorderThreshold = 10,
            TargetStockLevel = null
         };

         var validation = ProductRepository.ValidStockHandling(product);

         Assert.IsFalse(validation);
      }

      [TestMethod]
      public void ProductValidation_ManageStock_SpecifyLevels_NoThreshold()
      {
         var product = new Product()
         {
            AutoReorderSetting = ReorderSettings.SpecifyLevels,
            ManageStock = true,
            ReorderThreshold = null,
            TargetStockLevel = 10,
         };

         var validation = ProductRepository.ValidStockHandling(product);

         Assert.IsFalse(validation);
      }

      [TestMethod]
      public void ProductValidation_ManageStock_DoNotReorder()
      {
         var product = new Product()
         {
            AutoReorderSetting = ReorderSettings.DoNotReorder,
            ManageStock = true,
         };

         var validation = ProductRepository.ValidStockHandling(product);

         Assert.IsTrue(validation);
      }

      [TestMethod]
      public void ProductValidation_ManageStock_OneForOneReplace()
      {
         var product = new Product()
         {
            AutoReorderSetting = ReorderSettings.OneForOneReplace,
            ManageStock = true,
         };

         var validation = ProductRepository.ValidStockHandling(product);

         Assert.IsFalse(validation);
      }

      [TestMethod]
      public void ProductValidation_UnManageStock_SpecifyLevels()
      {
         var product = new Product()
         {
            AutoReorderSetting = ReorderSettings.SpecifyLevels,
            ManageStock = false,
            ReorderThreshold = 20,
            TargetStockLevel = 10
         };

         var validation = ProductRepository.ValidStockHandling(product);

         Assert.IsFalse(validation);
      }

      [TestMethod]
      public void ProductValidation_UnManageStock_OneForOneReplace()
      {
         var product = new Product()
         {
            AutoReorderSetting = ReorderSettings.OneForOneReplace,
            ManageStock = false,
            ReorderThreshold = 20,
            TargetStockLevel = 10
         };

         var validation = ProductRepository.ValidStockHandling(product);

         Assert.IsTrue(validation);
      }

      [TestMethod]
      public void ProductValidation_UnManageStock_DoNotReorder()
      {
         var product = new Product()
         {
            AutoReorderSetting = ReorderSettings.DoNotReorder,
            ManageStock = false,
            ReorderThreshold = 20,
            TargetStockLevel = 10
         };

         var validation = ProductRepository.ValidStockHandling(product);

         Assert.IsTrue(validation);
      }

      #endregion

      [TestMethod]
      public void WriteOffAvailableStock_AdjustmentsAdded()
      {
         var product = Product;
         product.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         product.ManageStock = false;

         var testData = new MockedInventoryProductData(new List<Product> { Product }, new List<PriceType>(),
            new List<HealthTrackConsumption>(), new List<ExternalProductMapping>(),
            new List<ConsumptionNotificationManagement>(), new List<StockAdjustment>(), new List<Stock> { Stock }, new List<ProductCategory>());
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         uow.WriteOffAvailableStockForProductPerLocation(product, Username);

         testData.MockedStockAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Once());
         testData.MockedStockAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(sa => sa.Quantity == Stock.Quantity)), Times.Once());
         testData.MockedStockAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(sa => sa.StockAdjustmentReasonId != null)), Times.Once());
         testData.MockedStockAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(sa => sa.Source == AdjustmentSource.System)), Times.Once());

         testData.MockedStock.Verify(m => m.Add(It.IsAny<Stock>()), Times.Never());
      }

      [TestMethod]
      public void WriteOffAvailableStock_StockQuantity_Deducted()
      {
         var product = Product;
         product.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         product.ManageStock = false;

         var stock = Stock;

         var testData = new MockedInventoryProductData(new List<Product> { Product }, new List<PriceType>(),
            new List<HealthTrackConsumption>(), new List<ExternalProductMapping>(),
            new List<ConsumptionNotificationManagement>(), new List<StockAdjustment>(), new List<Stock> { stock }, new List<ProductCategory>());
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         uow.WriteOffAvailableStockForProductPerLocation(product, Username);

         Assert.AreEqual(0, stock.Quantity);
      }

      [TestMethod]
      public void WriteOffAvailableStock_StockAtMultipleLocations_SingleAdjustment()
      {
         var product = Product;
         product.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         product.ManageStock = false;

         var stock = Stock;
         var stock2 = Stock;
         stock.StoredAt = 2;

         var testData = new MockedInventoryProductData(new List<Product> { Product }, new List<PriceType>(),
            new List<HealthTrackConsumption>(), new List<ExternalProductMapping>(),
            new List<ConsumptionNotificationManagement>(), new List<StockAdjustment>(), new List<Stock> { stock, stock2 }, new List<ProductCategory>());
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         uow.WriteOffAvailableStockForProductPerLocation(product, Username);

         var totalStock = Stock.Quantity + Stock.Quantity;
         testData.MockedStockAdjustments.Verify(m => m.Add(It.IsAny<StockAdjustment>()), Times.Once());
         testData.MockedStockAdjustments.Verify(m => m.Add(It.Is<StockAdjustment>(sa => sa.Quantity == totalStock)), Times.Once());

         testData.MockedStock.Verify(m => m.Add(It.IsAny<Stock>()), Times.Never());
      }

      [TestMethod]
      public void WriteOffAvailableStock_StockAtMultipleLocations_Deducted()
      {
         var product = Product;
         product.AutoReorderSetting = ReorderSettings.OneForOneReplace;
         product.ManageStock = false;

         var stock = Stock;
         var stock2 = Stock;
         stock.StoredAt = 2;

         var testData = new MockedInventoryProductData(new List<Product> { Product }, new List<PriceType>(),
            new List<HealthTrackConsumption>(), new List<ExternalProductMapping>(),
            new List<ConsumptionNotificationManagement>(), new List<StockAdjustment>(), new List<Stock> { stock, stock2 }, new List<ProductCategory>());
         var uow = MockedInventoryUnitOfWorkFactory.GetProductUnitOfWork(testData);

         uow.WriteOffAvailableStockForProductPerLocation(product, Username);

         Assert.AreEqual(0, stock.Quantity);
         Assert.AreEqual(0, stock2.Quantity);
      }



   }
}
