using Blocks.Core;
using EmailService.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Empty;

public static class MailServiceRegistration
{
		public static IServiceCollection AddEmptyEmailService(this IServiceCollection services, IConfiguration config)
		{
				services.AddAndValidateOptions<EmailOptions>(config);
				services.AddSingleton<IEmailService, EmptyEmailService>();

				return services;
		}
}
