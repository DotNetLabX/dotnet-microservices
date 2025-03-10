using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Blocks.Redis;

namespace Journals.Persistence.Data;

public static class Seed
{
    public static async Task SeedTestData(this IHost host)
    {
        using var scope = host.Services.CreateScope();
				var provider = scope.ServiceProvider.GetRequiredService<RedisConnectionProvider>();
				var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer> ();
				await provider.SeedTestData(redis.GetDatabase());
    }

    public static async Task SeedTestData(this RedisConnectionProvider provider, IDatabase redisDb)
    {
				await provider.SeedFromJson<Editor>(redisDb);
				await provider.SeedFromJson<Journal>(redisDb);

				//we need to set the sequence seed to avoid duplicate key errors because of the seeded records
				await redisDb.SetSequenceSeed<Journal>(7);
				await redisDb.SetSequenceSeed<Section>(14);
		}
}
