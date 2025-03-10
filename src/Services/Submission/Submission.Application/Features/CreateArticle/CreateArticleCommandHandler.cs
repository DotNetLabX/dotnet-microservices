using Journals.Grpc;
using Microsoft.EntityFrameworkCore;

namespace Submission.Application.Features.CreateArticle;

public class CreateArticleCommandHandler(SubmissionDbContext _dbContext, Repository<Journal> _journalRepository, IJournalService _journalClient)
		: IRequestHandler<CreateArticleCommand, IdResponse>
{
		public async Task<IdResponse> Handle(CreateArticleCommand command, CancellationToken ct)
		{
				var journal = await _journalRepository.FindByIdAsync(command.JournalId);
				if (journal is null)
						journal = await CreateJournal(command);

				var article = journal.CreateArticle(command.Title, command.Type, command.Scope, command);

				await AssignCurrentUserAsAuthor(article, command);

				await _journalRepository.SaveChangesAsync(ct);

				return new IdResponse(article.Id);
		}

		private async Task AssignCurrentUserAsAuthor(Article article, CreateArticleCommand command)
		{
				var author = await _dbContext.Authors.SingleOrDefaultAsync(t => t.UserId == command.CreatedById);
				if (author is not null)
						article.AssignAuthor(author, [ContributionArea.OriginalDraft], isCorrespondingAuthor: true, command);
		}

		private async Task<Journal> CreateJournal(CreateArticleCommand command)
		{
				var response = await _journalClient.GetJournalByIdAsync(new GetJournalByIdRequest { JournalId = command.JournalId });

				var journal = Journal.Create(response.Journal, command);

				await _journalRepository.AddAsync(journal);

				return journal;
		}
}
