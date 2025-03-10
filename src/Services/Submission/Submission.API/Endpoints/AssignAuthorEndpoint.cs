using Submission.Application.Features.AssignAuthor;

namespace Submission.API.Endpoints;

public static class AssignAuthorEndpoint
{
		public static void Map(this IEndpointRouteBuilder app)
		{
				app.MapPut("/articles/{articleId:int}/authors/{authorId:int}", async (int articleId, int authorId, AssignAuthorCommand command, ISender sender) =>
				{
						var response = await sender.Send(command with { ArticleId = articleId, AuthorId = authorId});
						return Results.Ok(response);
				})
				.RequireRoleAuthorization(Role.Author)
				.WithName("AssignAuthor")
				.WithTags("Articles")
				.Produces<IdResponse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}