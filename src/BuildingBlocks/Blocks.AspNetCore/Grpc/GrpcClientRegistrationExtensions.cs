using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.Configuration;

namespace Blocks.AspNetCore.Grpc;

public static class GrpcClientRegistrationExtensions
{
    public static IServiceCollection AddCodeFirstGrpcClient<TClient>(this IServiceCollection services, GrpcServicesOptions grpcOptions, string? serviceKey = null)
        where TClient : class
    {
        serviceKey ??= typeof(TClient).Name.Replace("Client", "").Replace("Service", "");

        if (string.IsNullOrWhiteSpace(serviceKey)
            || !grpcOptions.Services.TryGetValue(serviceKey, out var serviceSettings))
        {
            throw new InvalidOperationException($"Missing GrpcService config for: {typeof(TClient).Name}");
        }

        services.AddScoped(sp =>
        {
            var channel = GrpcChannel.ForAddress(serviceSettings.Url,
                new GrpcChannelOptions
                {
                    HttpHandler = new HttpClientHandler
                    {
#if DEBUG
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
#endif
                    }
                });
            return channel.CreateGrpcService<TClient>();
        });

        return services;
    }
}
