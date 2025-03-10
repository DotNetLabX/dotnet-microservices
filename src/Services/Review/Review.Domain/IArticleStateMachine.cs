using Blocks.Domain;
using Review.Domain.Shared.Enums;

namespace Review.Domain.StateMachines;

public interface IArticleStateMachine
{
		bool CanFire(ArticleActionType actionType);
}

public delegate IArticleStateMachine ArticleStateMachineFactory(ArticleStage articleStage);

public static class Extensions
{
		public static void ValidateStageTransition(this ArticleStateMachineFactory factory, ArticleStage articleStage, ArticleActionType actionType)
		{
				var stateMachine = factory(articleStage);

				if (!stateMachine.CanFire(actionType))
						throw new DomainException($"Action {actionType} not allowed in the {articleStage} article stage");
		}
}
