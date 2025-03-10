using Production.Persistence.Repositories;
using Articles.Abstractions.Enums;
using Production.API.Features.Shared;
using Production.Application.StateMachines;
using Blocks.EntityFrameworkCore;
using Blocks.Exceptions;
using Production.Domain.Shared.Enums;
using Production.Persistence;

namespace Production.API.Features.Articles.AssignTypesetter;

[Authorize(Roles = Role.ProdAdmin)]
[HttpPut("articles/{articleId:int}/typesetter/{typesetterId:int}")]
[Tags("Articles")]
public class AssignTypesetterEndpoint(ArticleRepository articleRepository, ArticleStateMachineFactory _stateMachineFactory, ProductionDbContext _dbContext) 
    : BaseEndpoint<AssignTypesetterCommand, IdResponse>(articleRepository)
{
		public override async Task HandleAsync(AssignTypesetterCommand command, CancellationToken ct)
    {
        _article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId);
								
				// todo move transisiton check to the domain ?!?
				CheckAndThrowStageTransition(command.ActionType);

				var typesetter = await _dbContext.Typesetters.FindByIdOrThrowAsync(command.TypesetterId);						

				_article.AssignTypesetter(typesetter, command);

				_article.SetStage(NextStage, command);

				await _articleRepository.SaveChangesAsync();

        await Send.OkAsync( new IdResponse(command.ArticleId));
    }


		//todo - hardcode the NextStage in the AssignTypesetter behavior
		protected override ArticleStage NextStage => ArticleStage.InProduction;

		protected virtual void CheckAndThrowStageTransition(ArticleActionType actionType)
		{
				if (!_stateMachineFactory(_article.Stage).CanFire(actionType))
						throw new BadRequestException("Action not allowed");
		}
}
