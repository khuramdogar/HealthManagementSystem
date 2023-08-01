using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using HMS.HealthTrack.Inventory.Common;
using HMS.HealthTrack.Web.Areas.Inventory.Controllers.API;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory;
using HMS.HealthTrack.Web.Data.Repositories.Inventory.UnitsOfWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace HMS.HealthTrack.Inventory.Test.Inventory.StockTakes
{
   [TestClass]
   public class StockTakeApiTests
   {
      [TestMethod]
      public void TestStockTakeProcessing_WriteOff_AllDeleted()
      {
         //Test Stock data
         var product = new Product { ProductId = 123 , SPC = "SPC999"};
         //product.UPN = "085533456845";

         //Test stock take data
         var stockTakeItem = new IncomingStockTakeItem {Description = "Big Stent", ProductId = 123, SPC = "SPC999", StockLevel = 1,};
         //stockTakeItem.UPN = "085533456845";
         var stockTake = new IncomingStockTake{  LocationId = 1, Items = new List<IncomingStockTakeItem> { stockTakeItem } };

         var mockedStockTakeUnitOfWork = new Mock<IStockTakeUnitOfWork>();
         var stockTakeRepo = new Mock<IStockTakeRepository>();
         mockedStockTakeUnitOfWork.Setup(u => u.StockTakeRepository).Returns(stockTakeRepo.Object);
         var logger = new Mock<ICustomLogger>();
         var propProvider = new Mock<IPropertyProvider>();

         //SUT
         var sut = new StockTakesController(mockedStockTakeUnitOfWork.Object, propProvider.Object, logger.Object) {Request = new HttpRequestMessage()};
         sut.Request.SetConfiguration(new HttpConfiguration());

         var result = sut.CreateAndSubmit(stockTake);

         var submissionResult = JsonConvert.DeserializeObject<StockTakeSubmissionResult>(result.Content.ReadAsStringAsync().Result);
         
         stockTakeRepo.Verify(s => s.Add(It.IsAny<StockTake>()), Times.Once, "New Stock");
         mockedStockTakeUnitOfWork.Verify(s => s.ProcessStockTake(It.IsAny<StockTake>()), Times.Once, "New Stock");
         
         Assert.AreEqual("Stock take complete", submissionResult.Message);
      }
   }
}
