using System.Web.Http;

namespace HMS.HealthTrack.Web
{
	public class FormatterConfig
	{
		public static void SetupJson(HttpConfiguration config)
		{
			//Setup the json formatter needed for EF entities
			var json = config.Formatters.JsonFormatter;
			config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
			json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
			config.Formatters.Remove(config.Formatters.XmlFormatter);
			config.Formatters.Add(json);
		}
	}
}