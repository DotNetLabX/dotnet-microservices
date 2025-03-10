namespace Review.Domain.Articles.Events;

public record ArticleRejected(Article Article, IArticleAction action)
		: DomainEvent(action);
