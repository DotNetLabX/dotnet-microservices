using Submission.Application.Features.GetArticle;

namespace Submission.API.Endpoints;

public static class GetArticleEndpoint
{
		public static void Map(this IEndpointRouteBuilder app)
		{
				app.MapGet("/articles/{articleId:int}", async ([AsParameters] GetArticleQuery query, ISender sender) =>
				{
						var article = await sender.Send(query);
						return Results.Ok(article);
				})
				.RequireRoleAuthorization(Role.Author, Role.Editor, Role.EditorAdmin)
				.WithName("GetArticle")
				.WithTags("Articles")
				.Produces<GetArticleResonse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}
