using FileStorage.Contracts;
using Mapster;
using Production.API.Features.Shared;
using Production.Application.Dtos;
using Production.Application.StateMachines;
using Production.Domain.Assets;
using Production.Domain.Assets.Enums;
using Production.Persistence.Repositories;

namespace Production.API.Features.Assets.UploadFiles._Shared;

public class UploadFileEndpoint<TUploadCommand>
    (ArticleRepository articleRepository, AssetTypeRepository assetTypeRepository, IFileService fileService, AssetStateMachineFactory factory)
    : AssetBaseEndpoint<TUploadCommand, AssetActionResponse>(assetTypeRepository, factory)
    where TUploadCommand : UploadFileCommand
{
    //insight - readonly fields in primary constructors are not supported yet but they will be
    protected readonly ArticleRepository _articleRepository = articleRepository;
    protected readonly IFileService _fileService = fileService;

    public async override Task HandleAsync(TUploadCommand command, CancellationToken ct)
    {
        //_article = await _articleRepository.GetByIdWithSingleAssetAsync(command.ArticleId, command.AssetType, command.GetAssetNumber());
        _article = await _articleRepository.GetByIdWithAssetsAsync(command.ArticleId);
        var assetType = _assetTypeRepository.GetById(command.AssetType);

        var asset = _article.Assets.SingleOrDefault(a => a.Type == command.AssetType && a.Number == command.GetAssetNumber());
        if (asset is null)
            asset = _article.CreateAsset(assetType, command.GetAssetNumber());

        CheckAndThrowStateTransition(asset, command.ActionType);
        asset.SetState(AssetState.Uploaded, command);

        var fileMetadata = await UploadFile(command, asset, ct);
        try
        {
            asset.CreateAndAddFile(fileMetadata, assetType);

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

    private async Task<FileMetadata> UploadFile(UploadFileCommand command, Asset asset, CancellationToken ct)
    {
        var filePath = asset.GenerateStorageFilePath(command.File.FileName);
        //insight about tags
        return await _fileService.UploadAsync(filePath, command.File, overwrite:true,
                new Dictionary<string, string>{ 
                    {"entity", nameof(Asset)},
                    {"entityId", asset.Id.ToString()}
                }, ct);
    }    
}
