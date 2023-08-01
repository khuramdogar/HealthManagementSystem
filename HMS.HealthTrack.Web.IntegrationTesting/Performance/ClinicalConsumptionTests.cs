using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMS.HealthTrack.Web.Data.Model.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework.Internal;

namespace HMS.HealthTrack.Web.IntegrationTesting.Performance
{
   [TestClass]
   public class ClinicalConsumptionTests
   {
      [TestMethod]
      public void GetSomeClinicalConsumptions()
      {
         var db = new InventoryContext();
         db.Database.CommandTimeout = 60;
         var newestRecord = DateTime.Parse("2016-09-19 12:30:27.260");
         var timeLimit = newestRecord.AddDays(-500);
         var start = DateTime.Now;
         
         var clinicalRecords = (from c in db.ClinicalConsumptions where c.testDate >= timeLimit select c).ToList();
         Console.WriteLine($"{clinicalRecords.Count} took {(DateTime.Now - start).TotalSeconds}");
      }

      [TestMethod]
      public void GetFirstClinicalConsumptions()
      {
         var db = new InventoryContext();
         var start = DateTime.Now;

         var clinicalRecords = db.ClinicalConsumptions.First();
         Console.WriteLine($"1 record took {(DateTime.Now - start).TotalSeconds}");
      }

      [TestMethod]
      public void GetFirstXClinicalConsumptions()
      {
         var db = new InventoryContext();
         var start = DateTime.Now;

         var clinicalRecords = db.ClinicalConsumptions.Take(100);
         Console.WriteLine($"100 record took {(DateTime.Now - start).TotalSeconds}");
      }

      [TestMethod]
      public void GetFirstYClinicalConsumptions()
      {
         var db = new InventoryContext();
         var start = DateTime.Now;

         var clinicalRecords = db.ClinicalConsumptions.Take(9823);
         Console.WriteLine($"10000 record took {(DateTime.Now - start).TotalSeconds}");
      }
   }
}
