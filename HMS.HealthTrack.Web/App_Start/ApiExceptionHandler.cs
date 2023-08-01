using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Serilog;

namespace HMS.HealthTrack.Web.App_Start
{
   public class ApiExceptionLogger : BaseApiExceptionLogger
   {
      public override void LogCore(ExceptionLoggerContext context)
      {
         var requestUri = context.Request.RequestUri;
         Log.Error(context.Exception, "Unhandled exception in " + requestUri);
      }
   }

   public class BaseApiExceptionLogger : IExceptionLogger
   {
      public virtual Task LogAsync(ExceptionLoggerContext context,
                                   CancellationToken cancellationToken)
      {
         if (!ShouldLog(context))
         {
            return Task.FromResult(0);
         }

         return LogAsyncCore(context, cancellationToken);
      }

      public virtual Task LogAsyncCore(ExceptionLoggerContext context,
                                       CancellationToken cancellationToken)
      {
         LogCore(context);
         return Task.FromResult(0);
      }

      public virtual void LogCore(ExceptionLoggerContext context)
      {
      }

      public virtual bool ShouldLog(ExceptionLoggerContext context)
      {
         IDictionary exceptionData = context.ExceptionContext.Exception.Data;

         if (!exceptionData.Contains("MS_LoggedBy"))
         {
            exceptionData.Add("MS_LoggedBy", new List<object>());
         }

         ICollection<object> loggedBy = ((ICollection<object>)exceptionData["MS_LoggedBy"]);

         if (!loggedBy.Contains(this))
         {
            loggedBy.Add(this);
            return true;
         }
         else
         {
            return false;
         }
      }
   }
}