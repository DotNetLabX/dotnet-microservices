using Submission.Application.Features.UploadFiles.Shared;

namespace Submission.Application.Features.UploadFiles;

public record UploadManuscriptFileCommand : UploadFileCommand;

public class UploadManuscriptCommandValidator : UploadFileValidator<UploadManuscriptFileCommand>
{
		public UploadManuscriptCommandValidator(AssetTypeRepository assetTypeRepository)
				: base(assetTypeRepository) { }

		public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.ManuscriptAsset;
}
