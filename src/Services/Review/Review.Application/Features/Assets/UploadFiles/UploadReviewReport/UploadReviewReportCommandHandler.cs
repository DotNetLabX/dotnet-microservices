using FileStorage.Contracts;
using Review.Application.Features.Assets.UploadFiles.Shared;

namespace Review.Application.Features.Assets.UploadFiles.UploadReviewReport;

public class UploadReviewReportCommandHandler(
        ArticleRepository articleRepository,
        AssetTypeDefinitionRepository assetTypeRepository,
        IFileService fileService,
        ArticleStateMachineFactory stateMachineFactory)
    : UploadFileCommandHandler<UploadReviewReportCommand>(articleRepository, assetTypeRepository, fileService, stateMachineFactory)
{

		protected override ArticleStage NextStage
    {
        get
        {
            var reviewReports = _article.Assets.Where(a => a.Type == AssetType.ReviewReport);
            var reviewers = _article.Actors.Where(a => a.Role == UserRoleType.REV);
            //if all the review reports are uploaded is time for the editor to take a decision regarding the article
            if (reviewers.Count() == reviewReports.Count()) 
                return ArticleStage.ReadyForDecision;

            
            return base.NextStage;
        }
    }
}
