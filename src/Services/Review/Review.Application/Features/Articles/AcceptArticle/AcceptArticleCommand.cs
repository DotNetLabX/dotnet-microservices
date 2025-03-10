using Review.Application.Features.Articles.Shared;
using Review.Domain.Shared.Enums;

namespace Review.Application.Features.Articles.AcceptArticle;

public record AcceptArticleCommand : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.AcceptArticle;
}


public class AcceptArticleCommandValidator : ArticleCommandValidator<AcceptArticleCommand>;
