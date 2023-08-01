using DbUp;
using DbUp.Engine;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace HMS.HealthTrack.Web.Migrator
{
   internal class Program
   {
      private static int Main(string[] args)
      {
         ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["HealthTrackDatabase"];
         if (connectionString == null)
            return ShowFatalError("No connection present in config");

         if (!TestConnectionString(connectionString.ConnectionString))
            return
               ShowFatalError("Failed to connect to the database with the configured connection string: " +
                              connectionString.ConnectionString);

         Console.WriteLine("Checking {0} for upgrades", connectionString.ConnectionString);

         UpgradeEngine upgrader = DeployChanges.To
            .SqlDatabase(connectionString.ConnectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransaction()
            .LogToConsole()
            .Build();

         DatabaseUpgradeResult result = upgrader.PerformUpgrade();

         if (!result.Successful)
         {
            return ShowFatalError(result.Error.ToString());
         }

         Console.ForegroundColor = ConsoleColor.Green;
         Console.WriteLine("Success!");
         Console.ResetColor();
         if (Environment.UserInteractive)
            Console.ReadLine();
         return 0;
      }

      private static bool TestConnectionString(string connectionString)
      {
         using (var conn = new SqlConnection(connectionString))
         {
            try
            {
               conn.Open();

               return (conn.State == ConnectionState.Open);
            }
            catch
            {
               return false;
            }
         }
      }

      private static int ShowFatalError(string error)
      {
         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine(error);
         Console.ResetColor();

         if (Environment.UserInteractive)
            Console.ReadKey();

         return -1; //Error code
      }
   }
}