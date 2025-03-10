using Auth.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Submission.Application.Features.CreateAndAssignAuthor;

public class CreateAndAssignAuthorCommandHandler(ArticleRepository _articleRepository, IPersonService _personClient, SubmissionDbContext _dbContext)
		: IRequestHandler<CreateAndAssignAuthorCommand, IdResponse>
{
		public async Task<IdResponse> Handle(CreateAndAssignAuthorCommand command, CancellationToken ct)
		{
				var article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId);

				Author? author;
				if (command.PersonId == null) // new author, new person
						author = await GetOrCreateAuthorByEmailAsync(command, ct);
				else
						author = await GetOrCreateAuthorByPersonIdAsync(command, ct);

				article.AssignAuthor(author, command.ContributionAreas, command.IsCorrespondingAuthor, command);

				await _articleRepository.SaveChangesAsync(ct);

				return new IdResponse(article.Id);
		}

		private async Task<Author> GetOrCreateAuthorByEmailAsync(CreateAndAssignAuthorCommand command, CancellationToken ct)
		{
				var author = await _dbContext.Authors.SingleOrDefaultAsync(x => x.Email.Value == command.Email, ct);
				if (author is null)
				{
						var createPersonRequest = command.Adapt<CreatePersonRequest>();
						var response = await _personClient.GetOrCreatePersonAsync(createPersonRequest, new CallOptions(cancellationToken: ct));
						author = Author.Create(response.PersonInfo, command);
				}

				return author;
		}

		private async Task<Author> GetOrCreateAuthorByPersonIdAsync(CreateAndAssignAuthorCommand command, CancellationToken ct)
		{
				var author = await _dbContext.Authors.SingleOrDefaultAsync(x => x.Id == command.PersonId, ct);
				if (author is null)
				{
						var response = await _personClient.GetPersonByIdAsync(new GetPersonRequest { PersonId = command.PersonId!.Value }, new CallOptions(cancellationToken: ct));
						author = Author.Create(response.PersonInfo, command);
				}

				return author;
		}
}
