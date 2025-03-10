using Production.API.Features.Assets.RequestAssets._Shared;
using Production.Domain.Assets.Enums;

namespace Production.API.Features.Assets.RequestAssets.CancelRequest;

public record CancelRequestFinalAssetsCommand : RequestMultipleAssetsCommand
{
		public override AssetActionType ActionType => AssetActionType.CancelRequest;
}

public record CancelRequestAuthorFilesCommand : RequestMultipleAssetsCommand
{
}