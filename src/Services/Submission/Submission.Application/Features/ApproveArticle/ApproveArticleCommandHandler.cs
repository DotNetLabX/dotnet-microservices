using Auth.Grpc;
using Blocks.Exceptions;
using Journals.Grpc;

namespace Submission.Application.Features.ApproveArticle;

public class ApproveArticleCommandHandler
		(ArticleRepository _articleRepository, PersonRepository _personRepository, ArticleStateMachineFactory _stateMachineFactory, IPersonService _personClient, IJournalService _journalClient)
		: IRequestHandler<ApproveArticleCommand, IdResponse>
{
		public async Task<IdResponse> Handle(ApproveArticleCommand command, CancellationToken ct)
		{
				var article = await _articleRepository.FindByIdOrThrowAsync(command.ArticleId);

				if (!await IsEditorAssignedToJournal(article.JournalId, command.CreatedById))
						throw new BadRequestException($"Editor is not assigned to the article's Journal (Id: {article.JournalId})");

				var editor = await GetOrCreatePersonByUserId(command.CreatedById, command, ct);

				article.Approve(editor, command, _stateMachineFactory);

				await _articleRepository.SaveChangesAsync();

				return new IdResponse(article.Id);
		}

		private async Task<bool> IsEditorAssignedToJournal(int journalId, int userId)
		{
				var response = await _journalClient.IsEditorAssignedToJournalAsync(new IsEditorAssignedToJournalRequest
				{
						JournalId = journalId,
						UserId = userId
				});

				return response.IsAssigned;
		}

		private async Task<Person> GetOrCreatePersonByUserId(int userId, IArticleAction action, CancellationToken ct)
		{
				var person = await _personRepository.GetByUserIdAsync(userId);
				if (person is null)
				{
						var response = await _personClient.GetPersonByUserIdAsync(new GetPersonByUserIdRequest{ UserId = userId });
						var personInfo = response.PersonInfo;
						person = Person.Create(personInfo, action);
						await _personRepository.AddAsync(person, ct);
				}

				return person;
		}
}
