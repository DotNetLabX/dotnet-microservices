namespace Submission.Domain.Events;

public record ArticleApproved(Article Article, IArticleAction Action)
		: DomainEvent(Action);