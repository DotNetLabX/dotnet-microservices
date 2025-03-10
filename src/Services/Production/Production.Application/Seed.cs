using Production.Domain.Assets;
using Production.Domain.Shared;
using Production.Domain.Articles;

namespace Production.Application;

public static class Seed
{
		public static void SeedTestData(this IServiceProvider services)
		{
				services.SeedTestData<ProductionDbContext>(context =>
				{
						context.SeedFromJsonFile<Person>();

						context.SeedFromJsonFile<Journal>();

						context.SeedFromJsonFile<Article>();

						context.SeedFromJsonFile<AssetCurrentFileLink>(); // this is a link between an asset and a file, which couldn't be included in Article seeding
				});
		}
}
