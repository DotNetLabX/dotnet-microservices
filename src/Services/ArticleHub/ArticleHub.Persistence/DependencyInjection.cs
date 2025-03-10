using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ArticleHub.Persistence.Repositories;
using Blocks.Hasura;

namespace ArticleHub.Persistence;

public static class DependencyInjection
{
		public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
		{
				services.AddDbContext<ArticleHubDbContext>(options 
						=> options.UseNpgsql(configuration.GetConnectionString("Database")));

				services.AddHasuraGraphQL(configuration);

				services.AddHasuraMetadata(configuration);
				services.AddHostedService<HasuraMetadataInitService>();


				services.AddScoped<ArticleGraphQLReadStore>();
				services.AddScoped(typeof(Repository<>));
				services.AddScoped<ArticleRepository>();

				return services;
		}
}
