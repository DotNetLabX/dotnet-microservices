using Microsoft.AspNetCore.Authorization;
using Grpc.Core;
using Blocks.Exceptions;
using Blocks.Redis;
using Articles.Security;
using Auth.Grpc;
using Journals.API.Features.Shared;
using Journals.Domain.Journals.Events;

namespace Journals.API.Features.Journals.Create;

[Authorize(Roles = Role.EditorAdmin)]
[HttpPost("journals")]
[Tags("Journals")]
public class CreateJournalEndpoint(Repository<Journal> _journalRepository, Repository<Editor> _editorRepository, IPersonService _personClient)
    : Endpoint<CreateJournalCommand, IdResponse>
{
    public override async Task HandleAsync(CreateJournalCommand command, CancellationToken ct)
    {
				if (_journalRepository.Collection.Any(j => j.Abbreviation == command.Abbreviation ||  j.NormalizedName == command.NormalizedName ))
            throw new BadRequestException("Journal with the same name or abbreviation already exists");

        if (!_editorRepository.Collection.Any(e => e.Id == command.ChiefEditorId))
            await CreateEditor(command.ChiefEditorId, ct);

				var journal = command.Adapt<Journal>();

        await _journalRepository.AddAsync(journal);

        await PublishAsync(new JournalCreated(journal));

        await Send.OkAsync(new IdResponse(journal.Id));
    }

		private async Task<Editor> CreateEditor(int userId, CancellationToken ct)
		{
				var response = await _personClient.GetPersonByUserIdAsync( new GetPersonByUserIdRequest { UserId = userId }, new CallOptions(cancellationToken: ct));
        var personInfo = response.PersonInfo;

				var editor = Editor.Create(personInfo); 
				await _editorRepository.AddAsync(editor);

				return editor;
		}
}
