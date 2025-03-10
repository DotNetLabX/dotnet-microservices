using Blocks.Linq;
using Blocks.Mapster;
using Blocks.Redis;
using Journals.API.Features.Editors.Get;
using Journals.API.Features.Shared;
using Journals.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace Journals.API.Features.Editors.GetById;

[Authorize]
[HttpGet("journals/{journalId:int}/sections/{sectionId:int}/editors")]
[Tags("Sections")]
public class GetEditorsBySectionIdEndpoint(JournalDbContext _dbContext)
    : Endpoint<GetEditorsBySectionIdQuery, GetEditorsBySectionIdResponse>
{
    public override async Task HandleAsync(GetEditorsBySectionIdQuery query, CancellationToken ct)
    {
        var journal = await _dbContext.Journals.GetByIdOrThrowAsync(query.JournalId);
        var section = journal.Sections.SingleOrThrow(s => s.Id == query.SectionId);

        var sectionDto = section.Adapt<SectionDto>();
        
        var editors = new List<EditorDto>();
        foreach (var editorRole in section.EditorRoles)
        {
            var editor = await _dbContext.Editors.GetByIdOrThrowAsync(editorRole.EditorId);
						var editorDto = editor.AdaptWith<EditorDto>(editor => editor.Role = editorRole.EditorRole);
						editors.Add(editorDto);
				}

				await Send.OkAsync(new GetEditorsBySectionIdResponse(editors));
    }
}
