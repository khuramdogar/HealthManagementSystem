using HMS.HealthTrack.Web.Data.Model.Inventory;
using NUnit.Framework;

namespace HMS.HealthTrack.Web.IntegrationTesting.Inventory
{
   [TestFixture]
   public class TestInventorySchema
   {
      [Test, Explicit]
      public void Add_and_remove_product()
      {
         var context = new InventoryContext();
         var product = new Product { };

         context.Products.Add(product);

         context.SaveChanges();

         context.Products.Remove(product);

         context.SaveChanges();
      }
   }
}