using Microsoft.AspNetCore.Authorization;
using Blocks.Redis;
using Blocks.Linq;
using Articles.Security;
using Journals.API.Features.Shared;
using Journals.Domain.Journals.Events;

namespace Journals.API.Features.Sections.Update;

[Authorize(Roles = $"{Role.EditorAdmin},{Role.Editor}")]
[HttpPut("journals/{journalId:int}/sections/{sectionId:int}")]
[Tags("Sections")]
public class UpdateSectionEndpoint(Repository<Journal> _repository)
    : Endpoint<UpdateSectionCommand, IdResponse>
{
    public override async Task HandleAsync(UpdateSectionCommand command, CancellationToken ct)
    {
				var journal = await _repository.GetByIdOrThrowAsync(command.JournalId);
				var section = journal.Sections.SingleOrThrow(s => s.Id == command.SectionId);
				command.Adapt(section);

				await _repository.ReplaceAsync(journal);

				await PublishAsync(section.Adapt<SectionUpdated>());

				await Send.OkAsync(new IdResponse(section.Id));
    }
}
