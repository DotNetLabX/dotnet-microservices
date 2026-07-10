using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
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

    //insight - showcased alternative to AddCodeFirstGrpcClient: gRPC-native retries via a MethodConfig RetryPolicy on the channel (a Polly handler at the HttpClient layer never sees RpcException); deliberately not wired by any service
    public static IServiceCollection AddCodeFirstGrpcClientWithRetry<TClient>(this IServiceCollection services, GrpcServicesOptions grpcOptions, string? serviceKey = null)
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
            var channelOptions = new GrpcChannelOptions
            {
                HttpHandler = new HttpClientHandler
                {
#if DEBUG
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
#endif
                }
            };

            if (serviceSettings.EnableRetry == true)
            {
                channelOptions.ServiceConfig = new ServiceConfig
                {
                    MethodConfigs =
                    {
                        new MethodConfig
                        {
                            Names = { MethodName.Default },
                            RetryPolicy = new RetryPolicy
                            {
                                MaxAttempts = grpcOptions.Retry.Count + 1,
                                InitialBackoff = TimeSpan.FromMilliseconds(grpcOptions.Retry.InitialDelayMs),
                                MaxBackoff = TimeSpan.FromSeconds(5),
                                BackoffMultiplier = 1.5,
                                RetryableStatusCodes = { StatusCode.Unavailable }
                            }
                        }
                    }
                };
            }

            var channel = GrpcChannel.ForAddress(serviceSettings.Url, channelOptions);
            return channel.CreateGrpcService<TClient>();
        });

        return services;
    }

    //insight - showcased alternative to AddCodeFirstGrpcClient: one channel + client pair shared for the whole process instead of created per scope; deliberately not wired by any service
    public static IServiceCollection AddCodeFirstGrpcClientAsSingleton<TClient>(this IServiceCollection services, GrpcServicesOptions grpcOptions, string? serviceKey = null)
        where TClient : class
    {
        serviceKey ??= typeof(TClient).Name.Replace("Client", "").Replace("Service", "");

        if (string.IsNullOrWhiteSpace(serviceKey)
            || !grpcOptions.Services.TryGetValue(serviceKey, out var serviceSettings))
        {
            throw new InvalidOperationException($"Missing GrpcService config for: {typeof(TClient).Name}");
        }

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

        services.AddSingleton(channel);
        services.AddSingleton(channel.CreateGrpcService<TClient>());

        return services;
    }
}
