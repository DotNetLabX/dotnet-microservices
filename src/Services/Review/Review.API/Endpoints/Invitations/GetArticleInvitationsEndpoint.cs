using Review.Application.Features.Invitations.GetArticleInvitations;

namespace Review.API.Endpoints.Invitations;

public class GetArticleInvitationsEndpoint : ICarterModule
{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
				app.MapGet("/articles/{articleId:int}/invitations", async ([AsParameters] GetArticleInvitationsQuery query, ISender sender) =>
				{
						var article = await sender.Send(query);
						return Results.Ok(article);
				})
				.RequireRoleAuthorization(Role.Editor, Role.EditorAdmin)
				.WithName("GetArticleInvitations")
				.WithTags("Invitations")
				.Produces<GetArticleInvitationsResonse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}
