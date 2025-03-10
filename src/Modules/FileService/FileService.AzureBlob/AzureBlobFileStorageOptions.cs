using FileStorage.Contracts;

namespace FileStorage.AzureBlob;

public class AzureBlobFileStorageOptions : IFileStorageOptions
{
		public string ConnectionStringName { get; init; } = default!;
		public string ContainerName { get; init; } = default!;
		public long FileSizeLimitInMB { get; init; } = 50;

		public long FileSizeLimitInBytes => FileSizeLimitInMB * 1024 * 1024;
}
