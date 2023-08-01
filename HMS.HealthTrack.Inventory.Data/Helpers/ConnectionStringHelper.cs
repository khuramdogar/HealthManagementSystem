using System;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using Serilog;

namespace HMS.HealthTrack.Web.Data.Helpers
{
   public static class ConnectionStringHelper
   {
      public static string GetConnectionString(string contextString)
      {
         var entityConString = ConfigurationManager.ConnectionStrings[contextString];
         var connectionString = entityConString.ConnectionString;

         var isEFConnectionString = connectionString.Contains("metadata=");
         if (!isEFConnectionString) return connectionString;

         var builder = new EntityConnectionStringBuilder();
         builder.ConnectionString = connectionString;
         var providerConnectionString = builder.ProviderConnectionString;

         var conStringBuilder = new SqlConnectionStringBuilder();
         conStringBuilder.ConnectionString = providerConnectionString;
         conStringBuilder.ConnectTimeout = 1;
         return conStringBuilder.ConnectionString;
      }

      public static string GetDatabaseNameForConnection(string contextName)
      {
         try
         {
            var constr = GetConnectionString(contextName);

            using (var conn = new SqlConnection(constr))
            {
               return conn.Database;
            }
         }
         catch (Exception exception)
         {
            Log.Logger.Error(exception, string.Format("Unable to get Database Name for connection: {0}", contextName));
         }

         return string.Empty;
      }
   }
}