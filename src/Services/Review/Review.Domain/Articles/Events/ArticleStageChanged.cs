namespace Review.Domain.Articles.Events;

public record ArticleStageChanged(ArticleStage CurrentStage, ArticleStage NewStage, IArticleAction action)
		: DomainEvent(action);