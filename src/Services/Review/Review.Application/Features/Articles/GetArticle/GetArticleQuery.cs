using Review.Application.Dtos;

namespace Review.Application.Features.Articles.GetArticle;

public record GetArticleQuery(int ArticleId) : IQuery<GetArticleResonse>;
public record GetArticleResonse(ArticleDto ArticleSummary);

public class GetArticleValidator : AbstractValidator<GetArticleQuery>
{
    public GetArticleValidator()
    {
        RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof(GetArticleQuery.ArticleId));
    }
}