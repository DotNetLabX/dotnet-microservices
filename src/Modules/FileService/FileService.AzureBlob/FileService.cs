using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Blocks.Core;
using FileStorage.Contracts;

namespace FileStorage.AzureBlob;

public class FileService : IFileService
{
		private readonly BlobContainerClient _containerClient;
		private readonly AzureBlobFileStorageOptions _options;

		private const string DefaultContentType = "application/octet-stream";

		public FileService(BlobContainerClient containerClient, IOptions<AzureBlobFileStorageOptions> options)
				=> (_containerClient, _options) = (containerClient, options.Value);

		public string GenerateId() => Guid.NewGuid().ToString();

		public async Task<FileMetadata> UploadAsync(string storagePath, IFormFile file, bool overwrite = false, Dictionary<string, string>? tags = null, CancellationToken ct = default)
		{
				using var stream = file.OpenReadStream();

				var request = new FileUploadRequest(storagePath, file.FileName, file.ContentType, file.Length);
				return await UploadInternalAsync(request, stream, overwrite, tags, ct);
		}

		public async Task<FileMetadata> UploadAsync(FileUploadRequest request, Stream stream, bool overwrite = false, Dictionary<string, string>? tags = null, CancellationToken ct = default)
		{
				try
				{
						request = request with { FileSize = stream.Length };
				}
				catch (NotSupportedException)
				{
						if (request.FileSize <= 0)
								throw new InvalidOperationException("Stream does not support Length. Cannot validate file size.");
				}

				return await UploadInternalAsync(request, stream, overwrite, tags, ct);
		}

		private async Task<FileMetadata> UploadInternalAsync(FileUploadRequest request, Stream stream, bool overwrite, Dictionary<string, string>? tags, CancellationToken ct)
		{
				if (request.FileSize > _options.FileSizeLimitInBytes)
						throw new InvalidOperationException($"File exceeds maximum allowed size of {_options.FileSizeLimitInMB} MB.");

				var blob = _containerClient.GetBlobClient(request.StoragePath);

				var headers = new BlobHttpHeaders
				{
						ContentType = request.ContentType ?? DefaultContentType,
						ContentDisposition = $"attachment; filename=\"{request.FileName}\""
				};

				var metadata = new Dictionary<string, string>
				{
						[nameof(FileMetadata.FileName)] = request.FileName
				};

				await blob.UploadAsync(stream, headers, metadata, overwrite, ct);

				if (!tags.IsNullOrEmpty())
						await blob.SetTagsAsync(tags, cancellationToken: ct);

				return new FileMetadata(
						StoragePath: request.StoragePath,
						FileName: request.FileName,
						ContentType: request.ContentType ?? DefaultContentType,
						FileSize: request.FileSize,
						FileId: request.StoragePath
				);
		}


		public async Task<bool> TryDeleteAsync(string filePath, CancellationToken ct = default)
		{
				if (filePath.IsNullOrEmpty())
						return false;

				try
				{
						var blob = _containerClient.GetBlobClient(filePath);
						return await blob.DeleteIfExistsAsync(cancellationToken: ct);
				}
				catch (Exception) { return false; }
		}

		public async Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadAsync(string storagePath, CancellationToken ct = default)
		{
				var blob = _containerClient.GetBlobClient(storagePath);

				if (!await blob.ExistsAsync(ct))
						throw new FileNotFoundException($"File '{storagePath}' not found in container '{_options.ContainerName}'.");

				var props = await blob.GetPropertiesAsync(cancellationToken: ct);
				string fileName = props.Value.Metadata[nameof(FileMetadata.FileName).ToLowerInvariant()]; // you can use TryGet her, but the FileName metadata will aways exist

				BlobDownloadInfo download = await blob.DownloadAsync(ct);
				return (download.Content, new FileMetadata(storagePath, fileName, ContentType: props.Value.ContentType, download.Content.Length, storagePath));
		}

		public Task<bool> TryDeleteByTagAsync(string key, string value, CancellationToken ct = default)
		{
				//todo implement
				throw new NotImplementedException();
		}

		public Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadByTagAsync(string key, string value, CancellationToken ct = default	)
		{
				//todo implement
				throw new NotImplementedException();
		}

		public async Task<IEnumerable<string>> FindFileIdsByTagAsync(string key, string value, CancellationToken ct = default)
		{
				var query = $"@tag.{key} = '{value}'";
				var results = new List<string>();

				await foreach (var blobItem in _containerClient.FindBlobsByTagsAsync(query, ct))
				{
						results.Add(blobItem.BlobName); // this is your fileId
				}

				return results;
		}
}
