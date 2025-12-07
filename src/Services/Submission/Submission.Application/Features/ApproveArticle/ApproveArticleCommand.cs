namespace Submission.Application.Features.ApproveArticle;

public record ApproveArticleCommand : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.ApproveDraft;
}


public class ApproveArticleCommandValidator : ArticleCommandValidator<ApproveArticleCommand>;
