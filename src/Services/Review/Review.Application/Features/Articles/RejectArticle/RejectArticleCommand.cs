using Review.Application.Features.Articles.Shared;
using Review.Domain.Shared.Enums;

namespace Review.Application.Features.Articles.RejectArticle;

public record RejectArticleCommand : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.RejectArticle;
}


public class RejectArticleCommandValidator : ArticleCommandValidator<RejectArticleCommand>;
