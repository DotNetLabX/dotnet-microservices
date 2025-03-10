using Review.Application.Features.Invitations.AcceptInvitation;

namespace Review.API.Endpoints.Invitations;

public class DeclineInvitationEndpoint : ICarterModule
{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
				app.MapPost("/articles/{articleId:int}/invitations/{token}:decline", async (int articleId, string token, DeclineInvitationCommand command, ISender sender) =>
				{
						var response = await sender.Send(command with { ArticleId = articleId, Token = token });
						return Results.Ok(response);
				})
				.AllowAnonymous()
				.WithName("Reject Invitation")
				.WithTags("Invitations")
				.Produces<IdResponse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}