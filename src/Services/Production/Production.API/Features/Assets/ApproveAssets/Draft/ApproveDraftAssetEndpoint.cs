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
public class ApproveDraftAssetEndpoint(ArticleRepository articleRepository, AssetTypeRepository assetTypeRepository, AssetStateMachineFactory stateMachineFactory)
    : AssetBaseEndpoint<ApproveDraftAssetCommand, AssetActionResponse>(assetTypeRepository, stateMachineFactory)
{
    private readonly ArticleRepository _articleRepository = articleRepository;

    public override async Task HandleAsync(ApproveDraftAssetCommand command, CancellationToken ct)
    {
        _article = await _articleRepository.GetByIdWithAssetsAsync(command.ArticleId);
        var asset = _article.Assets.Single(a => a.Id == command.AssetId);

        CheckAndThrowStateTransition(asset, command.ActionType);
        asset.SetState(AssetState.Approved, command);

        _article.SetStage(NextStage, command);

        await _articleRepository.SaveChangesAsync();
        await Send.OkAsync(new AssetActionResponse(asset.Adapt<AssetMinimalDto>()));
    }

    protected override ArticleStage NextStage => ArticleStage.FinalProduction;
}
