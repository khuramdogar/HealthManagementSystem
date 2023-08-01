using System;
using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Areas.Inventory;
using HMS.HealthTrack.Web.Data.Helpers;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Consumptions
{
   [TestClass]
   public class InventoryConsumptionProcessorLocationTests
   {
      [TestMethod]
      public void TestNoConsumptions()
      {
         var mockedConRepo = new Mock<IConsumptionRepository>();
         var mockedProductRepo = new Mock<IProductRepository>();
         var mockedUoW = new Mock<IConsumptionUnitOfWork>();
         var consumptionProcessor = new InventoryConsumptionProcessor(mockedConRepo.Object, mockedProductRepo.Object, mockedUoW.Object);
         consumptionProcessor.ProcessConsumptions();
      }

      [TestMethod]
      public void TestNewConsumption_ConsumptionLocationMissing()
      {
         var consumption = new Consumption {ConsumptionReference = 1};
         var consumptions = new List<Consumption> {consumption};

         var mockedConRepo = new Mock<IConsumptionRepository>();
         var mockedProductRepo = new Mock<IProductRepository>();
         var mockedUoW = new Mock<IConsumptionUnitOfWork>();
         var mockedStockLocationRepo = new Mock<IStockLocationRepository>();

         mockedConRepo.Setup(c => c.UnprocessedConsumptions).Returns(consumptions.AsQueryable());  
         mockedConRepo.Setup(c => c.CreateConsumptionManagement(It.IsAny<Consumption>())).Returns(new ConsumptionManagement());
         mockedConRepo.Setup(c => c.CreateConsumptionManagement(It.IsAny<Consumption>(),It.IsAny<string>())).Returns(new ConsumptionManagement());
         mockedUoW.Setup(u => u.FindOrCreateProduct(It.IsAny<Consumption>())).Returns(new Product());
         mockedUoW.Setup(u => u.StockLocationRepository).Returns(mockedStockLocationRepo.Object);

         var consumptionProcessor = new InventoryConsumptionProcessor(mockedConRepo.Object, mockedProductRepo.Object, mockedUoW.Object);
         var conMan = consumptionProcessor.ProcessConsumption(consumption);

         //Verify
         mockedConRepo.Verify(c => c.CreateConsumptionManagement(It.IsAny<Consumption>(), It.IsAny<string>()),Times.Once);
         conMan.ProcessingStatusMessage = ConsumptionProcessingMessages.LocationMissing(consumption);
      }

      
   }
}
