using System.Linq;
using System.Text;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Inventory.Test.Inventory.Mocks;
using HMS.HealthTrack.Web.Areas.Inventory;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Products
{
   /// <summary>
   /// Summary description for ProductImportTests
   /// </summary>
   [TestClass]
   public class ProductImportTests
   {
      [TestMethod]
      public void MaxStringLengthValidation_BelowMaxLength()
      {
         var supplierFieldname = Nameof<ProductImport>.Property(p => p.Supplier);

         var supplier = "MEDICAL VISION AUSTRALIA CARDIOLOGY & THORACIC PTY LTD";

         var testData = new MockedInventoryProductImportData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductImportUnitOfWork(testData);

         var helper = new ProductImportHelper(uow, testData.MockedPropertyProvider.Object);

         var maxStringLength = helper.GetMaxStringLength(supplierFieldname);

         Assert.IsTrue(supplier.Length < maxStringLength, "Max length is: " + maxStringLength);
      }

      [TestMethod]
      public void MaxStringLengthValidation_AtMaxLength()
      {
         var supplierFieldname = Nameof<ProductImport>.Property(p => p.Supplier);

         var supplier = "MEDICAL VISION AUSTRALIA CARDIOLOGY & THORACIC PTY LTD";
         supplier += supplier;
         supplier = supplier.Substring(0, 100);

         var testData = new MockedInventoryProductImportData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductImportUnitOfWork(testData);

         var helper = new ProductImportHelper(uow, testData.MockedPropertyProvider.Object);

         var maxStringLength = helper.GetMaxStringLength(supplierFieldname);

         Assert.IsTrue(supplier.Length == maxStringLength, "Max length is: " + maxStringLength);
      }

      [TestMethod]
      public void MaxStringLengthValidation_AboveMaxLength()
      {
         var supplierFieldName = Nameof<ProductImport>.Property(p => p.Supplier);

         var supplier = "MEDICAL VISION AUSTRALIA CARDIOLOGY & THORACIC PTY LTD";
         supplier += supplier;

         var testData = new MockedInventoryProductImportData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductImportUnitOfWork(testData);
         var helper = new ProductImportHelper(uow, testData.MockedPropertyProvider.Object);

         var maxStringLength = helper.GetMaxStringLength(supplierFieldName);

         Assert.IsTrue(supplier.Length > maxStringLength, "Max length is: " + maxStringLength);
      }

      [TestMethod]
      public void SetProductImportStringValue_BelowMaxLength()
      {
         var supplier = "MEDICAL VISION AUSTRALIA CARDIOLOGY & THORACIC PTY LTD";

         var testData = new MockedInventoryProductImportData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductImportUnitOfWork(testData);
         var helper = new ProductImportHelper(uow, testData.MockedPropertyProvider.Object);
         var supplierFieldName = Nameof<ProductImport>.Property(p => p.Supplier);
         var spreadsheetColumnNames = typeof(ProductImportColumnNames).GetFields().Where(f => f.Name == supplierFieldName).
            ToDictionary(f => f.Name, f => f.GetValue(f).ToString()); // spreadsheet column

         var productImport = new ProductImport();
         var errorStringBuilder = new StringBuilder();
         var productImportProperties = ProductImportColumnNames.ProductImportColumnsNames;
         var productImportType = typeof(ProductImport);

         ProductImportProcessor.SetProductImportValueFromColumn(helper, productImport, errorStringBuilder,
            spreadsheetColumnNames.First(), productImportProperties, productImportType, supplier, testData.MockedLogger.Object);

         Assert.AreEqual(supplier, productImport.Supplier);
         Assert.AreEqual(string.Empty, errorStringBuilder.ToString());

      }

      [TestMethod]
      public void SetProductImportStringValue_AtMaxLength()
      {
         var supplier = "MEDICAL VISION AUSTRALIA CARDIOLOGY & THORACIC PTY LTD";
         supplier += supplier;
         supplier = supplier.Substring(0, 100);

         var testData = new MockedInventoryProductImportData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductImportUnitOfWork(testData);
         var helper = new ProductImportHelper(uow, testData.MockedPropertyProvider.Object);
         var supplierFieldName = Nameof<ProductImport>.Property(p => p.Supplier);
         var spreadsheetColumnNames = typeof(ProductImportColumnNames).GetFields().Where(f => f.Name == supplierFieldName).
            ToDictionary(f => f.Name, f => f.GetValue(f).ToString()); // spreadsheet column

         var productImport = new ProductImport();
         var errorStringBuilder = new StringBuilder();
         var productImportProperties = ProductImportColumnNames.ProductImportColumnsNames;
         var productImportType = typeof(ProductImport);

         ProductImportProcessor.SetProductImportValueFromColumn(helper, productImport, errorStringBuilder,
            spreadsheetColumnNames.First(), productImportProperties, productImportType, supplier, testData.MockedLogger.Object);

         Assert.AreEqual(supplier, productImport.Supplier);
         Assert.AreEqual(string.Empty, errorStringBuilder.ToString());
      }

      [TestMethod]
      public void SetProductImportStringValue_AboveMaxLength()
      {
         var supplier = "MEDICAL VISION AUSTRALIA CARDIOLOGY & THORACIC PTY LTD";
         supplier += supplier;

         var testData = new MockedInventoryProductImportData();
         var uow = MockedInventoryUnitOfWorkFactory.GetProductImportUnitOfWork(testData);
         var helper = new ProductImportHelper(uow, testData.MockedPropertyProvider.Object);
         var supplierFieldName = Nameof<ProductImport>.Property(p => p.Supplier);
         var spreadsheetColumnNames = typeof(ProductImportColumnNames).GetFields().Where(f => f.Name == supplierFieldName).
            ToDictionary(f => f.Name, f => f.GetValue(f).ToString()); // spreadsheet column

         var productImport = new ProductImport();
         var errorStringBuilder = new StringBuilder();
         var productImportProperties = ProductImportColumnNames.ProductImportColumnsNames;
         var productImportType = typeof(ProductImport);

         ProductImportProcessor.SetProductImportValueFromColumn(helper, productImport, errorStringBuilder,
            spreadsheetColumnNames.First(), productImportProperties, productImportType, supplier, testData.MockedLogger.Object);

         Assert.AreEqual(null, productImport.Supplier);
         Assert.AreNotEqual(string.Empty, errorStringBuilder.ToString());
      }


   }
}
