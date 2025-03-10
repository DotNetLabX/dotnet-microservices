namespace Review.Domain.Articles.Events;

public record ArticleActionExecuted(Article Article, IArticleAction action)
		: DomainEvent(action);