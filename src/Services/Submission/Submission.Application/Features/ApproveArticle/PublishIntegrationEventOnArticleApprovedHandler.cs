using MassTransit;
using Articles.IntegrationEvents.Contracts.Articles;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Submission.Domain.Events;

namespace Submission.Application.Features.ApproveArticle;

public class PublishIntegrationEventOnArticleApprovedHandler(ArticleRepository _articleRepository, IPublishEndpoint _publishEndpoint) 
		: INotificationHandler<ArticleApproved>
{
		public async Task Handle(ArticleApproved notification, CancellationToken ct)
		{
				var article = await _articleRepository.GetFullArticleByIdAsync(notification.Article.Id);

				var articleDto = article.Adapt<ArticleDto>(); 

				await _publishEndpoint.Publish(new ArticleApprovedForReviewEvent(articleDto), ct);
		}
}