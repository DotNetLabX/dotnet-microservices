using FastEndpoints;
using FluentValidation;
using Journals.API.Features.Shared;

namespace Journals.API.Features.Sections.Create;

public record CreateSectionCommand(int JournalId, string Name, string Description, SectionEditorDto[] EditorRoles);

public class CreateSectionCommandValidator : Validator<CreateSectionCommand>
{
    public CreateSectionCommandValidator()
    {
				RuleFor(r => r.JournalId).GreaterThan(0);
				RuleFor(r => r.Name).NotEmpty();
				RuleFor(r => r.Description).NotEmpty();
		}
}