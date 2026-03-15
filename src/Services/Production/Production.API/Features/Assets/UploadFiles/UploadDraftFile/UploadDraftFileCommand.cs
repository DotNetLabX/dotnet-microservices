using Articles.Abstractions.Enums;
using Production.API.Features.Assets.UploadFiles._Shared;
using Production.Domain.Assets.Enums;
using Production.Persistence.Repositories;

namespace Production.API.Features.Assets.UploadFiles.UploadDraftFile;

public record UploadDraftFileCommand : UploadFileCommand;

public class UploadDraftFileValidator : UploadFileValidator<UploadDraftFileCommand>
{
    public UploadDraftFileValidator(AssetTypeRepository assetTypeRepository) : base(assetTypeRepository) { }

    public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.DraftAssets;
}
