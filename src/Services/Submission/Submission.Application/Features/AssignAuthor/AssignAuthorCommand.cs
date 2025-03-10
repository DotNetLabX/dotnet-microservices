namespace Submission.Application.Features.AssignAuthor;

public record AssignAuthorCommand(int AuthorId, bool IsCorrespondingAuthor, HashSet<ContributionArea> ContributionAreas)
		: ArticleCommand
{
		public override ArticleActionType ActionType => ArticleActionType.AssignAuthor;
}

public class AssignAuthorCommandValidator : ArticleCommandValidator<AssignAuthorCommand>
{
		public AssignAuthorCommandValidator()
		{
				RuleFor(c => c.AuthorId).GreaterThan(0).WithMessageForInvalidId(nameof(AssignAuthorCommand.AuthorId));

				RuleFor(command => command.ContributionAreas)
						.NotEmptyWithMessage(nameof(AssignAuthorCommand.ContributionAreas));
		}
}