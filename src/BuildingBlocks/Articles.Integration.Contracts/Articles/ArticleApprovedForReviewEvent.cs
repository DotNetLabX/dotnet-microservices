using Articles.IntegrationEvents.Contracts.Articles.Dtos;

//namespace Articles.Integration.Contracts.Articles;
namespace Articles.IntegrationEvents.Contracts.Articles;

public record ArticleApprovedForReviewEvent(ArticleDto Article);
