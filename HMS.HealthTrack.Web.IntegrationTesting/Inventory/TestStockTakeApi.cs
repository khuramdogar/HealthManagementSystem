using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using HMS.HealthTrack.Web.Areas.Inventory.Models.StockTakes;
using Newtonsoft.Json;
using NUnit.Framework;

namespace HMS.HealthTrack.Web.IntegrationTesting.Inventory
{
   [TestFixture]
   public class TestStockTakeApi
   {
      private const string testServerBaseUrl = "http://Testweb.healthtrack.com.au/";

      public Uri StockLocationsUrl
      {
         get { return new Uri(new Uri(testServerBaseUrl), "api/StockLocations"); }
      }

      public Uri StockTakeUrl
      {
         get { return new Uri(new Uri(testServerBaseUrl), "api/Inventory/StockTakes/CreateAndSubmit"); }
      }

      [Test, Explicit]
      public void TestGetStockLocations()
      {
         var client = new HttpClient();
         var result = client.GetAsync(StockLocationsUrl);
         var content = result.Result.Content.ReadAsStringAsync();
         Assert.IsNotNull(content);
      }

      [Test, Explicit]
      public void TestCreateAndSubmitStockTake_WithProductId()
      {
         var stockTakeItem = new IncomingStockTakeItem
         {
            Description = "TestItem",
            ProductId = 4322,
            SPC = "RS*B50N10MR5",
            StockLevel = 5,
            //UPN = "893522120050",
            Message = "Test",
         };

         var stockTake = new IncomingStockTake {CreatedBy = "Test", CreatedOn = DateTime.Now, StockTakeDate = DateTime.Now, Items = new List<IncomingStockTakeItem> {stockTakeItem}, LocationId = 1};
         var client = new HttpClient {BaseAddress = new Uri(testServerBaseUrl)};
         var result = client.PostAsJsonAsync("api/Inventory/StockTakes/CreateAndSubmit", stockTake);
         var content = result.Result.Content.ReadAsStringAsync();
         Assert.AreEqual(HttpStatusCode.OK, result.Result.StatusCode);
         Assert.IsNotNull(content);
      }


      [Test, Explicit]
      public void TestCreateAndSubmitStockTake_WithUPCAndSPC_Matches2Products()
      {
         var stockTakeItem = new IncomingStockTakeItem
         {
            Description = "TestItem",
            SPC = "RS*B50N10MR5",
            StockLevel = 5,
            //UPN = "893522120050",
            Message = "Test",
         };

         var stockTake = new IncomingStockTake { CreatedBy = "Test", CreatedOn = DateTime.Now, StockTakeDate = DateTime.Now, Items = new List<IncomingStockTakeItem> { stockTakeItem }, LocationId = 1 };
         var client = new HttpClient { BaseAddress = new Uri(testServerBaseUrl) };
         var result = client.PostAsJsonAsync("api/Inventory/StockTakes/CreateAndSubmit", stockTake);
         var content = result.Result.Content.ReadAsStringAsync();
         Assert.AreEqual(HttpStatusCode.BadRequest, result.Result.StatusCode);
         Assert.IsNotNull(content);
      }

      [Test, Explicit]
      public void TestCreateAndSubmitStockTake_WithUPC()
      {
         var stockTakeItem = new IncomingStockTakeItem
         {
            Description = "TestItem",
            StockLevel = 5,
            //UPN = "893522120050",
            Message = "Test",
         };

         var stockTake = new IncomingStockTake { CreatedBy = "Test", CreatedOn = DateTime.Now, StockTakeDate = DateTime.Now, Items = new List<IncomingStockTakeItem> { stockTakeItem }, LocationId = 1 };
         var client = new HttpClient { BaseAddress = new Uri(testServerBaseUrl) };
         var result = client.PostAsJsonAsync("api/Inventory/StockTakes/CreateAndSubmit", stockTake);
         var content = result.Result.Content.ReadAsStringAsync();
         Assert.AreEqual(HttpStatusCode.OK, result.Result.StatusCode);
         Assert.IsNotNull(content);
      }


      [Test, Explicit]
      public void TestCreateAndSubmitStockTake_TestCase()
      {
         var stockTakeItem = new IncomingStockTakeItem
         {
            StockLevel = 5,
            ProductId = 4317,
            SPC = "504-605X",
            //UPN = "H739504605X",
            Description = "5F Cordis 11cm sheath",
         };

         var stockTake = new IncomingStockTake { CreatedBy = "Test", CreatedOn = DateTime.Now, StockTakeDate = DateTime.Now, Items = new List<IncomingStockTakeItem> { stockTakeItem }, LocationId = 1 };
         var client = new HttpClient { BaseAddress = new Uri(testServerBaseUrl) };
         var result = client.PostAsJsonAsync("api/Inventory/StockTakes/CreateAndSubmit", stockTake);
         var content = result.Result.Content.ReadAsStringAsync();
         Assert.AreEqual(HttpStatusCode.OK, result.Result.StatusCode);
         Assert.IsNotNull(content);
      }

   }
}
