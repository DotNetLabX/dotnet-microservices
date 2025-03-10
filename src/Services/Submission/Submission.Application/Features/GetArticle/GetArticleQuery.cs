using Submission.Application.Dtos;

namespace Submission.Application.Features.GetArticle;

public record GetArticleQuery(int ArticleId) : IQuery<GetArticleResonse>;
public record GetArticleResonse(ArticleDto ArticleSummary);

public class GetArticleValidator : AbstractValidator<GetArticleQuery>
{
		public GetArticleValidator()
		{
				RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof(GetArticleQuery.ArticleId));
		}
}