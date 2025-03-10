using Review.Application.Features.Articles.AssignEditor;

namespace Review.API.Endpoints.Articles;

public class AssignEditorEndpoint : ICarterModule
{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
				app.MapPost("/articles/{articleId:int}/editor/{editorId:int}", async (int articleId, int editorId, AssignEditorCommand command, ISender sender) =>
				{
						var response = await sender.Send(command with { ArticleId = articleId, EditorId = editorId});
						return Results.Ok(response);
				})
				.RequireRoleAuthorization(Role.EditorAdmin)
				.WithName("AssignEditor")
				.WithTags("Articles")
				.Produces<IdResponse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}