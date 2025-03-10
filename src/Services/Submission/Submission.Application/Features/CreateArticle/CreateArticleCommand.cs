namespace Submission.Application.Features.CreateArticle;

public record CreateArticleCommand(int JournalId, string Title, ArticleType Type, string Scope)
		: ArticleCommand
{
		public override ArticleActionType ActionType => ArticleActionType.CreateArticle;
}

public class CreateArticleCommandValidator : AbstractValidator<CreateArticleCommand>
{
		public CreateArticleCommandValidator()
		{
				RuleFor(x => x.Title)
						.NotEmptyWithMessage(nameof(CreateArticleCommand.Title))
						.MaximumLengthWithMessage(MaxLength.C256, nameof(CreateArticleCommand.Title));

				RuleFor(x => x.Scope)
						.NotEmptyWithMessage(nameof(CreateArticleCommand.Scope))
						.MaximumLengthWithMessage(MaxLength.C2048, nameof(CreateArticleCommand.Scope));

				RuleFor(c => c.JournalId).GreaterThan(0).WithMessageForInvalidId(nameof(CreateArticleCommand.JournalId));
		}
}
