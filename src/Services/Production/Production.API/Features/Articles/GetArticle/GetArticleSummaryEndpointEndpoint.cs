using Mapster;
using Production.Application.Dtos;
using Production.Persistence.Repositories;

namespace Production.API.Features.Articles.GetArticle;

[Authorize(Roles = $"{Role.ProdAdmin},{Role.Author},{Role.Typesetter}")]
[HttpGet("articles/{articleId:int}/summary")]
public class GetArticleSummaryEndpointEndpoint(ArticleRepository _articleRepository) : Endpoint<GetArticleAssetsQuery>
{
		public override async Task HandleAsync(GetArticleAssetsQuery command, CancellationToken ct)
		{
				var article = await _articleRepository.GetArticleSummaryById(command.ArticleId);

				await Send.OkAsync(new GetArticleSummaryResonse(article.Adapt<ArticleSummaryDto>()));
		}
}

