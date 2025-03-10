using Submission.Application.Features.UploadFiles.Shared;

namespace Submission.Application.Features.UploadFiles;

public record UploadSupplementaryFileCommand : UploadFileCommand;

public class UploadSupplementaryFileValidator : UploadFileValidator<UploadSupplementaryFileCommand>
{
		public UploadSupplementaryFileValidator(AssetTypeRepository assetTypeRepository)
				: base(assetTypeRepository) { }

		public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.SupplementaryAssets;
}