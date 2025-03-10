using FluentValidation;
using Production.API.Features.Shared;
using Production.Domain.Assets.Enums;

namespace Production.API.Features.Assets.ApproveAssets.Draft;

public record ApproveDraftAssetCommand : AssetCommand<AssetActionResponse>
{
    public int AssetId { get; set; }
    public override AssetActionType ActionType => AssetActionType.Approve;
}

public class ApproveDraftAssetCommandValidator
    : ArticleCommandValidator<ApproveDraftAssetCommand>
{
    public ApproveDraftAssetCommandValidator()
    {
        //ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.AssetId).GreaterThan(0);
    }
}
