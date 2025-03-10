using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FileStorage.AzureBlob;

public static class BlobClientExtensions
{
		public static Task<Response<BlobContentInfo>> UploadAsync(
				this BlobClient blob,
				Stream content,
				BlobHttpHeaders? httpHeaders = null,
				IDictionary<string, string>? metadata = null,
				bool overwrite = false,
				CancellationToken ct = default)
		{
				var options = new BlobUploadOptions
				{
						HttpHeaders = httpHeaders,
						Metadata = metadata
				};

				if (!overwrite)
				{
						// very strange implementation
						options.Conditions = new BlobRequestConditions
						{
								IfNoneMatch = ETag.All
						};
				}

				return blob.UploadAsync(content, options, ct);
		}
}
