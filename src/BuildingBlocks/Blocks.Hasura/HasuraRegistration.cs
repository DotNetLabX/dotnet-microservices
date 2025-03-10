using Blocks.Core;
using Blocks.Hasura.Metadata;
using Blocks.Hasura.Query;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blocks.Hasura;

public static class HasuraRegistration
{
		public static IServiceCollection AddHasuraGraphQL(this IServiceCollection services, IConfiguration configuration)
		{
				var hasuraOptions = configuration.GetSectionByTypeName<HasuraOptions>();

				services.AddSingleton(_ =>
				{
						var jsonSerializerOptions = new JsonSerializerOptions
						{
								PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
								DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
						};

						var graphQLClientOptions = new GraphQLHttpClientOptions
						{
								EndPoint = new Uri(new Uri(hasuraOptions.BaseUrl), "v1/graphql")
						};

						var graphQLHttpClient = new GraphQLHttpClient(graphQLClientOptions, new SystemTextJsonSerializer(jsonSerializerOptions));

						//graphQLHttpClient.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hasuraOptions.AdminSecret);
						graphQLHttpClient.HttpClient.DefaultRequestHeaders.Add("x-hasura-admin-secret", hasuraOptions.AdminSecret);

						// add other headers if needed, like tenant id, etc.

						return graphQLHttpClient;
				});

				return services;
		}

		public static IServiceCollection AddHasuraMetadata(this IServiceCollection services, IConfiguration configuration)
		{
				var hasuraOptions = configuration.GetSectionByTypeName<HasuraOptions>();

				//var refitSettings = new RefitSettings(new SystemTextJsonContentSerializer(
				//		new JsonSerializerOptions
				//		{
				//				PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
				//				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
				//		}));

				var refitSettings = new RefitSettings(new NewtonsoftJsonContentSerializer(
						new JsonSerializerSettings
						{
								ContractResolver = new DefaultContractResolver
								{
										NamingStrategy = new SnakeCaseNamingStrategy()
								},
								NullValueHandling = NullValueHandling.Ignore
						}));

				services.AddRefitClient<IHasuraMetadataApi>(refitSettings)
						.ConfigureHttpClient((sp, client) =>
						{
								var options = sp.GetRequiredService<IOptions<HasuraOptions>>().Value;
								client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
								client.DefaultRequestHeaders.Add("x-hasura-admin-secret", options.AdminSecret);
						});

				services.AddRefitClient<IHasuraSqlApi>(refitSettings)
						.ConfigureHttpClient((sp, client) =>
						{
								var options = sp.GetRequiredService<IOptions<HasuraOptions>>().Value;
								client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
								client.DefaultRequestHeaders.Add("x-hasura-admin-secret", options.AdminSecret);
						});

				services.AddSingleton<HasuraMetadataService>();


				return services;
		}

}
