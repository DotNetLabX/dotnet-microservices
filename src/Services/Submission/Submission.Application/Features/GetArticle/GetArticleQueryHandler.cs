using Submission.Application.Dtos;

namespace Submission.Application.Features.GetArticle;

public class GetArticleQueryHandler(ArticleRepository _articleRepository)
		: IRequestHandler<GetArticleQuery, GetArticleResonse>
{
		public async Task<GetArticleResonse> Handle(GetArticleQuery command, CancellationToken ct)
		{
				var article = Guard.NotFound(
						await _articleRepository.GetFullArticleByIdAsync(command.ArticleId));

				return new GetArticleResonse(article.Adapt<ArticleDto>());
		}
}