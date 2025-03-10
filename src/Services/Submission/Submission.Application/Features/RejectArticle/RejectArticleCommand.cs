namespace Submission.Application.Features.RejectArticle;

public record RejectArticleCommand : ArticleCommand
{
		public override ArticleActionType ActionType => ArticleActionType.RejectDraft;
}


public class RejectArticleCommandValidator : ArticleCommandValidator<RejectArticleCommand>;
