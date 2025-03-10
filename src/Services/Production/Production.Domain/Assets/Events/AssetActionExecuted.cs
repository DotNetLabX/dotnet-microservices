using Production.Domain.Assets.Enums;

namespace Production.Domain.Assets.Events;

public record AssetActionExecuted(IArticleAction<AssetActionType> action, ArticleStage CurrentStage, AssetType AssetType, int AssetNumber, File? File)
		: DomainEvent<IArticleAction<AssetActionType>>(action);