using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Storage.Blobs;
using Blocks.Core;
using FileStorage.Contracts;
using Azure.Storage.Blobs.Models;

namespace FileStorage.AzureBlob;

public static class FileStorageRegistration
{
		public static IServiceCollection AddAzureFileStorage(this IServiceCollection services, IConfiguration config)
		{
				services.AddAndValidateOptions<AzureBlobFileStorageOptions>(config);
				var options = config.GetSectionByTypeName<AzureBlobFileStorageOptions>();

				services.AddSingleton(_ =>
				{
						var client = new BlobServiceClient(config.GetConnectionStringOrThrow(options.ConnectionStringName));
						var container = client.GetBlobContainerClient(options.ContainerName);

						container.CreateIfNotExists(PublicAccessType.None);
						return container;
				});

				services.AddSingleton<IFileService, FileService>();
				services.AddScoped<FileService>();

				return services;
		}
}
