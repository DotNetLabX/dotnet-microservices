# File Storage — Lifted Templates

Copy-paste starting points for `file-storage-patterns`. Every block is lifted from the repo; namespaces and option types are the only things you normally change. Row ids (filestorage-1..4, modules-3, consumers-4) tie each block back to the reference model.

---

## 1. The boundary contract (already in `FileService.Contracts` — do NOT redefine)

Only these types cross `IFileService` (filestorage-4). Provider-native types stay inside the adapter.

```csharp
namespace FileStorage.Contracts;

// Generic marker version — lets one service hold multiple IFileService instances (modules-3).
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
```

---

## 2. Options subclass per extra store (Phase 2 — the DI key)

Empty subclass; its only job is to be a distinct closed generic (modules-3, filestorage-1). One file per extra store, in the consuming service.

```csharp
// Review needs read access to Submission's GridFS bucket:
using FileStorage.MongoGridFS;

namespace Review.API.FileStorage;

public class SubmissionFileStorageOptions : MongoGridFsFileStorageOptions;
```

```csharp
// Production needs read access to Review's GridFS bucket:
using FileStorage.MongoGridFS;

namespace Production.API;

public class ReviewFileStorageOptions : MongoGridFsFileStorageOptions;
```

---

## 3. Registration extensions (Phase 3 — singleton default + scoped per extra)

These live in the module. The own store is a **singleton** (client + bucket reused); each extra store is **scoped** and keyed by its `TOptions` closed generic.

```csharp
// FileService.MongoGridFS/FileStorageRegistration.cs (module — reference only)

public static IServiceCollection AddMongoFileStorageAsSingletone(this IServiceCollection services, IConfiguration config)
    => services.AddMongoFileStorageAsSingletone<FileService, MongoGridFsFileStorageOptions>(config); // default registration

public static IServiceCollection AddMongoFileStorageAsSingletone<TService, TOptions>(this IServiceCollection services, IConfiguration config)
    where TService : FileService
    where TOptions : MongoGridFsFileStorageOptions
{
    services.AddAndValidateOptions<TOptions>(config);
    var options = config.GetSectionByTypeName<TOptions>();

    services.AddSingleton<IMongoClient>(sp => new MongoClient(config.GetConnectionStringOrThrow(options.ConnectionStringName)));
    services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(options.DatabaseName));
    services.AddSingleton(sp =>
    {
        var db = sp.GetRequiredService<IMongoDatabase>();
        return new GridFSBucket(db, new GridFSBucketOptions
        {
            BucketName = options.BucketName,
            ChunkSizeBytes = options.ChunkSizeBytes,
            WriteConcern = WriteConcern.WMajority,
            ReadPreference = ReadPreference.Primary
        });
    });

    services.AddSingleton<IFileService, TService>();
    services.AddSingleton<TService>();
    return services;
}

// TOptions is mandatory so DI can register more than one IFileService (modules-3, filestorage-1).
public static IServiceCollection AddMongoFileStorageAsScoped<TOptions>(this IServiceCollection services, IConfiguration config)
    where TOptions : MongoGridFsFileStorageOptions
{
    services.AddAndValidateOptions<TOptions>(config);

    services.AddScoped<IFileService<TOptions>>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<TOptions>>();
        var optValue = options.Value;
        var client = new MongoClient(config.GetConnectionStringOrThrow(optValue.ConnectionStringName));
        var db = client.GetDatabase(optValue.DatabaseName);
        var bucket = new GridFSBucket(db, new GridFSBucketOptions
        {
            BucketName = optValue.BucketName,
            ChunkSizeBytes = optValue.ChunkSizeBytes,
            WriteConcern = WriteConcern.WMajority,
            ReadPreference = ReadPreference.Primary
        });
        return new FileService<TOptions>(bucket, options);
    });
    return services;
}
```

```csharp
// FileService.AzureBlob/FileStorageRegistration.cs (module — reference only)
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
```

Per-service wiring (in each `DependencyInjection.cs`):

