using Review.Application.Features.Invitations.AcceptInvitation;

namespace Review.API.Endpoints.Invitations;

public class AcceptInvitationEndpoint : ICarterModule
{
		public void AddRoutes(IEndpointRouteBuilder app)
		{
				//app.MapPost("/articles/{articleId:int}/invitations:accept", 
				app.MapPost("/articles/{articleId:int}/invitations/{token}:accept",
						async (int articleId, string token, AcceptInvitationCommand command, ISender sender) =>
						{
								var response = await sender.Send(command with { ArticleId = articleId, Token = token });
								return Results.Ok(response);
						})
				.AllowAnonymous()
				.WithName("Accept Invitation")
				.WithTags("Invitations")
				.Produces<IdResponse>(StatusCodes.Status200OK)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.ProducesProblem(StatusCodes.Status404NotFound)
				.ProducesProblem(StatusCodes.Status401Unauthorized);
		}
}