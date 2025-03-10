using Mapster;
using Articles.Abstractions.Enums;
using Production.Persistence.Repositories;
using Production.API.Features.Shared;
using Production.Application.Dtos;
using Production.Application.StateMachines;
using Production.Domain.Assets.Enums;

namespace Production.API.Features.Assets.ApproveAssets.Draft;

[Authorize(Roles = $"{Role.ProdAdmin},{Role.Author}")]
[HttpPut("articles/{articleId:int}/assets/draft/{assetId:int}:approve")]
[Tags("Assets")]

public class ApproveDraftAssetEndpoint(AssetRepository _assetRepository, AssetTypeRepository assetTypeRepository, AssetStateMachineFactory stateMachineFactory)
    : AssetBaseEndpoint<ApproveDraftAssetCommand, AssetActionResponse>(assetTypeRepository, stateMachineFactory)
{
    public override async Task HandleAsync(ApproveDraftAssetCommand command, CancellationToken ct)
    {
        var asset = await _assetRepository.GetByIdAsync(command.ArticleId, command.AssetId);
        _article = asset.Article;

				//stateMachineFactory.ValidateStageTransition(_article.Stage, NextStage, asset.Type, command.ActionType);

				CheckAndThrowStateTransition(asset, command.ActionType);
				asset.SetState(AssetState.Approved, command);

				_article.SetStage(NextStage, command);

        await _assetRepository.SaveChangesAsync();
        await Send.OkAsync(new AssetActionResponse(asset.Adapt<AssetMinimalDto>()));
    }

		protected override ArticleStage NextStage => ArticleStage.FinalProduction;
}
