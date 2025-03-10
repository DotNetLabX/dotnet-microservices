using Auth.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Persistence;
public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices (this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Database");
				services.AddDbContext<AuthDbContext>(opts => opts.UseSqlServer(connectionString));

        services.AddScoped<PersonRepository>();
				services.AddScoped<UserRepository>();

				return services;
    }
}
