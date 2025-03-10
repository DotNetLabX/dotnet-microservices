using Production.Domain.Assets.Enums;
using Production.Domain.Shared.Enums;

namespace Production.Domain.Articles.Events;

public record ArticleStageChanged<TActionType>(ArticleStage CurrentStage, ArticleStage NewStage, IArticleAction<TActionType> action)
		: DomainEvent<IArticleAction<TActionType>>(action)
		where TActionType : Enum;
		