using Articles.Abstractions.Enums;

namespace Articles.Abstractions.Events;

public record ArticleStageChanged(
    ArticleStage CurrentStage,
    ArticleStage NewStage,
    IArticleAction Action
) : DomainEvent<IArticleAction>(Action);
