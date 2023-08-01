using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Diagnostics;

namespace HMS.HealthTrack.Web.Data.Model.Inventory
{
   public partial class InventoryContext : IDbContextInventoryContext
   {
      public InventoryContext(ConnectionStringSettings connectionStringSetting)
         : base(connectionStringSetting.ConnectionString)
      {
         // http://robsneuron.blogspot.com.au/2013/11/entity-framework-upgrade-to-6.html
         // ROLA - This is a hack to ensure that Entity Framework SQL Provider is copied across to the output folder.
         // As it is installed in the GAC, Copy Local does not work. It is required for probing.
         // Fixed "Provider not loaded" error
         var ensureDllIsCopied = SqlProviderServices.Instance;

         Database.Log = zz => Debug.WriteLine(zz);
      }
   }
}