using Review.Application.Dtos;

namespace Review.Application.Features.Articles.GetArticle;

public class GetArticleQueryHandler(ArticleRepository _articleRepository)
    : IRequestHandler<GetArticleQuery, GetArticleResponse>
{
    public async Task<GetArticleResponse> Handle(GetArticleQuery command, CancellationToken ct)
    {
        var article = Guard.NotFound(
            await _articleRepository.GetFullArticleByIdAsync(command.ArticleId));

        return new GetArticleResponse(article.Adapt<ArticleDto>());
    }
}