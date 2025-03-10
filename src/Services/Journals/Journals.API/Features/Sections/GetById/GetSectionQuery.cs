using FastEndpoints;
using FluentValidation;
using Journals.API.Features.Shared;

namespace Journals.API.Features.Sections.GetById;

public record GetSectionQuery(int JournalId, int SectionId);
public record GetSectionResponse(SectionDto Section);



public class GetJournalQueryValidator : Validator<GetSectionQuery>
{
    public GetJournalQueryValidator()
    {
				RuleFor(r => r.JournalId).GreaterThan(0);
				RuleFor(r => r.SectionId).GreaterThan(0);
		}
}