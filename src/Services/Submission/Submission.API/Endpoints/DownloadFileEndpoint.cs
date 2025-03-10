using Submission.Application.Features.DownloadFile;

namespace Submission.API.Endpoints;

public static class DownloadFileEndpoint
{
		public static void Map(this IEndpointRouteBuilder app)
		{
				app.MapGet("/articles/{articleId:int}/assets/{assetId:int}:download", async ([AsParameters] DownloadFileQuery query, ISender sender) =>
				{
						var result = await sender.Send(query);
						return Results.File(result.Stream, result.ContentType, result.FileName);
				})
				.RequireRoleAuthorization(Role.Author, Role.Editor, Role.EditorAdmin)
				.WithName("Download")
				.WithTags("Assets")
				.Produces(StatusCodes.Status200OK)
				.Produces(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}
