using Production.Persistence.Repositories;
using Production.API.Features.Shared;
using Production.Domain.StateMachines;
using Blocks.EntityFrameworkCore;
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

        var typesetter = await _dbContext.Typesetters.FindByIdOrThrowAsync(command.TypesetterId);

        _article.AssignTypesetter(typesetter, _stateMachineFactory, command);

        await _articleRepository.SaveChangesAsync();

        await Send.OkAsync(new IdResponse(command.ArticleId));
    }
}
