using Articles.Abstractions.Enums;
using Production.API.Features.Assets.RequestAssets._Shared;
using Production.Domain.Assets.Enums;

namespace Production.API.Features.Assets.RequestAssets.Supplementary;

public record RequestSupplementaryAssetsCommand : RequestMultipleAssetsCommand;

public class RequestSupplementaryAssetsValidator() 
    : RequestAssetsValidator<RequestSupplementaryAssetsCommand>()
{
    public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.SupplementaryAssets;
}
