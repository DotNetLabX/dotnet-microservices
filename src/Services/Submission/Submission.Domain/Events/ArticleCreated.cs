namespace Submission.Domain.Events;

public record ArticleCreated(Article Article, IArticleAction action)
    : DomainEvent(action);