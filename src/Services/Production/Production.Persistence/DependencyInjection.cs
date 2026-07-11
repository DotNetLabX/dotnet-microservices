using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Production.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        services.AddScoped<ISaveChangesInterceptor, TransactionalDispatchDomainEventsInterceptor>();

        // decide if we need the same DBConnection/Transaction when we are saving the Timeline
        services.AddScoped<DbConnection>(provider =>
        {
            return new SqlConnection(connectionString);
        });
        services.AddDbContext<ProductionDbContext>((provider, options) =>
        {
            var dbConnection = provider.GetRequiredService<DbConnection>();
            options.AddInterceptors(provider.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(dbConnection);
        });

        // decide if we need the same DBConnection/Transaction when we are saving the Timeline
        services.AddScoped<TransactionProvider>();

        // ArticleTimeline registers its own DbContext — see AddArticleTimelinePersistence().

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<ArticleRepository>();
        services.AddScoped<AssetRepository>();
        services.AddScoped<AssetTypeRepository>();
        services.AddScoped<FileRepository>();

        return services;
    }
}
