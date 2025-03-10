using Articles.IntegrationEvents.Contracts.Articles.Dtos;

namespace Articles.Abstractions.Events;

public record ArticlePublishedEvent(ArticleDto Article);