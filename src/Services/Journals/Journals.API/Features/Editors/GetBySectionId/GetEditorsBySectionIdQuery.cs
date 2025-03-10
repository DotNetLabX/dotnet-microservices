using FastEndpoints;
using FluentValidation;
using Journals.API.Features.Shared;

namespace Journals.API.Features.Editors.Get;

public record GetEditorsBySectionIdQuery(int JournalId, int SectionId);
public record GetEditorsBySectionIdResponse(List<EditorDto> Editors);

public class GetJournalQueryValidator : Validator<GetEditorsBySectionIdQuery>
{
    public GetJournalQueryValidator()
    {
				RuleFor(r => r.JournalId).GreaterThan(0);
				RuleFor(r => r.SectionId).GreaterThan(0);
		}
}