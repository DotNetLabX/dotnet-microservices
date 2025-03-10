namespace Submission.Application.Features.CreateAuthor;

public record CreateAuthorCommand(
		string Email,
		string FirstName,
		string LastName,
		Honorific? Honorific,
		string Affiliation) : ArticleCommand
{
		public override ArticleActionType ActionType =>  ArticleActionType.CreateAuthor;
}

public class CreateAuthorCommandValidator : AbstractValidator<CreateAuthorCommand>
{
		public CreateAuthorCommandValidator()
		{
				RuleFor(x => x.Email)
						.NotEmptyWithMessage(nameof(CreateAuthorCommand.Email))
						.MaximumLengthWithMessage(MaxLength.C64, nameof(CreateAuthorCommand.Email));

				RuleFor(x => x.FirstName)
						.NotEmptyWithMessage(nameof(CreateAuthorCommand.FirstName))
						.MaximumLengthWithMessage(MaxLength.C64, nameof(CreateAuthorCommand.FirstName));

				RuleFor(x => x.LastName)
						.NotEmptyWithMessage(nameof(CreateAuthorCommand.LastName))
						.MaximumLengthWithMessage(MaxLength.C64, nameof(CreateAuthorCommand.LastName));

				RuleFor(x => x.Affiliation)
						.NotEmptyWithMessage(nameof(CreateAuthorCommand.Affiliation))
						.MaximumLengthWithMessage(MaxLength.C512, nameof(CreateAuthorCommand.Affiliation));
		}
}