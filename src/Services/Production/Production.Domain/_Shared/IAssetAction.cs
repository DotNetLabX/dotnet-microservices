using Production.Domain.Assets.Enums;

namespace Production.Domain.Shared;

public interface IAssetAction : IArticleAction<AssetActionType>;