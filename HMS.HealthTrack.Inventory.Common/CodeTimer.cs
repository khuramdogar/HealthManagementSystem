using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Serilog;

namespace HMS.HealthTrack.Inventory.Common
{
   /// <summary>
   /// </summary>
   public class CodeTimer : IDisposable
   {
      readonly string identifier;

      QueryPerfCounter qpc;

      public CodeTimer()
      {
         qpc = new QueryPerfCounter();
         qpc.Start();
      }

      public CodeTimer(string id)
      {
         identifier = id;
         qpc = new QueryPerfCounter();
         qpc.Start();
      }
      #region IDisposable Members

      public void Dispose()
      {
         qpc.Stop();
         double time = qpc.Duration(1);
         //Log.Message(Log.LogLevel.Warning, this, "{0} Time elapsed is {1} nanoseconds.", identifier, time.ToString());
         Log.Logger.Debug("{0} Time elapsed is {1} ms.", identifier, (time / 1000000).ToString("0."));
      }

      #endregion
   }

   public class QueryPerfCounter
   {
      [DllImport("KERNEL32")]
      private static extern bool QueryPerformanceCounter(
         out long lpPerformanceCount);

      [DllImport("Kernel32.dll")]
      private static extern bool QueryPerformanceFrequency(out long lpFrequency);

      private long start;
      private long stop;
      private long frequency;
      Decimal multiplier = new Decimal(1.0e9);

      public QueryPerfCounter()
      {
         if (QueryPerformanceFrequency(out frequency) == false)
         {
            // Frequency not supported
            throw new Win32Exception();
         }
      }

      public void Start()
      {
         QueryPerformanceCounter(out start);
      }

      public void Stop()
      {
         QueryPerformanceCounter(out stop);
      }

      public double Duration(int iterations)
      {
         return ((((double)(stop - start) * (double)multiplier) / (double)frequency) / iterations);
      }
   }
}
