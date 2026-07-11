using Production.Domain.Assets.Enums;

namespace Production.Domain.StateMachines;

public interface IAssetStateMachine
{
    bool CanFire(ArticleStage articleStage, AssetType assetType, AssetActionType actionType);
}

public delegate IAssetStateMachine AssetStateMachineFactory(AssetState assetState);
