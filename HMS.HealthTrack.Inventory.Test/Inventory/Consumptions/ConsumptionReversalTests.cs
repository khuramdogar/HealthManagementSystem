using System.Collections.Generic;
using System.Linq;
using HMS.HealthTrack.Web.Areas.Inventory;
using HMS.HealthTrack.Web.Data.Exceptions;
using HMS.HealthTrack.Web.Data.Model;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HMS.HealthTrack.Inventory.Test.Inventory.Consumptions
{
   [TestClass]
   public class ConsumptionReversalTests
   {
      [TestMethod]
      public void TestNoReversals()
      {
         var mockedConRepo = new Mock<IConsumptionRepository>();
         var mockedProductRepo = new Mock<IProductRepository>();
         var mockedUoW = new Mock<IConsumptionUnitOfWork>();
         var consumptionProcessor = new InventoryConsumptionProcessor(mockedConRepo.Object,mockedProductRepo.Object,mockedUoW.Object);
         consumptionProcessor.ProcessConsumptions();
      }

      [TestMethod]
      public void ReverseConsumption()
      {
         //Test data
         var consumptionReversal = new ConsumptionReversal{ConsumptionReference = 99};
         var consumption = new Consumption{ConsumptionReference = 99};
         var consumptionsWithoutReversals = new List<Consumption>{consumption}.AsQueryable();
         var consumptionManagement = new ConsumptionManagement();

         //Arrange mocks
         var mockedConRepo = new Mock<IConsumptionRepository>();
         var mockedProductRepo = new Mock<IProductRepository>();
         var mockedUoW = new Mock<IConsumptionUnitOfWork>();

         mockedConRepo.Setup(c => c.ReversedConsumption(99)).Returns(consumptionReversal);
         mockedConRepo.Setup(c => c.UnprocessedConsumptions).Returns(consumptionsWithoutReversals);
         mockedConRepo.Setup(c => c.CreateConsumptionManagement(consumption)).Returns(consumptionManagement);

         //sut
         var consumptionProcessor = new InventoryConsumptionProcessor(mockedConRepo.Object, mockedProductRepo.Object, mockedUoW.Object);

         //act
         consumptionProcessor.ProcessConsumptions();

         //verify
         mockedConRepo.Verify(c=>c.CreateConsumptionManagement(consumption),Times.Once);
         mockedConRepo.Verify(c=>c.Commit(),Times.Once);

         Assert.AreEqual((int)ConsumptionProcessingStatus.Ignored, consumptionManagement.ProcessingStatus);
         Assert.AreEqual(ConsumptionProcessingMessages.ConsumptionReversed(consumptionReversal), consumptionManagement.ProcessingStatusMessage);
      }
   }
}
