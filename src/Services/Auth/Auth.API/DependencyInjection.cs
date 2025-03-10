using Articles.Security;
using Auth.API;
using Auth.API.Mappings;
using Auth.Persistence;
using Blocks.AspNetCore;
using Blocks.Core.Security;
using Blocks.Domain;
using Blocks.FastEndpoints;
using EmailService.Empty;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using ProtoBuf.Grpc.Server;
using System.IO.Compression;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Auth.Api;

public static class DependenciesConfiguration
{
		public static IServiceCollection ConfigureApiOptions(this IServiceCollection services, IConfiguration configuration)
		{
				services
						.AddAndValidateOptions<JwtOptions>(configuration)
						.ConfigureOptions<PostConfigureJwtBearerOptions>()
						.Configure<JsonOptions>(opt =>
						{
								opt.SerializerOptions.PropertyNameCaseInsensitive = true;
								opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
						});

				return services;
		}

		public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
		{
				services
						.AddFastEndpoints()
						.SwaggerDocument()
						.AddEndpointsApiExplorer()											// Minimal API docs (Swagger)
						.AddSwaggerGen()																// Swagger setup
						.AddJwtIdentity(config)
						.AddJwtAuthentication(config)										// JWT Authentication
						.AddAuthorization();                            // Authorization configuration

				services
						.AddScoped<IClaimsProvider, HttpContextProvider>()
						.AddScoped<IRouteProvider, HttpContextProvider>()
						.AddScoped<HttpContextProvider>();

				services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

				services.AddSingleton<GrpcTypeAdapterConfig>();			//mapster mappings for grpc contracts

				services.AddCodeFirstGrpc(options =>
				{
						options.ResponseCompressionLevel = CompressionLevel.Fastest;
						options.EnableDetailedErrors = true;
				});

				// replace empty email service with the real smtp one after configuring smtp settings in appsettings.json
				services.AddEmptyEmailService(config);
				//services.AddSmtpEmailService(config);

				return services;
		}


		public static IServiceCollection AddJwtIdentity(this IServiceCollection services, IConfiguration configuration)
		{
				services.AddIdentityCore<User>(options =>
				{
						// Lockout settings
						options.Lockout.AllowedForNewUsers = true;
						options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
						options.Lockout.MaxFailedAccessAttempts = 5;

						//options.User.RequireUniqueEmail = false; //to-do - change back to true after test training users not needed anymore
						//options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";
				})
			  .AddRoles<Auth.Domain.Roles.Role>()
				.AddEntityFrameworkStores<AuthDbContext>()
				.AddSignInManager<SignInManager<User>>()
				.AddDefaultTokenProviders();

				services.Configure<IdentityOptions>(options =>
				{
						options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
				});

				return services;
		}
}
