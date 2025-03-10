using Submission.Application.Features.RejectArticle;

namespace Submission.API.Endpoints;

public static class RejectArticleEndpoint
{
		public static void Map(this IEndpointRouteBuilder app)
		{
				app.MapPost("/articles/{articleId:int}:reject", async (int articleId, RejectArticleCommand command, ISender sender) =>
				{
						var response = await sender.Send(command with { ArticleId = articleId });
						return Results.Ok(response);
				})
				.RequireRoleAuthorization(Role.Editor, Role.EditorAdmin)
				.WithName("RejectArticle")
				.WithTags("Articles")
				.Produces<IdResponse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}
