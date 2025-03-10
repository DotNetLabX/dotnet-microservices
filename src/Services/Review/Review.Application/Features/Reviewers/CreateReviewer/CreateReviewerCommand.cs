using Review.Application.Features.Articles.Shared;
using Review.Domain.Shared.Enums;

namespace Review.Application.Features.Reviewers.CreateReviewer;

public record CreateReviewerCommand(
        IEnumerable<int>  Specializations,
        int? UserId,
        string Email,
        string FirstName,
        string LastName,
        Honorific? Honorific,
        string Affiliation) : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.CreateReviewer;
}

public class CreateReviewerCommandValidator : AbstractValidator<CreateReviewerCommand>
{
    public CreateReviewerCommandValidator()
    {

        When(c => c.UserId == null, () =>
        {

            RuleFor(x => x.Email)
                .NotEmptyWithMessage(nameof(CreateReviewerCommand.Email))
                .MaximumLengthWithMessage(MaxLength.C64, nameof(CreateReviewerCommand.Email));

            RuleFor(x => x.FirstName)
                    .NotEmptyWithMessage(nameof(CreateReviewerCommand.FirstName))
                    .MaximumLengthWithMessage(MaxLength.C64, nameof(CreateReviewerCommand.FirstName));

            RuleFor(x => x.LastName)
                    .NotEmptyWithMessage(nameof(CreateReviewerCommand.LastName))
                    .MaximumLengthWithMessage(MaxLength.C64, nameof(CreateReviewerCommand.LastName));

            RuleFor(x => x.Affiliation)
                    .NotEmptyWithMessage(nameof(CreateReviewerCommand.Affiliation))
                    .MaximumLengthWithMessage(MaxLength.C512, nameof(CreateReviewerCommand.Affiliation));
        });

        RuleFor(x => x.Specializations).NotEmptyWithMessage("A reviewer must have at least one specialization");

		}
}