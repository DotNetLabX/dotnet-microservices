using Articles.IntegrationEvents.Contracts.Articles.Dtos;

namespace Articles.IntegrationEvents.Contracts.Articles;

public record ArticleAcceptedForProductionEvent(ArticleDto Article);
