using Carter;
using ArticleHub.Domain.Dtos;
using ArticleHub.Persistence;

namespace ArticleHub.API.Articles.GetArticle;

public class GetArticleEndpoint : ICarterModule
{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
				app.MapGet("/articles/{articleId:int}", async (int articleId, ArticleGraphQLReadStore graphQLReadStore, CancellationToken ct) =>
				{
						var article = await graphQLReadStore.GetArticleById(articleId, ct);
						if(article == null)
								return Results.NotFound();
						else
								return Results.Ok(article);
				})
				.RequireAuthorization() // allows all authenticated users
				.WithName("GetArticle")
				.WithTags("Articles")
				.Produces<ArticleDto>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}
