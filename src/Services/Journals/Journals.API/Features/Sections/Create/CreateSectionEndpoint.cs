using Microsoft.AspNetCore.Authorization;
using Blocks.Exceptions;
using Blocks.Redis;
using Articles.Security;
using Journals.API.Features.Shared;

namespace Journals.API.Features.Sections.Create;

[Authorize(Roles = Role.EditorAdmin)]
[HttpPost("journals/{journalId:int}/sections")]
[Tags("Sections")]
public class CreateSectionEndpoint(Repository<Journal> _repository)
    : Endpoint<CreateSectionCommand, IdResponse>
{
    public override async Task HandleAsync(CreateSectionCommand command, CancellationToken ct)
    {
        var journal = await _repository.GetByIdOrThrowAsync(command.JournalId);
        if(journal.Sections.Any(s => s.Name.EqualsIgnoreCase(command.Name)))
            throw new BadRequestException("Section with the same name already exists");

				var section = command.Adapt<Section>();
				section.Id = await _repository.GenerateNewId<Section>();
				journal.Sections.Add(section);

				await _repository.ReplaceAsync(journal);

				await Send.OkAsync(new IdResponse(section.Id));
    }
}
