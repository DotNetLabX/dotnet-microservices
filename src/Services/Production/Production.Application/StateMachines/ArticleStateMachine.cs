using Production.Domain.Articles;
using Production.Domain.Shared.Enums;
using Stateless;

namespace Production.Application.StateMachines;

public delegate ArticleStateMachine ArticleStateMachineFactory(ArticleStage articleStage);

public class ArticleStateMachine
{
		private StateMachine<ArticleStage, ArticleActionType> _stateMachine;

		public ArticleStateMachine(ArticleStage articleStage, ProductionDbContext _dbContext)
    {
				 _stateMachine = new(articleStage);
				var transitions = _dbContext.GetAllCached<ArticleStageTransition>();

				foreach (var transition in transitions)
				{
						_stateMachine.Configure(transition.CurrentStage)
								.Permit(transition.ActionType, transition.DestinationStage);
				}
		}

		public bool CanFire(ArticleActionType actionType) 
				=> _stateMachine.CanFire(actionType);
}