```csharp
// Submission — one store:
services.AddMongoFileStorageAsSingletone(config);

// Review — own + Submission read + runtime factory:
services.AddMongoFileStorageAsSingletone(config);
services.AddMongoFileStorageAsScoped<SubmissionFileStorageOptions>(config);
services.AddFileServiceFactory();

// Production — own Azure + Review read:
services.AddAzureFileStorage(configuration);
services.AddMongoFileStorageAsScoped<ReviewFileStorageOptions>(configuration);
```

---

## 4. Upload handler with compensating delete (Phase 4 — filestorage-3)

### MediatR variant (Submission / Review `_Shared/UploadFileCommandHandler.cs`)

```csharp
using FileStorage.Contracts;

public class UploadFileCommandHandler<TUploadCommand>
    (ArticleRepository _articleRepository, AssetTypeDefinitionRepository _assetTypeRepository, IFileService _fileService, ArticleStateMachineFactory _stateMachineFactory)
    : IRequestHandler<TUploadCommand, IdResponse>
        where TUploadCommand : UploadFileCommand
{
    protected Article _article = null!;

    public virtual async Task<IdResponse> Handle(TUploadCommand command, CancellationToken ct)
    {
        _article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId);

        var assetType = _assetTypeRepository.GetById(command.AssetType);
        var asset = GetOrCreateAsset(assetType, command);

        var fileMetada = await UploadFile(command, asset, assetType, ct); // upload FIRST — outside the SQL transaction

        try
        {
            asset.CreateFile(fileMetada, assetType, command);
            _article.SetStage(NextStage, _stateMachineFactory, command);

            await _articleRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            await _fileService.TryDeleteAsync(fileMetada.StoragePath); // delete the file if something is wrong
            throw;
        }

        return new IdResponse(asset.Id);
    }

    protected virtual ArticleStage NextStage => _article!.Stage;

    protected async Task<FileMetadata> UploadFile(UploadFileCommand command, Asset asset, AssetTypeDefinition assetType, CancellationToken ct)
    {
        var filePath = asset.GenerateStorageFilePath(command.File.FileName);
        return await _fileService.UploadAsync(
            filePath,
            command.File,
            overwrite: true,
            tags: new Dictionary<string, string>
            {
                { "entity", nameof(Asset) },
                { "entityId", asset.Id.ToString() }
            }, ct);
    }
}
```

### FastEndpoints variant (Production `_Shared/UploadFileEndpoint.cs`)

Same compensating-delete shape, no MediatR; the own store is the non-generic `IFileService`.

```csharp
public class UploadFileEndpoint<TUploadCommand>
    (ArticleRepository articleRepository, AssetTypeRepository assetTypeRepository, IFileService fileService, AssetStateMachineFactory factory)
    : AssetBaseEndpoint<TUploadCommand, AssetActionResponse>(assetTypeRepository, factory)
    where TUploadCommand : UploadFileCommand
{
    protected readonly ArticleRepository _articleRepository = articleRepository;
    protected readonly IFileService _fileService = fileService;

    public async override Task HandleAsync(TUploadCommand command, CancellationToken ct)
    {
        _article = await _articleRepository.GetByIdWithAssetsAsync(command.ArticleId);
        var assetType = _assetTypeRepository.GetById(command.AssetType);

        var asset = _article.Assets.SingleOrDefault(a => a.Type == command.AssetType && a.Number == command.GetAssetNumber())
                    ?? _article.CreateAsset(assetType, command.GetAssetNumber());

        CheckAndThrowStateTransition(asset, command.ActionType);

        var fileMetadata = await UploadFile(command, asset, ct);
        try
        {
            asset.CreateAndAddFile(fileMetadata, assetType, command);
            _article.SetStage(NextStage, command);
            await _articleRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            await _fileService.TryDeleteAsync(fileMetadata.StoragePath); // delete the file if something is wrong
            throw;
        }

        await Send.OkAsync(new AssetActionResponse(asset.Adapt<AssetMinimalDto>()));
    }
}
```

