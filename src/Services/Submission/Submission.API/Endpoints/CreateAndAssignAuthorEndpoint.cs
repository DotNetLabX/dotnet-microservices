using Submission.Application.Features.CreateAndAssignAuthor;

namespace Submission.API.Endpoints;

public static class CreateAndAssignAuthorEndpoint
{
		public static void Map(this IEndpointRouteBuilder app)
		{
				app.MapPost("/articles/{articleId:int}/authors", async (int articleId, CreateAndAssignAuthorCommand command, ISender sender) =>
				{
						var response = await sender.Send(command with { ArticleId = articleId });
						return Results.Ok(response);
				})
				.RequireRoleAuthorization(Role.Author)
				.WithName("CreateAndAssignAuthor")
				.WithTags("Articles")
				.Produces<IdResponse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}


