using System.Configuration;

namespace HMS.HealthTrack.Web.Data.Model.Clinical
{
   public partial class ClinicalContext : IDbContextClinicalContext
   {
      public ClinicalContext(ConnectionStringSettings connectionStringSetting)
         : base(connectionStringSetting.ConnectionString)
      {
      }
   }
}