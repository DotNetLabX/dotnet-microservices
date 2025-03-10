using Articles.Security;
using Blocks.Core;
using Blocks.Mapster;
using Blocks.Messaging;
using Carter;
using Microsoft.AspNetCore.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using Blocks.Messaging.MassTransit;
using Blocks.Core.Security;
using Blocks.AspNetCore;
using Blocks.Core.Context;
using Blocks.Hasura;

namespace ArticleHub.API;

public static class DependecyInjection
{
		public static void ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
		{
				services
						.AddAndValidateOptions<RabbitMqOptions>(configuration)
						.AddAndValidateOptions<HasuraOptions>(configuration)
						.Configure<JsonOptions>(opt =>
						{
								opt.SerializerOptions.PropertyNameCaseInsensitive = true;
								opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
						});
		}

		public static IServiceCollection AddApiAndApplicationServices(this IServiceCollection services, IConfiguration configuration)
		{
				// API + Infra
				services
						.AddCarter()
						.AddHttpContextAccessor()                // For accessing HTTP context
						.AddEndpointsApiExplorer()               // Minimal API docs (Swagger)
						.AddSwaggerGen()                         // Swagger setup
						.AddJwtAuthentication(configuration)     // JWT Authentication
						.AddAuthorization();                     // Authorization configuration

				// Application
				services
						.AddMemoryCache()
						.AddMapsterConfigsFromCurrentAssembly()
						.AddMassTransitWithRabbitMQ(configuration, Assembly.GetExecutingAssembly());



				//// http
				services
						.AddScoped<IClaimsProvider, HttpContextProvider>()
						.AddScoped<IRouteProvider, HttpContextProvider>()
						.AddScoped<HttpContextProvider>();

				services.AddScoped<RequestContext>();

				//// authorization
				//services
				//		.AddScoped<IAuthorizationHandler, ArticleRoleAuthorizationHandler>()
				//		.AddScoped<IArticleRoleChecker, ContributorRepository>();

				return services;
		}
}
