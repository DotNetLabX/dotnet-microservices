using Blocks.AspNetCore;
using Blocks.Core;
using Blocks.Core.Security;
using Blocks.Domain;
using Blocks.EntityFrameworkCore;
using Blocks.FastEndpoints;
using FastEndpoints.Swagger;
using FileStorage.AzureBlob;
using Microsoft.AspNetCore.Http.Json;
using Production.Application;
using Production.Persistence.Repositories;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Production.API;

public static class DependecyInjection
{
    public static void ConfigureApiOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
        //.AddAndValidateOptions<FileStorage.Contracts.FileServerOptions>(configuration)
        .AddAndValidateOptions<TransactionOptions>(configuration)
        .Configure<JsonOptions>(opt =>
        {
            opt.SerializerOptions.PropertyNameCaseInsensitive = true;
            opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        //insight - fluid vs normal
        services.AddControllers();

        services
            .AddMemoryCache()
            .AddFastEndpoints()
            .AddMapster()
            .SwaggerDocument()
            .AddEndpointsApiExplorer()
            //.AddAutoMapper(new Assembly[] { typeof(Production.API.Features.Shared.FileResponseMappingProfile).Assembly })
            .AddDistributedMemoryCache() //.AddMemoryCache()
            .AddSwaggerGen()
            .AddJwtAuthentication(configuration)
            .AddAuthorization();

        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        //insight - SOLID principle interface segragation, injecting multiple interfaces using the same class
        services.AddScoped<HttpContextProvider>();
        services.AddScoped<IClaimsProvider, HttpContextProvider>();
        services.AddScoped<IRouteProvider, HttpContextProvider>();

        services.AddScoped<IAuthorizationHandler, ArticleAccessAuthorizationHandler>();

        // monolith modules registration 
        services.AddAzureFileStorage(configuration);

        return services;
    }
}