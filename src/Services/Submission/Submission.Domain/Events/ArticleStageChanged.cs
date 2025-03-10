namespace Submission.Domain.Events;

public record ArticleStageChanged(ArticleStage CurrentStage, ArticleStage NewStage, IArticleAction action)
		: DomainEvent(action);