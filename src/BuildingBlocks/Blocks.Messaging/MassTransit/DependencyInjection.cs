using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Blocks.Core;
using Blocks.Core.Extensions;

namespace Blocks.Messaging.MassTransit;

public static class DependencyInjection
{
		public static IServiceCollection AddMassTransitWithRabbitMQ
				(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
		{
				var rabbitMqOptions = configuration.GetSectionByTypeName<RabbitMqOptions>();

				var serviceName = assembly.GetServiceName();

				services.AddMassTransit(config =>
				{
						config.SetEndpointNameFormatter(
								new SnakeCaseWithServiceSuffixNameFormatter(serviceName)
						);

						if (assembly != null)
								config.AddConsumers(assembly);

						config.UsingRabbitMq((context, rabbitConfig) =>
						{
								// Configure RabbitMQ connection
								rabbitConfig.Host(new Uri(rabbitMqOptions.Host), rabbitMqOptions.VirtualHost, h =>
								{
										h.Username(rabbitMqOptions.UserName);
										h.Password(rabbitMqOptions.Password);
								});

								rabbitConfig.ConfigureEndpoints(context);
						});
				});


				return services;
		}
}
