using Blocks.Core;
using EmailService.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Smtp;

public static class MailServiceRegistration
{
    public static IServiceCollection AddSmtpEmailService(this IServiceCollection services, IConfiguration config)
    {
        services.AddAndValidateOptions<EmailOptions>(config);
        services.AddSingleton<IEmailService, SmtpEmailService>();

        return services;
    }
}
