using AutoMapper;

namespace HMS.HealthTrack.Web.Infrastructure
{
	public interface IHaveCustomMappings
	{
		void CreateMappings(IConfiguration configuration);
	}
}