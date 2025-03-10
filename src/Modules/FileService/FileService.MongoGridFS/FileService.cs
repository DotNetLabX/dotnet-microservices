using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Microsoft.AspNetCore.Http;
using FileStorage.Contracts;
using Microsoft.Extensions.Options;

namespace FileStorage.MongoGridFS;

public class FileService(GridFSBucket bucket, IOptions<MongoGridFsFileStorageOptions> options) 
		: FileService<MongoGridFsFileStorageOptions>(bucket, options);

public class FileService<TFileStorageOptions> : IFileService<TFileStorageOptions>
		where TFileStorageOptions: MongoGridFsFileStorageOptions
{
		private readonly GridFSBucket _bucket;
		private readonly TFileStorageOptions _options;

		private const string DefaultContentType = "application/octet-stream";

		public FileService(GridFSBucket bucket, IOptions<TFileStorageOptions> options)
				=> (_bucket, _options) = (bucket, options.Value);

		public string GenerateId() => ObjectId.GenerateNewId().ToString();

		public async Task<FileMetadata> UploadAsync(string storagePath, IFormFile file, bool overwrite = false, Dictionary<string, string>? tags = null, CancellationToken ct = default)
		{
				using var stream = file.OpenReadStream();

				var request = new FileUploadRequest(storagePath, file.FileName, file.ContentType, file.Length);

				return await UploadInternalAsync(request, stream, overwrite, tags, ct);
		}

		public async Task<FileMetadata> UploadAsync(FileUploadRequest request, Stream stream, bool overwrite = false, Dictionary<string, string>? tags = null, CancellationToken ct = default)
		{
				try { request = request with { FileSize = stream.Length }; }
				catch (NotSupportedException) 
				{ 
						if(request.FileSize <= 0) // it means we don't have the length here so we cannot continue
								throw new InvalidOperationException("Stream does not support Length. Cannot validate file size."); 
				}

				return await UploadInternalAsync(request, stream, overwrite, tags, ct);
		}

		private async Task<FileMetadata> UploadInternalAsync(FileUploadRequest request, Stream stream, bool overwrite, Dictionary<string, string>? tags, CancellationToken ct)
		{
				if (request.FileSize > _options.FileSizeLimitInBytes)
						throw new InvalidOperationException($"File exceeds maximum allowed size of {_options.FileSizeLimitInMB} MB.");

				if (overwrite)
						await TryDeleteAsync(request.StoragePath);

				var metadata = new BsonDocument(tags ?? new Dictionary<string, string>())
				{
						{ nameof(FileMetadata.StoragePath), request.StoragePath },
						{ nameof(FileMetadata.FileName), request.FileName },
						{ nameof(FileMetadata.ContentType), request.ContentType ?? DefaultContentType }
				};

				var uploadOptions = new GridFSUploadOptions
				{
						Metadata = metadata,
						ChunkSizeBytes = _options.ChunkSizeBytes
				};

				var fileId = await _bucket.UploadFromStreamAsync(request.FileName, stream, uploadOptions, ct);

				return new FileMetadata(
						StoragePath: request.StoragePath,
						FileName: request.FileName,
						ContentType: request.ContentType ?? DefaultContentType,
						FileSize: request.FileSize,
						FileId: fileId.ToString()
				);
		}


		public async Task<IEnumerable<string>> FindFileIdsByTagAsync(string key, string value, CancellationToken ct = default)
		{
				var filter = Builders<GridFSFileInfo>.Filter.Eq($"metadata.{key}", value);
				var fileInfos = await _bucket.Find(filter, cancellationToken:ct).ToListAsync(ct);

				return fileInfos.Select(f => f.Id.ToString());
		}

		public async Task<bool> TryDeleteByTagAsync(string key, string value, CancellationToken ct = default)
		{
				var filter = Builders<GridFSFileInfo>.Filter.Eq($"metadata.{key}", value);
				var fileInfo = await _bucket.Find(filter, cancellationToken: ct).FirstOrDefaultAsync(ct);
				if (fileInfo == null)
						return false;

				await _bucket.DeleteAsync(fileInfo.Id, ct);
				return true;
		}

		public async Task<bool> TryDeleteAsync(string fileId, CancellationToken ct = default)
		{
				if (!ObjectId.TryParse(fileId, out var objectId))
						return false;

				try
				{
						await _bucket.DeleteAsync(objectId, ct);
						return true;
				}
				catch (GridFSFileNotFoundException) { return false; }
		}

		public async Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadAsync(string fileId, CancellationToken ct = default)
		{
				if (!ObjectId.TryParse(fileId, out var objectId))
						throw new FileNotFoundException($"Invalid file ID: {fileId}");

				return await DownloadByIdAsync(objectId, ct);
		}

		public async Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadByTagAsync(string key, string value, CancellationToken ct = default)
		{
				var filter = Builders<GridFSFileInfo>.Filter.Eq($"metadata.{key}", value);
				var fileInfo = await _bucket.Find(filter, cancellationToken: ct).FirstOrDefaultAsync(ct);

				if (fileInfo == null)
						throw new FileNotFoundException($"No file found with tag [{key}={value}]");

				return await DownloadByIdAsync(fileInfo.Id);
		}

		private async Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadByIdAsync(ObjectId fileId, CancellationToken ct = default)
		{
				var fileInfo = await _bucket.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", fileId), cancellationToken: ct).FirstOrDefaultAsync(ct);
				if (fileInfo == null)
						throw new FileNotFoundException($"No file found with ID: {fileId}");

				var stream = await _bucket.OpenDownloadStreamAsync(fileId, cancellationToken: ct);

				var md = fileInfo.Metadata;

				var fileMetadata = new FileMetadata(
						StoragePath: md[nameof(FileMetadata.StoragePath)].AsString,
						FileName: md[nameof(FileMetadata.FileName)].AsString,
						ContentType: md[nameof(FileMetadata.ContentType)].AsString,
						FileSize: fileInfo.Length,
						FileId: fileId.ToString()
						);

				return (stream, fileMetadata);
		}
}

