using Review.Application.Features.Articles.GetArticle;

namespace Review.API.Endpoints.Articles;

public class GetArticleEndpoint : ICarterModule
{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
				app.MapGet("/articles/{articleId:int}", async ([AsParameters] GetArticleQuery query, ISender sender) =>
				{
						var article = await sender.Send(query);
						return Results.Ok(article);
				})
				.RequireRoleAuthorization(Role.Author, Role.Editor, Role.Reviewer, Role.EditorAdmin)
				.WithName("GetArticle")
				.WithTags("Articles")
				.Produces<GetArticleResonse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}
