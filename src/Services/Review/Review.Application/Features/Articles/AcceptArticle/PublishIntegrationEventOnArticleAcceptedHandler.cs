using MassTransit;
using Articles.IntegrationEvents.Contracts.Articles;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Review.Domain.Articles.Events;

namespace Review.Application.Features.Articles.AcceptArticle;

public class PublishIntegrationEventOnArticleAcceptedHandler(ArticleRepository _articleRepository, IPublishEndpoint _publishEndpoint)
    : INotificationHandler<ArticleAccepted>
{
    public async Task Handle(ArticleAccepted notification, CancellationToken ct)
    {
        var article = await _articleRepository.GetFullArticleByIdAsync(notification.Article.Id);

        var articleDto = article.Adapt<ArticleDto>();

        await _publishEndpoint.Publish(new ArticleAcceptedForProductionEvent(articleDto), ct);
    }
}
