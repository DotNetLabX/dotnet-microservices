using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ArticleTimeline.Persistence;
using ArticleTimeline.Application.VariableResolvers;

namespace ArticleTimeline.Application;
public static class DependencyInjection
{
		public static IServiceCollection AddArticleTimeline(this IServiceCollection services, IConfiguration config)
		{
				services.AddArticleTimelineApplication(config);
				services.AddArticleTimelinePersistence(config);
				return services;
		}

		public static IServiceCollection AddArticleTimelineApplication(this IServiceCollection services, IConfiguration configuration)
    {
				services.AddMediatR(config =>
				{
						config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
				});

				services.AddArticleTimelineVariableResolvers();
				return services;
    }
}
