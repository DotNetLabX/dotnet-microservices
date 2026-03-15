using FluentValidation;
using Production.API.Features.Shared;
using Production.Domain.Shared.Enums;

namespace Production.API.Features.Articles.AssignTypesetter;

public record AssignTypesetterCommand : ArticleCommand
{
    public int TypesetterId { get; init; }
    public override ArticleActionType ActionType => ArticleActionType.AssignTypesetter;
}

public class AssignTypesetterCommandValidator : ArticleCommandValidator<AssignTypesetterCommand>
{
    public AssignTypesetterCommandValidator()
    {
        RuleFor(r => r.ArticleId).GreaterThan(0);
        RuleFor(r => r.TypesetterId).GreaterThan(0);
    }
}
