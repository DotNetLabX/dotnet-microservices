using Mapster;
using Production.API.Features.Assets.RequestAssets._Shared;
using Production.API.Features.Shared;
using Production.Application.Dtos;
using Production.Application.StateMachines;
using Production.Domain.Assets;
using Production.Domain.Assets.Enums;
using Production.Persistence.Repositories;

namespace Production.API.Features.Assets.RequestAssets.CancelRequest;

[Authorize(Roles = Role.ProdAdmin)]
[HttpPut("articles/{articleId:int}/assets/final:cancel-request")]
[Tags("Assets")]
public class CancelRequestFinalAssetsEndpoint(ArticleRepository articleRepository, AssetTypeRepository assetTypeRepository, AssetStateMachineFactory factory)
    : AssetBaseEndpoint<CancelRequestFinalAssetsCommand, RequestAssetsResponse>(assetTypeRepository, factory)
{
    private readonly ArticleRepository _articleRepository = articleRepository;

    public async override Task HandleAsync(CancelRequestFinalAssetsCommand command, CancellationToken cancellationToken)
    {
        _article = await _articleRepository.GetByIdWithAssetsAsync(command.ArticleId);

        var assets = new List<Asset>();
        foreach (var assetRequest in command.AssetRequests)
        {
            var asset = _article.Assets
                    .SingleOrDefault(asset => asset.Type == assetRequest.AssetType && asset.Number == assetRequest.AssetNumber);

            if (asset?.State != AssetState.Requested)
                continue;

            CheckAndThrowStateTransition(asset, command.ActionType);
            asset.CancelRequest(command);
            assets.Add(asset);
        }

        _article.SetStage(NextStage, command);
        await _articleRepository.SaveChangesAsync();

        await Send.OkAsync(new RequestAssetsResponse
        {
            Assets = assets.Select(a => a.Adapt<AssetMinimalDto>())
        });
    }
}