---

## 5. Variant A — runtime factory (Review, filestorage-2)

Use only when a handler picks the store dynamically per call.

```csharp
// Review.Application/FileStorage/FileServiceFactory.cs
namespace Review.Application.FileStorage;

public enum FileStorageType
{
    Review,
    Submission
}

public delegate IFileService FileServiceFactory(FileStorageType fileStorageType);
```

```csharp
// Review.API/FileServiceFactoryRegistration.cs
public static class FileServiceFactoryRegistration
{
    public static IServiceCollection AddFileServiceFactory(this IServiceCollection services)
    {
        services.AddScoped<FileServiceFactory>(serviceProvider => fileStorageType =>
        {
            return fileStorageType switch
            {
                FileStorageType.Submission => serviceProvider.GetRequiredService<IFileService<SubmissionFileStorageOptions>>(),
                FileStorageType.Review     => serviceProvider.GetRequiredService<IFileService>(), // own store = the non-generic singleton (Phase 1)
                _ => throw new ApplicationException()
            };
        });
        return services;
    }
}
```

> Correctness note: the shipped Review registration keys its own arm to `IFileService<MongoGridFsFileStorageOptions>`, but Review registers its own store only under the plain `IFileService` (via `AddMongoFileStorageAsSingletone`), so that closed generic is not registered. The form above — own arm resolves the non-generic `IFileService`, foreign arm resolves the closed generic — is the resolvable one and matches how the own store is actually written.

---

## 6. Variant B — direct dual injection + cross-stage migration (Production, filestorage-2 + consumers-4)

Use when both stores are always needed together. The consumer reads the foreign store and writes the own store, physically copying bytes stage-to-stage.

```csharp
public sealed class ArticleAcceptedForProductionConsumer(
    ProductionDbContext dbContext,
    ArticleRepository articleRepository,
    Repository<Person> personRepository,
    Repository<Journal> journalRepository,
    AssetTypeRepository assetTypeRepository,
    IFileService<ReviewFileStorageOptions> reviewFileService, // foreign store (read)
    IFileService azureBlobFileService                         // own store (write)
) : IConsumer<ArticleAcceptedForProductionEvent>
{
    public async Task Consume(ConsumeContext<ArticleAcceptedForProductionEvent> context)
    {
        var articleDto = context.Message.Article;
        var ct = context.CancellationToken;

        if (await articleRepository.ExistsAsync(articleDto.Id))
            return;

        var journal = await GetOrCreateJournal(articleDto);
        var actors = await CreateActors(articleDto, ct);
        var assets = await CreateAssets(articleDto, ct); // migrates bytes — below

        var action = new ArticleAction
        {
            ArticleId = articleDto.Id,
            ActionType = ArticleActionType.AcceptForProduction,
            CreatedById = articleDto.SubmittedBy.UserId ?? 0,
        };

        var article = Article.FromReview(articleDto, actors, assets, action);
        await articleRepository.AddAsync(article);
        await dbContext.SaveChangesAsync(ct);
    }

    private async Task<IEnumerable<Asset>> CreateAssets(ArticleDto articleDto, CancellationToken ct)
    {
        var assets = new List<Asset>();
        foreach (var assetDto in articleDto.Assets)
        {
            var assetType = assetTypeRepository.GetById(assetDto.Type);
            var asset = Asset.CreateFromReview(articleDto.Id, assetType, (byte)(assetDto.Number - 1));

            // consumers-4: download from the previous stage's store, upload into our own.
            var (fileStream, fileMetadata) = await reviewFileService.DownloadAsync(assetDto.File.FileServerId, ct);

            var uploadedMetadata = await azureBlobFileService.UploadAsync(
                new FileUploadRequest(fileMetadata.StoragePath, fileMetadata.FileName, fileMetadata.ContentType, fileMetadata.FileSize),
                fileStream,
                ct: ct);

            asset.CreateAndAddFile(uploadedMetadata, assetType);
            assets.Add(asset);
        }

        return assets;
    }
}
```
