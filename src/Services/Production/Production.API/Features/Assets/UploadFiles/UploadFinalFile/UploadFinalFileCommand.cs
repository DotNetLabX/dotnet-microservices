using Articles.Abstractions.Enums;
using Production.API.Features.Assets.UploadFiles._Shared;
using Production.Domain.Assets.Enums;

namespace Production.API.Features.Assets.UploadFiles.UploadFinalFile;

public record UploadFinalFileCommand : UploadFileCommand;

public abstract class UploadFinalFileValidator : UploadFileValidator<UploadFinalFileCommand>
{
		public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.FinalAssets;
}