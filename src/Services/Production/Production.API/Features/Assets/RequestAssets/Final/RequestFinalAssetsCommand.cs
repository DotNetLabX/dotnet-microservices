using Articles.Abstractions.Enums;
using Production.API.Features.Assets.RequestAssets._Shared;
using Production.Domain.Assets.Enums;

namespace Production.API.Features.Assets.RequestAssets.Final;

public record RequestFinalAssetsCommand : RequestMultipleAssetsCommand;

public class RequestFinalFilesValidator() 
    : RequestAssetsValidator<RequestFinalAssetsCommand>()
{
    public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.FinalAssets;
}