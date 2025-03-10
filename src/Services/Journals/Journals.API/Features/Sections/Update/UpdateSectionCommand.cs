using FastEndpoints;
using FluentValidation;

namespace Journals.API.Features.Sections.Update;

public record UpdateSectionCommand(int JournalId, int SectionId, string Name, string Description);

public class UpdateSectionCommandValidator : Validator<UpdateSectionCommand>
{
    public UpdateSectionCommandValidator()
    {
				RuleFor(r => r.JournalId).GreaterThan(0);
				RuleFor(r => r.SectionId).GreaterThan(0);
				RuleFor(r => r.Name).NotEmpty();
				RuleFor(r => r.Description).NotEmpty();
		}
}