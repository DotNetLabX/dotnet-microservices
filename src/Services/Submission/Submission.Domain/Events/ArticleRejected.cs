namespace Submission.Domain.Events;

public record ArticleRejected(Article Article, IArticleAction action)
		: DomainEvent(action);