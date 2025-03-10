using MassTransit;
using Blocks.Redis;
using Articles.IntegrationEvents.Contracts.Articles;
using Journals.Persistence;

namespace Journals.API.Features.Journals.Consumers;

public class ArticleApprovedForReviewConsumer(Repository<Journal> _journalRepository)
		: IConsumer<ArticleApprovedForReviewEvent>
{
		public async Task Consume(ConsumeContext<ArticleApprovedForReviewEvent> context)
		{
				var journal = await _journalRepository.GetByIdOrThrowAsync(context.Message.Article.Journal.Id);

				journal.ArticlesCount += 1;

				await _journalRepository.UpdateAsync(journal);
		}
}
