using Articles.Security;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Production.Application.StateMachines;
using Production.Domain.StateMachines;
using System.Reflection;

namespace Production.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices (this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddFeatureManagement();

        services.AddScoped<IArticleAccessChecker, ArticleAccessChecker>();

        services.AddScoped<ArticleStateMachineFactory>(provider => articleStage =>
        {
            var dbContext = provider.GetRequiredService<ProductionDbContext>();
            return new ArticleStateMachine(articleStage, dbContext);
        });
        
        //services.AddScoped<AssetStateMachine>();
        services.AddScoped<AssetStateMachineFactory>(provider => assetState =>
        {
            var dbConntext = provider.GetRequiredService<ProductionDbContext>();
            return new AssetStateMachine(assetState, dbConntext);
        });

        return services;
    }

    public static IServiceCollection AddMapster(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetEntryAssembly()!);
        return services;
    }
}
