using Articles.Security;
using Auth.Grpc;
using Blocks.AspNetCore.Grpc;
using Blocks.Core.Security;
using Blocks.Domain;
using Blocks.FastEndpoints;
using Blocks.Mapster;
using Blocks.Messaging.MassTransit;
using Microsoft.AspNetCore.Http.Json;
using ProtoBuf.Grpc.Server;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Journals.API;

public static class DependencyInjection
{
		public static IServiceCollection ConfigureApiOptions(this IServiceCollection services, IConfiguration configuration)
		{
				services
						.AddAndValidateOptions<JwtOptions>(configuration)
						.Configure<JsonOptions>(opt =>
						{
								opt.SerializerOptions.PropertyNameCaseInsensitive = true;
								opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
								//opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
						});

				return services;
		}

		public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
		{
				services
					.AddFastEndpoints()
					.AddEndpointsApiExplorer()
					.AddSwaggerGen()
					.AddJwtAuthentication(config)
					.AddMapsterConfigsFromCurrentAssembly()
					.AddAuthorization()
					.AddMassTransitWithRabbitMQ(config, Assembly.GetExecutingAssembly())
				;										

				services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

				// Grpc server
				services.AddCodeFirstGrpc(options =>
				{
						options.ResponseCompressionLevel = CompressionLevel.Fastest;
						options.EnableDetailedErrors = true;
				});

				// Grpc clients
				var grpcOptions = config.GetSectionByTypeName<GrpcServicesOptions>();
				services.AddCodeFirstGrpcClient<IPersonService>(grpcOptions, "Person");

				return services;
		}
}
