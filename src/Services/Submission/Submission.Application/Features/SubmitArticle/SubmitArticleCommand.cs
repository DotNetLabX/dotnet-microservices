namespace Submission.Application.Features.SubmitArticle;

public record SubmitArticleCommand : ArticleCommand
{
		public override ArticleActionType ActionType => ArticleActionType.SubmitDraft;
}


public class SubmitArticleCommandValidator : ArticleCommandValidator<SubmitArticleCommand>;
