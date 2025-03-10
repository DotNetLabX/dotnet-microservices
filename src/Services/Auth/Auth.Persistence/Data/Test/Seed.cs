using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;

namespace Auth.Persistence.Data.Test;

public static class Seed
{
    public static void SeedTestData(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
				using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
				context.SeedTestData(userManager);
    }

    public static void SeedTestData(this AuthDbContext context, UserManager<User> userManager)
    {
        const string DefaultPassword = "Pass.123!";
        using var transaction = context.Database.BeginTransaction();

				var persons = context.LoadFromJson<Person>();

        context.UseManualGenerateId<User>(true);
				foreach (var person in persons) 
        {
            var user = person.User;
            person.User = null;
						context.Persons.Add(person);
            context.SaveChanges();

						if (user != null) {
                user.UserName = person.Email;
                user.Email = person.Email;
								user.PersonId = person.Id;
                user.Id = person.Id;

								var result = userManager.CreateAsync(user, DefaultPassword).GetAwaiter().GetResult();
								if (!result.Succeeded)
								{
										throw new Exception($"User creation failed for {person.Email}");
								}
                person.UserId = user.Id;
						}
				}

				context.SaveChanges();
				transaction.Commit();
    }
}
