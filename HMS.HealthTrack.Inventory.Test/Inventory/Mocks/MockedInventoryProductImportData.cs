using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HMS.HealthTrack.Inventory.Common.Constants;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Mocks
{
   internal class MockedInventoryProductImportData : MockedInventoryData
   {
      public Mock<IDbSet<GeneralLedger>> MockedGeneralLedgers { get; set; }
      public Mock<IDbSet<GeneralLedgerTier>> MockedGeneralLedgerTiers { get; set; }
      public Mock<IDbSet<Company>> MockedCompanies { get; set; }
      public Mock<IDbSet<Supplier>> MockedSuppliers { get; set; }
      public Mock<IDbSet<GeneralLedgerType>> MockedGeneralLedgerTypes { get; set; }
      public Mock<IDbSet<Product>> MockedProducts { get; set; }


      public MockedInventoryProductImportData()
      {
         MockedGeneralLedgers = MockingHelper.GetMockedDbSet(new List<GeneralLedger>().AsQueryable());
         MockedGeneralLedgerTiers = MockingHelper.GetMockedDbSet(new List<GeneralLedgerTier>().AsQueryable());
         MockedCompanies = MockingHelper.GetMockedDbSet(new List<Company>().AsQueryable());
         MockedSuppliers = MockingHelper.GetMockedDbSet(new List<Supplier>().AsQueryable());
         MockedGeneralLedgerTypes = MockingHelper.GetMockedDbSet(new List<GeneralLedgerType>
         {
            new GeneralLedgerType
            {
               LedgerTypeId = 1,
               Name = InventoryConstants.ProductLedgerType
            }
         }.AsQueryable());

         MockedProducts = MockingHelper.GetMockedDbSet(new List<Product>().AsQueryable());

         MockedContext.Setup(c => c.GeneralLedgers).Returns(MockedGeneralLedgers.Object);
         MockedContext.Setup(c => c.GeneralLedgerTiers).Returns(MockedGeneralLedgerTiers.Object);
         MockedContext.Setup(c => c.Companies).Returns(MockedCompanies.Object);
         MockedContext.Setup(c => c.GeneralLedgerTypes).Returns(MockedGeneralLedgerTypes.Object);
         MockedContext.Setup(c => c.Suppliers).Returns(MockedSuppliers.Object);
         MockedContext.Setup(c => c.Products).Returns(MockedProducts.Object);

         MockedPropertyProvider.Setup(c => c.LedgerTypes).Returns(new List<GeneralLedgerType>
         {
            new GeneralLedgerType
            {
               LedgerTypeId = 1,
               Name = InventoryConstants.ProductLedgerType
            }
         });
      }
   }
}