using Production.Domain.Shared.Enums;

namespace Production.Domain.StateMachines;

public interface IArticleStateMachine
{
    bool CanFire(ArticleActionType actionType);
}

public delegate IArticleStateMachine ArticleStateMachineFactory(ArticleStage articleStage);
