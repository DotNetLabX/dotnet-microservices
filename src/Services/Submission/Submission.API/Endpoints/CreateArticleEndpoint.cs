using Submission.Application.Features.CreateArticle;

namespace Submission.API.Endpoints;

public static class CreateArticleEndpoint
{
		public static void Map(this IEndpointRouteBuilder app)
		{
				app.MapPost("/articles", async (CreateArticleCommand command, ISender sender) =>
				{
						var response = await sender.Send(command);
						return Results.Created($"/api/articles/{response.Id}", response);
				})
				.RequireRoleAuthorization(Role.Author)
				.WithName("CreateArticle")
				.WithTags("Articles")
				.Produces<IdResponse>(StatusCodes.Status201Created)
				.ProducesValidationProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}