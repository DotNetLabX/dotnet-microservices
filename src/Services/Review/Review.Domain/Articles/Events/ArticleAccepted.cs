namespace Review.Domain.Articles.Events;

public record ArticleAccepted(Article Article, IArticleAction action)
		: DomainEvent(action);