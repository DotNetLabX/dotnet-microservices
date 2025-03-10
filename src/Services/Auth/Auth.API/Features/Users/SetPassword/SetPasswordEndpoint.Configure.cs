namespace Auth.API.Features.Users.SetPassword;

public partial class SetPasswordEndpoint
{
		public override void Configure()
		{
				AllowAnonymous();
				Post("/password/first-time", "/password/reset");

				Description(x => x
						.WithSummary("Set or reset user password")
						.WithTags("Password")
						.Produces<SetPasswordResponse>(StatusCodes.Status200OK)
						.ProducesProblem(StatusCodes.Status400BadRequest)
						.ProducesProblem(StatusCodes.Status404NotFound)
						.ProducesProblem(StatusCodes.Status401Unauthorized)
						);
		}
}
