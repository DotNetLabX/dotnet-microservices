namespace Production.Domain.Articles.Events;

public record ArticleAcceptedForProduction(Article Article, IArticleAction action)
    : DomainEvent(action);
