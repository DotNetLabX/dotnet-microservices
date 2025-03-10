using Articles.Security;
using Submission.Application.Features.ApproveArticle;

namespace Submission.API.Endpoints;

public static class ApproveArticleEndpoint
{
		public static void Map(this IEndpointRouteBuilder app)
		{
				app.MapPost("/articles/{articleId:int}:approve", async (int articleId, ApproveArticleCommand command, ISender sender) =>
				{
						var response = await sender.Send(command with { ArticleId = articleId });
						return Results.Ok(response);
				})
				.RequireRoleAuthorization(Role.Editor, Role.EditorAdmin)
				.WithName("ApproveArticle")
				.WithTags("Articles")
				.Produces<IdResponse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}
