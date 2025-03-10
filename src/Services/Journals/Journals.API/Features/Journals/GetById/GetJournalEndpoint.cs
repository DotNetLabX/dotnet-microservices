using Blocks.Mapster;
using Blocks.Redis;
using Journals.API.Features.Shared;
using Journals.Domain.Journals.Enums;
using Journals.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace Journals.API.Features.Journals.Get;

[Authorize]
[HttpGet("journals/{journalId:int}")]
[Tags("Journals")]
public class GetJournalEndpoint(JournalDbContext _dbContext)
    : Endpoint<GetJournalQuery, GetJournalResponse>
{
    public override async Task HandleAsync(GetJournalQuery query, CancellationToken ct)
    {
        var journal = await _dbContext.Journals.GetByIdOrThrowAsync(query.JournalId);

        var journalDto = journal.Adapt<JournalDto>();

        var chiefEditor = await _dbContext.Editors.GetByIdOrThrowAsync(journal.ChiefEditorId);
        journalDto.ChiefEditor = chiefEditor.AdaptWith<EditorDto>(editor => editor.Role = EditorRole.ChiefEditor);


        var editors = journal.Sections.SelectMany(s => s.EditorRoles)
                .DistinctBy(e => e.EditorId)
                .ToList()
                .AsReadOnly();

        foreach (var editorRole in editors)
				{
						var editor = await _dbContext.Editors.GetByIdOrThrowAsync(editorRole.EditorId);
            var editorDto = editor.AdaptWith<EditorDto>(editor => 
                { 
                    editor.Role = journalDto.ChiefEditor.Id == editor.Id ? EditorRole.ChiefEditor : EditorRole.ReviewEditor; 
                });
						journalDto.Editors.Add(editorDto);
				}

				await Send.OkAsync(new GetJournalResponse(journalDto));
    }
}
