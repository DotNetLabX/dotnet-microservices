namespace Review.Persistence.Data.Test;

public static class Seed
{
		public static void SeedTestData(this IServiceProvider services)
		{
				services.SeedTestData<ReviewDbContext>(context =>
				{
						context.SeedFromJsonFile<Journal>();
						context.SeedFromJsonFile<Person>();
				});
		}
}
