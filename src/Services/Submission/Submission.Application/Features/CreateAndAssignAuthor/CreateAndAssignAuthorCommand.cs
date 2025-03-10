namespace Submission.Application.Features.CreateAndAssignAuthor;

public record CreateAndAssignAuthorCommand(int? PersonId, string? FirstName, string? LastName, string? Email, Honorific? Honorific, string? Affiliation, bool IsCorrespondingAuthor, HashSet<ContributionArea> ContributionAreas)
		: ArticleCommand
{
		public override ArticleActionType ActionType => ArticleActionType.AssignAuthor;
}

public class CreateAndAssignAuthorCommandValidator : ArticleCommandValidator<CreateAndAssignAuthorCommand>
{
		public CreateAndAssignAuthorCommandValidator()
		{
				When(c => c.PersonId == null, () =>
				{
						RuleFor(x => x.Email)
								.NotEmptyWithMessage(nameof(CreateAndAssignAuthorCommand.Email))
								.MaximumLengthWithMessage(MaxLength.C64, nameof(CreateAndAssignAuthorCommand.Email))
								.EmailAddress();

						RuleFor(x => x.FirstName)
								.NotEmptyWithMessage(nameof(CreateAndAssignAuthorCommand.FirstName))
								.MaximumLengthWithMessage(MaxLength.C64, nameof(CreateAndAssignAuthorCommand.FirstName));

						RuleFor(x => x.LastName)
								.NotEmptyWithMessage(nameof(CreateAndAssignAuthorCommand.LastName))
								.MaximumLengthWithMessage(MaxLength.C256, nameof(CreateAndAssignAuthorCommand.LastName));

						RuleFor(x => x.Affiliation)
								.NotEmptyWithMessage(nameof(CreateAndAssignAuthorCommand.Affiliation))
								.MaximumLengthWithMessage(MaxLength.C512, nameof(CreateAndAssignAuthorCommand.Affiliation));

				});

				RuleFor(x => x.ContributionAreas)
						.NotEmptyWithMessage("The author must contribute to at least one mandatory area.");
		}
}
