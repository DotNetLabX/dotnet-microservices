using Review.Application.Features.Assets.DownloadFile;

namespace Review.API.Endpoints.Assets;

public class DownloadFileEndpoint : ICarterModule
{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
				app.MapGet("/articles/{articleId:int}/assets/{assetId:int}:download", async ([AsParameters] DownloadFileQuery query, ISender sender) =>
				{
						var result = await sender.Send(query);
						return Results.File(result.Stream, result.ContentType, result.FileName);
				})
				.RequireRoleAuthorization(Role.Author, Role.Editor, Role.Reviewer, Role.EditorAdmin)
				.WithName("Download")
				.WithTags("Assets")
				.Produces(StatusCodes.Status200OK)
				.Produces(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}
