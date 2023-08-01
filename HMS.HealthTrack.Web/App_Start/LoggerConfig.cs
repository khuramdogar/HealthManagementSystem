using Serilog;
using SerilogWeb.Classic.Enrichers;
using System;
using System.Configuration;
using HMS.HealthTrack.Inventory.Common;
using Serilog.Core;
using Serilog.Events;

namespace HMS.HealthTrack.Web.App_Start
{
   public class LoggerConfig
   {
      public static ICustomLogger CreateLogger()
      {
         var seqConfig = ConfigurationManager.AppSettings["SeqUrl"];

         var levelSwitch = new LoggingLevelSwitch();
         levelSwitch.MinimumLevel = LogEventLevel.Verbose;

         var loggingConfig = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .Enrich.With(new HttpRequestIdEnricher())
            .Enrich.With(new HttpSessionIdEnricher())
            .Enrich.With<UserNameEnricher>()
            .WriteTo.Seq(seqConfig ?? "http://localhost:5341/").MinimumLevel.ControlledBy(levelSwitch)
             .CreateLogger()
             .ForContext("Application", ConfigurationManager.AppSettings["ApplicationIdentifier"] ?? System.AppDomain.CurrentDomain.FriendlyName)
             .ForContext("Machine", Environment.MachineName);
         return new CustomLogger(loggingConfig);
      }
   }
}