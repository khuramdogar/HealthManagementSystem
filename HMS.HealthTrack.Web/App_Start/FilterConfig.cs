using System.Web.Mvc;

namespace HMS.HealthTrack.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
	        filters.Add(new ExceptionLogger());
			  filters.Add(new HandleErrorAttribute());
        }
    }
}
