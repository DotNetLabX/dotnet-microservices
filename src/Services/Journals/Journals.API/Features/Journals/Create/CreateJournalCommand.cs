using FastEndpoints;
using FluentValidation;

namespace Journals.API.Features.Journals.Create;

public record CreateJournalCommand(string Abbreviation, string Name, string Description, string ISSN, int ChiefEditorId)
{
    public string NormalizedName => Name.ToLowerInvariant();
}

public class CreateJournalCommandValidator : Validator<CreateJournalCommand>
{
    public CreateJournalCommandValidator()
    {
				RuleFor(r => r.Abbreviation).NotEmpty();
				RuleFor(r => r.Name).NotEmpty();
				RuleFor(r => r.ISSN).NotEmpty().Matches(@"\d{4}-\d{3}[\dX]").WithMessage("Invalid ISSN format");
				RuleFor(r => r.Description).NotEmpty();
				RuleFor(r => r.ChiefEditorId).GreaterThan(0);
		}
}