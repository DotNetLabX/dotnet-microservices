using Mapster;
using Production.API.Features.Shared;
using Production.Application.Dtos;
using Production.Application.StateMachines;
using Production.Domain.Assets.Enums;
using Production.Domain.Assets;
using Production.Persistence.Repositories;

namespace Production.API.Features.Assets.RequestAssets._Shared;

public class RequestAssetsEndpointBase<TCommand>(ArticleRepository _articleRepository, AssetTypeRepository assetTypeRepository, AssetStateMachineFactory factory)
    : AssetBaseEndpoint<TCommand, RequestAssetsResponse>(assetTypeRepository, factory)
    where TCommand : RequestMultipleAssetsCommand
{
    public async override Task HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        _article = await _articleRepository.GetByIdWithAssetsAsync(command.ArticleId);

        var assetsResponse = new List<Asset>();
        foreach (var assetRequest in command.AssetRequests)
        {
            var asset = _article.Assets
                .SingleOrDefault(asset => asset.Type == assetRequest.AssetType && asset.Number == assetRequest.AssetNumber);
            
            if (asset is null)
								asset = CreateAsset(assetRequest.AssetType, assetRequest.AssetNumber);

						CheckAndThrowStateTransition(asset, command.ActionType);
						asset.SetState(AssetState.Requested, command);
						
            assetsResponse.Add(asset);
				}

				_article.SetStage(NextStage, command);
				await _articleRepository.SaveChangesAsync();

        var response = new RequestAssetsResponse()
        {
            Assets = assetsResponse.Select(a => a.Adapt<AssetMinimalDto>())
        };				
				await Send.OkAsync(response);
    }
}
