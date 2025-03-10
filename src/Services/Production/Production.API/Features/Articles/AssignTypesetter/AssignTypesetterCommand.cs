using Blocks.EntityFrameworkCore;
using FluentValidation;
using Production.API.Features.Shared;
using Production.Application.StateMachines;
using Production.Domain.Shared.Enums;
using Production.Persistence.Repositories;

namespace Production.API.Features.Articles.AssignTypesetter;

public record AssignTypesetterCommand : ArticleCommand
{
		public int TypesetterId { get; init; }
		public override ArticleActionType ActionType => ArticleActionType.AssignTypesetter;
}

//todo - add validation for all commands following the Submission approach
//todo - remove Action validation from all commands, it will be executed in the handlers or aggregates
public class AssignTypesetterCommandValidator : ArticleCommandValidator<AssignTypesetterCommand>
{
		public AssignTypesetterCommandValidator()
		{
				RuleFor(r => r.ArticleId).GreaterThan(0);
				RuleFor(r => r.TypesetterId).GreaterThan(0);

				// todo remove the stage transion check fromm here.  keep it only in the domain
				RuleFor(r => r).MustAsync(async (r, _, cancellation) => await IsActionValid(r))
						.WithMessage("Action not allowed");
		}

		protected virtual async Task<bool> IsActionValid(ArticleCommand action)
		{
				var articleRepository = Resolve<ArticleRepository>();
				var article = await articleRepository.GetByIdOrThrowAsync(action.ArticleId);
				
				var stateMachineFactory = Resolve<ArticleStateMachineFactory>();
				var stateMachine = stateMachineFactory(article.Stage);

				return stateMachine.CanFire(action.ActionType);
		}
}