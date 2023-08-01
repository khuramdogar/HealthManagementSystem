using System.Configuration;

namespace HMS.HealthTrack.Web.Data.Model.Security
{
   public partial class Security : IDbContextSecurity
   {
      public Security(ConnectionStringSettings connectionStringSetting)
         : base(connectionStringSetting.ConnectionString)
      {
      }
   }
}