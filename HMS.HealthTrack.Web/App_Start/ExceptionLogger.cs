using System.Web.Mvc;
using Serilog;

namespace HMS.HealthTrack.Web
{
   public class ExceptionLogger : FilterAttribute, IExceptionFilter
   {
      public void OnException(ExceptionContext filterContext)
      {
         Log.Error(filterContext.Exception, "Unhandled exception in " + filterContext.Controller);
      }
   }
}