using Grpc.Core;
using Blocks.Exceptions;
using Auth.Grpc;

namespace Submission.Application.Features.CreateAuthor;

public class CreateAuthorCommandHandler(Repository<Person> _personRepository, IPersonService _personClient)
		: IRequestHandler<CreateAuthorCommand, IdResponse>
{
		public async Task<IdResponse> Handle(CreateAuthorCommand command, CancellationToken ct)
		{
				if (await _personRepository.ExistsAsync(p => p.Email.Value == command.Email, ct))
						throw new BadRequestException("Author with this email already exists.");

				var personInfo = await CreatePerson(command, ct);

				var author = Author.Create(personInfo, command);

				await _personRepository.AddAsync(author);

				await _personRepository.SaveChangesAsync();

				return new IdResponse(author.Id);
		}

		private async Task<PersonInfo> CreatePerson(CreateAuthorCommand command, CancellationToken ct)
		{
				var createPersonRequest = command.Adapt<CreatePersonRequest>();
				var response = await _personClient.GetOrCreatePersonAsync(createPersonRequest, new CallOptions(cancellationToken: ct));
				return response.PersonInfo;
		}
}
