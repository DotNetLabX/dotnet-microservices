using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ArticleHub.Persistence;
using Articles.IntegrationEvents.Contracts.Articles;
using ArticleHub.Domain.Entities;
using Articles.Abstractions.Enums;
using Blocks.EntityFrameworkCore;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;

namespace ArticleHub.API.Articles.Consumers;

public sealed class ArticleAcceptedForProductionConsumer(ArticleHubDbContext _dbContext)
		: IConsumer<ArticleAcceptedForProductionEvent>
{
		public async Task Consume(ConsumeContext<ArticleAcceptedForProductionEvent> ctx)
		{
				var articleDto = ctx.Message.Article;

				// Must already exist from ApprovedForReview
				var article = await _dbContext.Articles
						.Include(a => a.Actors)
						.SingleOrThrowAsync(a => a.Id == articleDto.Id);

				// Update only fields that can change during Review
				article.Title = articleDto.Title;
				article.Stage = articleDto.Stage; // should now be AcceptedForProduction

				// Adding reviewers
				await AddReviewers(articleDto, article);

				await _dbContext.SaveChangesAsync();
		}

		private async Task AddReviewers(ArticleDto articleDto, Article article)
		{
				foreach (var actorDto in articleDto.Actors.Where(a => a.Role == UserRoleType.REV))
				{
						var person = await _dbContext.Persons.FirstOrDefaultAsync(p => p.Id == actorDto.Person.Id);
						if (person is null)
						{
								person = actorDto.Person.Adapt<Person>();
								_dbContext.Persons.Add(person);
						}

						//article.Actors.Add(
						//		new ArticleActor { ArticleId = article.Id, PersonId = person.Id, Role = actorDto.Role });

						article.Actors.Add(
								new ArticleActor { Person = person, Role = actorDto.Role });
				}
		}
}
