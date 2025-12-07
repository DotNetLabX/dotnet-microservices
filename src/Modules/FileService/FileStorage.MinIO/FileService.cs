using FileStorage.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Tags;

namespace FileStorage.MinIO;

// todo - not tested, just a prototype to show how it can be done with MinIO
public class FileService : IFileService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioFileStorageOptions _options;
    private const string DefaultContentType = "application/octet-stream";

    public FileService(IMinioClient minioClient, IOptions<MinioFileStorageOptions> options)
        => (_minioClient, _options) = (minioClient, options.Value);

    public string GenerateId() => Guid.NewGuid().ToString();

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
            if (request.FileSize <= 0)
                throw new InvalidOperationException("Stream does not support Length. Cannot validate file size.");
        }

        return await UploadInternalAsync(request, stream, overwrite, tags, ct);
    }

    private async Task<FileMetadata> UploadInternalAsync(FileUploadRequest request, Stream stream, bool overwrite, Dictionary<string, string>? tags, CancellationToken ct)
    {
        if (request.FileSize > _options.FileSizeLimitInBytes)
            throw new InvalidOperationException($"File exceeds maximum allowed size of {_options.FileSizeLimitInMB} MB.");

        // Delete if overwrite
        if (overwrite)
            await TryDeleteAsync(request.StoragePath, ct);

        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(request.StoragePath)
            .WithStreamData(stream)
            .WithObjectSize(request.FileSize)
            .WithContentType(request.ContentType ?? DefaultContentType)
            .WithHeaders(new Dictionary<string, string>
            {
                ["x-amz-meta-filename"] = request.FileName
            })
            .WithTagging(new Tagging(tags, false)), ct);

        return new FileMetadata(
            StoragePath: request.StoragePath,
            FileName: request.FileName,
            ContentType: request.ContentType ?? DefaultContentType,
            FileSize: request.FileSize,
            FileId: request.StoragePath
        );
    }

    public async Task<bool> TryDeleteAsync(string fileId, CancellationToken ct = default)
    {
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(fileId), ct);
            return true;
        }
        catch (Exception) { return false; }
    }

    public async Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadAsync(string fileId, CancellationToken ct = default)
    {
        var memStream = new MemoryStream();

        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(fileId)
            .WithCallbackStream(s => s.CopyTo(memStream)), ct);

        memStream.Position = 0;

        var stat = await _minioClient.StatObjectAsync(new StatObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(fileId), ct);

        return (memStream, new FileMetadata(
            StoragePath: fileId,
            FileName: stat.MetaData["x-amz-meta-filename"],
            ContentType: stat.ContentType,
            FileSize: stat.Size,
            FileId: fileId));
    }

    public Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadByTagAsync(string key, string value, CancellationToken ct = default)
        => throw new NotImplementedException(); // MinIO does not support tag-based search natively

    public Task<bool> TryDeleteByTagAsync(string key, string value, CancellationToken ct = default)
        => throw new NotImplementedException(); // Same limitation

    public Task<IEnumerable<string>> FindFileIdsByTagAsync(string key, string value, CancellationToken ct = default)
        => throw new NotImplementedException(); // Same limitation
}
