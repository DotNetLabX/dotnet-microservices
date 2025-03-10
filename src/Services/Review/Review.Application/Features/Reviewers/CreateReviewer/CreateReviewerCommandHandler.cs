using Auth.Grpc;
using Blocks.Exceptions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Review.Domain.Reviewers;
using Review.Persistence;

namespace Review.Application.Features.Reviewers.CreateReviewer;

public class CreateReviewerCommandHandler(ReviewDbContext _dbContext, IPersonService _personClient)
        : IRequestHandler<CreateReviewerCommand, IdResponse>
{
    public async Task<IdResponse> Handle(CreateReviewerCommand command, CancellationToken ct)
		{
				var exists = await _dbContext.Reviewers.AnyAsync(p => p.Email.Value.EqualsIgnoreCase(command.Email) && nameof(Reviewer) == p.TypeDiscriminator);
				if (exists)
						throw new BadRequestException("A reviewer with this email already exists.");

				PersonInfo? personInfo = default;
				if (command.UserId is not null)
						personInfo = await GetPersonByUserId(command.UserId.Value, ct);
				else
						personInfo = await CreatePersonAsync(command, ct);

				var reviewer = await CreateReviewerFromPerson(personInfo, command.Specializations, command, ct);

				await _dbContext.Reviewers.AddAsync(reviewer);

				await _dbContext.SaveChangesAsync();

				return new IdResponse(reviewer.Id);
		}

		private async Task<PersonInfo> GetPersonByUserId(int userId, CancellationToken ct)
		{
				var response = await _personClient.GetPersonByUserIdAsync(new GetPersonByUserIdRequest { UserId = userId }, new CallOptions(cancellationToken: ct));
				return response.PersonInfo;
		}

		private async Task<Reviewer> CreateReviewerFromPerson(PersonInfo personInfo, IEnumerable<int> journalIds, Domain.Shared.IArticleAction action, CancellationToken ct)
		{
				var reviewer = Reviewer.Create(personInfo, journalIds, action);
				await _dbContext.Reviewers.AddAsync(reviewer, ct);

				return reviewer;
		}

		private async Task<PersonInfo> CreatePersonAsync(CreateReviewerCommand command, CancellationToken ct)
		{
				var createPersonRequest = command.Adapt<CreatePersonRequest>();
				var response = await _personClient.GetOrCreatePersonAsync(createPersonRequest, new CallOptions(cancellationToken: ct));
				return response.PersonInfo;
		}
}
