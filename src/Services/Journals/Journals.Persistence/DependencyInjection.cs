using Blocks.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Journals.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Database");

        services.AddSingleton(new RedisConnectionProvider(connectionString!));
        var redis = ConnectionMultiplexer.Connect(connectionString!.Replace("redis://", ""));
        services.AddSingleton<IConnectionMultiplexer>(redis);

        services.AddSingleton<JournalDbContext>();
        services.AddScoped(typeof(Repository<>));

        return services;
    }
}
