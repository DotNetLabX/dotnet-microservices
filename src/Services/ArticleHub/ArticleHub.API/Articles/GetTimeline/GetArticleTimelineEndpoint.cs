using Carter;
using Blocks.EntityFrameworkCore;
using ArticleHub.Domain.Entities;
using ArticleHub.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ArticleHub.API.Articles.GetArticleTimeline;

public class GetArticleTimelineEndpoint : ICarterModule
{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
				app.MapGet("/articles/{articleId:int}/timeline", async (int articleId, [FromServices] Repository<Article> repository, CancellationToken ct) =>
				{
						//todo - replace it with the actual timeline
						var article = await repository.GetByIdOrThrowAsync(articleId, ct);

						return Results.Ok(article);
				})
				.RequireAuthorization() // allows all authenticated users
				.WithName("GetArticleTimeline")
				.WithTags("Articles")
				.Produces<Article>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}
