using Stateless;
using Blocks.Core.Cache;
using Microsoft.Extensions.Caching.Memory;
using Review.Domain.Shared.Enums;

namespace Review.Application.StateMachines;

//todo - reimplement it directly with the database (cached), remove stateless library
public class ArticleStateMachine : IArticleStateMachine
{
    private StateMachine<ArticleStage, ArticleActionType> _stateMachine;
    public ArticleStateMachine(ArticleStage articleStage, IMemoryCache cache)
    {
        _stateMachine = new(articleStage);
        
        var transitions = cache.Get<List<ArticleStageTransition>>();
        foreach (var transition in transitions)
        {
            _stateMachine.Configure(transition.CurrentStage)
                .Permit(transition.ActionType, transition.DestinationStage);
        }
    }

    public bool CanFire(ArticleActionType actionType)
    {
        return _stateMachine.CanFire(actionType);
    }
}