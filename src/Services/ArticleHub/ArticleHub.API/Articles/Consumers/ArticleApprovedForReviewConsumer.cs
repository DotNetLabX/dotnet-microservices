using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Blocks.Mapster;
using ArticleHub.Persistence;
using Articles.IntegrationEvents.Contracts.Articles;
using ArticleHub.Domain.Entities;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Blocks.Exceptions;

namespace ArticleHub.API.Articles.Consumers;

public class ArticleApprovedForReviewConsumer(ArticleHubDbContext _dbContext) 
		: IConsumer<ArticleApprovedForReviewEvent>
{
		public async Task Consume(ConsumeContext<ArticleApprovedForReviewEvent> context)
		{
				var articleDto = context.Message.Article;

				if (await _dbContext.Articles.AnyAsync(a => a.Id == articleDto.Id, context.CancellationToken))
						throw new BadRequestException("Article was already approved for review.");

				var journal = await GetOrCreateJournalAsync(articleDto, context.CancellationToken);

				var article = articleDto.AdaptWith<Article>(article =>
				{
						article.Journal = journal;
						article.SubmittedById = articleDto.SubmittedBy.Id;
				});

				await CreateActorsAsync(articleDto, article, context.CancellationToken);

				await _dbContext.Articles.AddAsync(article);

				await _dbContext.SaveChangesAsync(context.CancellationToken);
		}

		private async Task<Journal> GetOrCreateJournalAsync(ArticleDto articleDto, CancellationToken ct = default)
		{
				var journal = await _dbContext.Journals.SingleOrDefaultAsync(j => j.Id == articleDto.Journal.Id, ct);
				if (journal is null)
				{
						journal = articleDto.Journal.Adapt<Journal>();
						await _dbContext.Journals.AddAsync(journal);
				}

				return journal;
		}

		private async Task CreateActorsAsync(ArticleDto articleDto, Article article, CancellationToken ct = default)
		{
				foreach (var actorDto in articleDto.Actors)
				{
						var person = await _dbContext.Persons.SingleOrDefaultAsync(p => p.Id == actorDto.Person.Id, ct);
						if (person is null)
						{
								person = actorDto.Person.Adapt<Person>();
								_dbContext.Persons.Add(person);
						}

						article.Actors.Add(new ArticleActor
						{
								ArticleId = article.Id,
								PersonId = person.Id,
								Role = actorDto.Role,
						});
				}
		}
}
