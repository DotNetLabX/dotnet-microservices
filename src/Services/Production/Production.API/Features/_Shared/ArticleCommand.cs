using FluentValidation;
using Blocks.Core;
using Production.Domain.Assets.Enums;
using Production.Domain.Shared.Enums;
using Production.Domain.Shared;

namespace Production.API.Features.Shared;

public abstract record ArticleCommand : ArticleCommandBase<ArticleActionType>, IArticleAction, ICommand<IdResponse>;
public abstract record ArticleCommand<TResponse> : ArticleCommandBase<ArticleActionType>, IArticleAction, ICommand<TResponse>;
public abstract record AssetCommand<TResponse> : ArticleCommandBase<AssetActionType>, IAssetAction, ICommand<TResponse>;

public abstract class ArticleCommandValidator<TCommand> : BaseValidator<TCommand>
    where TCommand : global::Articles.Abstractions.IArticleAction
{
    public ArticleCommandValidator()
    {
        RuleFor(command => command.ArticleId).GreaterThan(ValidatorsConstants.Id)
            .WithMessage(ValidatorsMessagesConstants.InvalidId.FormatWith("articleId"));
    }
}
