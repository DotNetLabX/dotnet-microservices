using Review.Application.Features.Assets.UploadFiles.Shared;
using Review.Domain.Shared.Enums;
using Review.Domain.Assets.Enums;

namespace Review.Application.Features.Assets.UploadFiles.UploadReviewReport;

public record UploadReviewReportCommand : UploadFileCommand
{
		public override ArticleActionType ActionType => ArticleActionType.UploadReviewReport;
}

public abstract class UploadReviewReportCommandValidator : UploadFileValidator<UploadReviewReportCommand>
{
    public UploadReviewReportCommandValidator(AssetTypeDefinitionRepository assetTypeRepository)
            : base(assetTypeRepository) { }

    public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.ReviewReport;
}