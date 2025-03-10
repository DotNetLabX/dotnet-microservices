using Microsoft.AspNetCore.Http;

namespace FileStorage.Contracts;


// we need this version of the interface so we can include multiple file storages into a single microservice. TFileStorageOptions will allow that.
public interface IFileService<TFileStorageOptions> : IFileService
		where TFileStorageOptions : IFileStorageOptions;

public interface IFileService
{
		string GenerateId();
		Task<FileMetadata> UploadAsync(string storagePath, IFormFile file, bool overwrite = false, Dictionary<string, string>? tags = null, CancellationToken ct = default);
		Task<FileMetadata> UploadAsync(FileUploadRequest request, Stream stream, bool overwrite = false, Dictionary<string, string>? tags = null, CancellationToken ct = default);

		Task<IEnumerable<string>> FindFileIdsByTagAsync(string key, string value, CancellationToken ct = default);

		Task<bool> TryDeleteAsync(string fileId, CancellationToken ct = default);
		Task<bool> TryDeleteByTagAsync(string key, string value, CancellationToken ct = default);

		Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadAsync(string fileId, CancellationToken ct = default);
		Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadByTagAsync(string key, string value, CancellationToken ct = default);
}


public record FileUploadRequest(string StoragePath, string FileName, string ContentType, long FileSize = default);

public record FileMetadata(string StoragePath, string FileName, string ContentType, long FileSize, string FileId);

public interface IFileStorageOptions; // marker interface