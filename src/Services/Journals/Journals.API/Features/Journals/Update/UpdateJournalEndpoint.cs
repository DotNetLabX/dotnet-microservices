using Articles.Security;
using Blocks.Redis;
using Journals.API.Features.Shared;
using Journals.Domain.Journals.Events;
using Microsoft.AspNetCore.Authorization;

namespace Journals.API.Features.Journals.Update;

[Authorize(Roles = Role.EditorAdmin)]
[HttpPut("journals/{journalId:int}")]
[Tags("Journals")]
public class UpdateJournalEndpoint(Repository<Journal> _journalRepository)
    : Endpoint<UpdateJournalCommand, IdResponse>
{
    public override async Task HandleAsync(UpdateJournalCommand command, CancellationToken ct)
    {
				var journal = await _journalRepository.GetByIdOrThrowAsync(command.JournalId);
				
				command.Adapt(journal);

				// UpdateAsync does not work properly for children collections(e.g Sections), so we use ReplaceAsync.
				await _journalRepository.ReplaceAsync(journal);

				await PublishAsync(new JournalUpdated(journal));

				await Send.OkAsync(new IdResponse(journal.Id));
    }
}
