using Microsoft.AspNetCore.Mvc;
using Submission.Application.Features.SubmitArticle;

namespace Submission.API.Endpoints;

public static class SubmitArticleEndpoint
{
		public static void Map(this IEndpointRouteBuilder app)
		{
				app.MapPost("/articles/{articleId:int}:submit", async ([FromRoute] int articleId, [FromBody] SubmitArticleCommand command, ISender sender) =>
				{
						var response = await sender.Send(command with { ArticleId = articleId });
						return Results.Ok(response);
				})				
				.RequireRoleAuthorization(Role.Author)
				.WithName("SubmitArticle")
				.WithTags("Articles")
				.Produces<IdResponse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}
