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
   public class InventoryConsumptionProcessorTests
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
      public void TestNewConsumption_UnmanagedProcessed()
      {
         var consumption = new Consumption { ConsumptionReference = 1, LocationId = 1 };
         var consumptions = new List<Consumption> { consumption };
         var locationMapper = new HealthTrackLocationMapper
         {
            HealthTrackLocationId = 1,
            StockLocations = new List<int> { 1 }
         };
         var mockedConRepo = new Mock<IConsumptionRepository>();
         var mockedHealthTrackConRepo = new Mock<IHealthTrackConsumptionRepository>();
         var mockedProductRepo = new Mock<IProductRepository>();
         var mockedUoW = new Mock<IConsumptionUnitOfWork>();
         var mockedStockLocationRepo = new Mock<IStockLocationRepository>();


         mockedStockLocationRepo.Setup(l => l.GetHealthTrackLocationMapper(It.IsAny<int>())).Returns(locationMapper);
         mockedConRepo.Setup(c => c.UnprocessedConsumptions).Returns(consumptions.AsQueryable());
         mockedConRepo.Setup(c => c.CreateConsumptionManagement(It.IsAny<Consumption>())).Returns(new ConsumptionManagement());
         mockedConRepo.Setup(c => c.CreateConsumptionManagement(It.IsAny<Consumption>(), It.IsAny<string>())).Returns(new ConsumptionManagement());
         mockedUoW.Setup(u => u.FindOrCreateProduct(It.IsAny<Consumption>())).Returns(new Product());
         mockedUoW.Setup(u => u.StockLocationRepository).Returns(mockedStockLocationRepo.Object);
         mockedUoW.Setup(u => u.HealthTrackConsumptionRepository).Returns(mockedHealthTrackConRepo.Object);

         var consumptionProcessor = new InventoryConsumptionProcessor(mockedConRepo.Object, mockedProductRepo.Object, mockedUoW.Object);
         var conMan = consumptionProcessor.ProcessConsumption(consumption);

         //Verify
         mockedConRepo.Verify(c => c.CreateConsumptionManagement(It.IsAny<Consumption>()), Times.Once);
         mockedConRepo.Verify(c => c.ArchiveConsumptionNotification(It.IsAny<ConsumptionManagement>(), It.IsAny<string>()), Times.Once);
         conMan.ProcessingStatus = (int)ConsumptionProcessingStatus.Processed;
      }

      [TestMethod]
      public void TestNewConsumption_ManagedNoStock()
      {
         //Test data
         var consumption = new Consumption { ConsumptionReference = 1, LocationId = 1};
         var consumptions = new List<Consumption> { consumption };
         var product = new Product { ProductId = 1, ManageStock = true };
         var locationMapper = new HealthTrackLocationMapper { HealthTrackLocationId = 1, StockLocations = new List<int> { 1 } };
         var itemAdjustment = new ItemAdjustment { Quantity = 1, ProductId = product.ProductId };
         var stock = new Stock { StockStatus = StockStatus.Available, Quantity = 1 };
         var username = "Processor";
         var consumptionManagement = new ConsumptionManagement();

         //Arrange mocks
         var mockedConRepo = new Mock<IConsumptionRepository>();
         var mockedHealthTrackConRepo = new Mock<IHealthTrackConsumptionRepository>();
         var mockedProductRepo = new Mock<IProductRepository>();
         var mockedUoW = new Mock<IConsumptionUnitOfWork>();
         var mockedStockLocationRepo = new Mock<IStockLocationRepository>();
         var stockAdjustmentRepoMock = new Mock<IStockAdjustmentRepository>();
         var stockRepoMock = new Mock<IStockRepository>();
         var stockAdjustment = new StockAdjustment();

         //Arrange mocks to return data
         // Consumption Repo
         mockedConRepo.Setup(c => c.UnprocessedConsumptions).Returns(consumptions.AsQueryable());
         mockedConRepo.Setup(c => c.CreateConsumptionManagement(consumption)).Returns(consumptionManagement);
         mockedConRepo.Setup(c => c.CreateConsumptionManagement(consumption, It.IsAny<string>())).Returns(consumptionManagement);

         //Consumption unit of work
         mockedUoW.Setup(u => u.CreateAdjustmentFromConsumption(consumption, 1, 1, It.IsAny<DateTime>())).Returns(itemAdjustment);
         mockedUoW.Setup(u => u.FindOrCreateProduct(consumption)).Returns(product);

         //Unit of work repos
         mockedUoW.Setup(u => u.StockAdjustmentRepository).Returns(stockAdjustmentRepoMock.Object);
         mockedUoW.Setup(u => u.StockLocationRepository).Returns(mockedStockLocationRepo.Object);
         mockedUoW.Setup(u => u.HealthTrackConsumptionRepository).Returns(mockedHealthTrackConRepo.Object);
         mockedUoW.Setup(u => u.StockRepository).Returns(stockRepoMock.Object);
         
         //Stock Locations
         mockedStockLocationRepo.Setup(l => l.GetHealthTrackLocationMapper(1)).Returns(locationMapper);
         
         //Stock
         stockRepoMock.Setup(c => c.CreateNegativeStock(itemAdjustment,product,username)).Returns(stock);

         //Stock adjustments
         stockAdjustmentRepoMock.Setup(u => u.CreateStockAdjustment(itemAdjustment, username)).Returns(stockAdjustment);
         
         //sut
         var consumptionProcessor = new InventoryConsumptionProcessor(mockedConRepo.Object, mockedProductRepo.Object, mockedUoW.Object);
         var conMan = consumptionProcessor.ProcessConsumption(consumption);

         //Verify
         mockedConRepo.Verify(c => c.CreateConsumptionManagement(consumption), Times.Once);
         mockedConRepo.Verify(c => c.ArchiveConsumptionNotification(consumptionManagement, It.IsAny<string>()), Times.Never,"Shoudn't be archived");
         mockedUoW.Verify(u=>u.CreateAdjustmentFromConsumption(consumption,1,1,It.IsAny<DateTime>()),Times.Once,"Adjustment object created from consumption");
         stockAdjustmentRepoMock.Verify(u=>u.CreateStockAdjustment(itemAdjustment,username),Times.Once,"Adjustment row added to stock adjustments");
         stockRepoMock.Verify(r => r.CreateNegativeStock(itemAdjustment,product,username),Times.Once,"Negative stock created");
         conMan.ProcessingStatus = (int)ConsumptionProcessingStatus.Processed;
      }

      [TestMethod]
      public void TestNewConsumption_StockPresent()
      {
         //Test data
         var consumption = new Consumption { ConsumptionReference = 1, LocationId = 1 };
         var consumptions = new List<Consumption> { consumption };
         var product = new Product { ProductId = 1, ManageStock = true };
         var locationMapper = new HealthTrackLocationMapper { HealthTrackLocationId = 1, StockLocations = new List<int> { 1 } };
         var itemAdjustment = new ItemAdjustment { Quantity = 1, ProductId = product.ProductId};
         var stock = new Stock { StockStatus = StockStatus.Available, Quantity = 1, ProductId = product.ProductId};
         const string username = "Processor";
         var consumptionManagement = new ConsumptionManagement();

         //Arrange mocks
         var mockedConRepo = new Mock<IConsumptionRepository>();
         var mockedHealthTrackConRepo = new Mock<IHealthTrackConsumptionRepository>();
         var mockedProductRepo = new Mock<IProductRepository>();
         var mockedUoW = new Mock<IConsumptionUnitOfWork>();
         var mockedStockLocationRepo = new Mock<IStockLocationRepository>();
         var stockAdjustmentRepoMock = new Mock<IStockAdjustmentRepository>();
         var stockRepoMock = new Mock<IStockRepository>();
         var stockAdjustment = new StockAdjustment();

         //Arrange mocks to return data
         // Consumption Repo
         mockedConRepo.Setup(c => c.UnprocessedConsumptions).Returns(consumptions.AsQueryable());
         mockedConRepo.Setup(c => c.CreateConsumptionManagement(consumption)).Returns(consumptionManagement);
         mockedConRepo.Setup(c => c.CreateConsumptionManagement(consumption, It.IsAny<string>())).Returns(consumptionManagement);

         //Consumption unit of work
         mockedUoW.Setup(u => u.CreateAdjustmentFromConsumption(consumption, 1, 1, It.IsAny<DateTime>())).Returns(itemAdjustment);
         mockedUoW.Setup(u => u.FindOrCreateProduct(consumption)).Returns(product);

         //Unit of work repos
         mockedUoW.Setup(u => u.StockAdjustmentRepository).Returns(stockAdjustmentRepoMock.Object);
         mockedUoW.Setup(u => u.StockLocationRepository).Returns(mockedStockLocationRepo.Object);
         mockedUoW.Setup(u => u.HealthTrackConsumptionRepository).Returns(mockedHealthTrackConRepo.Object);
         mockedUoW.Setup(u => u.StockRepository).Returns(stockRepoMock.Object);

         //Stock Locations
         mockedStockLocationRepo.Setup(l => l.GetHealthTrackLocationMapper(1)).Returns(locationMapper);

         //Stock
         stockRepoMock.Setup(c => c.GetAvailableStock()).Returns(new List<Stock>{stock}.AsQueryable);
         stockRepoMock.Setup(c => c.CreateNegativeStock(itemAdjustment, product, username)).Returns(stock);

         //Stock adjustments
         stockAdjustmentRepoMock.Setup(u => u.CreateStockAdjustment(itemAdjustment, username)).Returns(stockAdjustment);

         //sut
         var consumptionProcessor = new InventoryConsumptionProcessor(mockedConRepo.Object, mockedProductRepo.Object, mockedUoW.Object);
         var conMan = consumptionProcessor.ProcessConsumption(consumption);

         //Verify
         mockedConRepo.Verify(c => c.CreateConsumptionManagement(consumption), Times.Once);
         mockedConRepo.Verify(c => c.ArchiveConsumptionNotification(consumptionManagement, It.IsAny<string>()), Times.Never, "Shoudn't be archived");
         mockedUoW.Verify(u => u.CreateAdjustmentFromConsumption(consumption, 1, 1, It.IsAny<DateTime>()), Times.Once, "Adjustment object created from consumption");
         stockAdjustmentRepoMock.Verify(u => u.CreateStockAdjustment(itemAdjustment, username), Times.Once, "Adjustment row added to stock adjustments");
         stockRepoMock.Verify(r => r.CreateNegativeStock(itemAdjustment, product, username), Times.Never, "Negative stock created");
         conMan.ProcessingStatus = (int)ConsumptionProcessingStatus.Processed;
      }
   }
}
