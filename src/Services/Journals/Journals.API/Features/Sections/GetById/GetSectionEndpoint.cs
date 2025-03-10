using Blocks.Linq;
using Blocks.Mapster;
using Blocks.Redis;
using Journals.API.Features.Shared;
using Journals.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace Journals.API.Features.Sections.GetById;

[Authorize]
[HttpGet("journals/{journalId:int}/sections/{sectionId:int}")]
[Tags("Sections")]
public class GetSectionEndpoint(JournalDbContext _dbContext)
    : Endpoint<GetSectionQuery, GetSectionResponse>
{
    public override async Task HandleAsync(GetSectionQuery query, CancellationToken ct)
    {
        var journal = await _dbContext.Journals.GetByIdOrThrowAsync(query.JournalId);
				var section = journal.Sections.SingleOrThrow(s => s.Id == query.SectionId);

        var sectionDto = section.Adapt<SectionDto>();
        
        sectionDto.Editors = new List<EditorDto>();
				foreach (var editorRole in section.EditorRoles)
        {
						var editor = await _dbContext.Editors.GetByIdOrThrowAsync(editorRole.EditorId);
						var editorDto = editor.AdaptWith<EditorDto>(editor => editor.Role = editorRole.EditorRole);
						sectionDto.Editors.Add(editorDto);
				}

        await Send.OkAsync(new GetSectionResponse(sectionDto));
    }
}
