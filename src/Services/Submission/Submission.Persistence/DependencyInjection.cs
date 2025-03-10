using Blocks.Core;
using Blocks.EntityFrameworkCore.Interceptors;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace Submission.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices (this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

				services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

				// decide if we need the same DBConnection/Transaction when we are saving the Timeline
				services.AddScoped<DbConnection>(provider =>
				{
						return new SqlConnection(connectionString);
				});
				services.AddDbContext<SubmissionDbContext>((provider, options) =>
				{
						var dbConnection = provider.GetRequiredService<DbConnection>();
						options.AddInterceptors(provider.GetServices<ISaveChangesInterceptor>());
						options.UseSqlServer(dbConnection);
				});
				
				services.AddScoped<TransactionProvider>();

				services.AddScoped(typeof(Repository<>)); // only Repository<>
				services.AddDerivedTypesOf(typeof(Repository<>)); // inherits from Repository<>
				services.AddScoped<AssetTypeRepository>(); // cached repository


				//services.AddScoped(typeof(Repository<>));
				//services.AddScoped<ArticleRepository>();
				//services.AddScoped<AssetRepository>();
				//services.AddScoped<AssetTypeRepository>();
				//services.AddScoped<PersonRepository>();

				services.AddHostedService<DatabaseCacheLoader>();

				return services;
    }
}
