using Articles.Abstractions.Enums;
using Production.API.Features.Assets.UploadFiles._Shared;
using Production.Domain.Assets.Enums;
using Production.Persistence.Repositories;

namespace Production.API.Features.Assets.UploadFiles.UploadFinalFile;

public record UploadFinalFileCommand : UploadFileCommand;

public class UploadFinalFileValidator : UploadFileValidator<UploadFinalFileCommand>
{
    public UploadFinalFileValidator(AssetTypeRepository assetTypeRepository) : base(assetTypeRepository) { }

    public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.FinalAssets;
}