using Production.Application.Dtos;

namespace Production.API.Features.Articles.GetArticle;

public record GetArticleAssetsQuery(int ArticleId);
public record GetArticleAssetsResponse(IReadOnlyList<AssetDto> Assets);

