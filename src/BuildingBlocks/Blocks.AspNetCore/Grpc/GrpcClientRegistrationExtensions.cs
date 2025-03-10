using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.Configuration;

namespace Blocks.AspNetCore.Grpc;

public static class GrpcClientRegistrationExtensions
{
		public static IHttpClientBuilder AddConfiguredGrpcClient1<TClient>(this IServiceCollection services, GrpcServicesOptions grpcOptions, string? serviceKey = null)
				where TClient : class
		{
				//serviceKey ??= typeof(TClient).Namespace?.Split('.').FirstOrDefault()?.ToLowerInvariant();
				serviceKey ??= typeof(TClient).Name.Replace("Client", "").Replace("Service", "");

				if (string.IsNullOrWhiteSpace(serviceKey) || !grpcOptions.Services.TryGetValue(serviceKey, out var serviceSettings))
				{
						throw new InvalidOperationException($"Missing GrpcService config for: {typeof(TClient).Name}");
				}

				var clientBuilder = services.AddGrpcClient<TClient>(o =>
				{
						o.Address = new Uri(serviceSettings.Url);
				});

#if DEBUG
				// Accept self-signed certs in dev only
				clientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
						new HttpClientHandler
						{
								ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
						});
#endif

				if (serviceSettings.EnableRetry == true)
				{
						clientBuilder.AddPolicyHandler(Policy<HttpResponseMessage>
								.Handle<RpcException>()
								.WaitAndRetryAsync(
										grpcOptions.Retry.Count,
										retryAttempt => TimeSpan.FromMilliseconds(grpcOptions.Retry.InitialDelayMs * retryAttempt)
								));
				}

				return clientBuilder;
		}

		public static IServiceCollection AddGrpcServiceSingleton2<TClient>(this IServiceCollection services, GrpcServicesOptions grpcOptions, string? serviceKey = null)
		{
				serviceKey ??= typeof(TClient).Name.Replace("Client", "").Replace("Service", "");

				if (string.IsNullOrWhiteSpace(serviceKey) || !grpcOptions.Services.TryGetValue(serviceKey, out var serviceSettings))
				{
						throw new InvalidOperationException($"Missing GrpcService config for: {typeof(TClient).Name}");
				}

				var channel = GrpcChannel.ForAddress(serviceSettings.Url);
				return services;
		}

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
												ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
										}
								});
						return channel.CreateGrpcService<TClient>();
				});

				return services;
		}
}
