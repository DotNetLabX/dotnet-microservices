using Articles.Abstractions.Enums;
using Microsoft.EntityFrameworkCore;
using Production.Domain.Assets.Enums;
using Production.Domain.Assets;
using Production.Persistence;
using Stateless;

namespace Production.Application.StateMachines;

public delegate AssetStateMachine AssetStateMachineFactory(AssetState assetState);

public class AssetStateMachine
{
		private readonly StateMachine<AssetState, AssetActionType> _stateMachine;
		private readonly IEnumerable<AssetStateTransitionCondition> _conditions;
		private readonly Dictionary<AssetActionType, 
				StateMachine<AssetState, AssetActionType>.TriggerWithParameters<ArticleStage, AssetType>> _triggers = new(); 


		public AssetStateMachine(AssetState assetState, ProductionDbContext dbContext)
		{
				_stateMachine = new StateMachine<AssetState, AssetActionType>(assetState);

				var transitions = dbContext.GetAllCached<AssetStateTransition>();
				_conditions = dbContext.GetAllCached<AssetStateTransitionCondition>();

				foreach (var transition in transitions)
				{
						var trigger = _triggers.GetValueOrDefault(transition.ActionType);
						if (trigger == null)
								trigger = _stateMachine.SetTriggerParameters<ArticleStage, AssetType>(transition.ActionType);

						_triggers[transition.ActionType] = trigger;

						if(transition.CurrentState != transition.DestinationState)
								_stateMachine.Configure(transition.CurrentState)
										.PermitIf(trigger, transition.DestinationState,
												(stage, assetType) => CanPerform(stage, assetType, transition.ActionType));
						else
								_stateMachine.Configure(transition.CurrentState)
										.PermitReentryIf(trigger, 
												(stage, assetType) => CanPerform(stage, assetType, transition.ActionType));

				}
		}

		private bool CanPerform(ArticleStage articleStage, AssetType assetType, AssetActionType actionType)
		{
				return _conditions
						.Any(c=> c.ArticleStage == articleStage && c.AssetTypes.Contains(assetType) && c.ActionTypes.Contains(actionType));
		}

		public bool CanFire(ArticleStage articleStage, AssetType assetType, AssetActionType actionType)
		{
				var trigger = _triggers[actionType];
				return _stateMachine.CanFire(trigger, articleStage, assetType);
		}
}