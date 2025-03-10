using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Application;

public static class DependenciesConfiguration
{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
		{
				services.AddSingleton<TokenFactory>();

				return services;
		}
}