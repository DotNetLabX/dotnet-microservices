using FileStorage.Contracts;
using Review.Domain.Assets;

namespace Review.Application.Features.Assets.UploadFiles.Shared;

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

        var fileMetada = await UploadFile(command, asset, assetType, ct);

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
        //insight about tags
        return await _fileService.UploadAsync(
                        filePath,
                        command.File,
                        //overwrite: !assetType.AllowsMultipleAssets, // if the asset type does not support multiple assets, we are overriding the file.
                        overwrite: true,
                        tags: new Dictionary<string, string>{
                    {"entity", nameof(Asset)},
                    {"entityId", asset.Id.ToString()}
                }, ct);
    }

    protected Asset GetOrCreateAsset(AssetTypeDefinition assetType, TUploadCommand command)
    {
        Asset? asset = null;

        if (!assetType.AllowsMultipleAssets) // if the asset type doesn't support multiple assets, we are overriding the single one.
            asset = _article.Assets.SingleOrDefault(a => a.Type == assetType.Id);

        if (asset is null)
            asset = _article.CreateAsset(assetType, command);

        return asset;
    }
}
