namespace Submission.Persistence.Data.Test;

public static class Seed
{
    public static void SeedTestData(this IServiceProvider services)
		{
				services.SeedTestData<SubmissionDbContext>(context =>
				{
						context.SeedFromJsonFile<Person>();
						context.SeedFromJsonFile<Journal>();
				});
		}
}
