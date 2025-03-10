using System.Data.Common;
using ArticleTimeline.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArticleTimeline.Persistence;

public static class DependencyInjection
{
		public static IServiceCollection AddArticleTimelinePersistence(this IServiceCollection services, IConfiguration configuration)
		{
				services.AddDbContext<ArticleTimelineDbContext>((provider, options) =>
				{
						//options.AddInterceptors(provider.GetServices<ISaveChangesInterceptor>());
						var dbConnection = provider.GetRequiredService<DbConnection>();

						options.UseSqlServer(dbConnection, options =>
						{
								options.MigrationsHistoryTable("__EFMigrationsHistory", "ArticleTimeline");
						});
				});

				services.AddScoped<TimelineRepository>();
				return services;
		}
}
