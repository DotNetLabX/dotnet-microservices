namespace Auth.API.Features.Users.RefreshToken;

public class RefreshTokenCommandValidator : Validator<RefreshTokenCommand>
{
		public RefreshTokenCommandValidator()
		{
				RuleFor(x => x.RefreshToken)
						.NotEmpty().WithMessage("Refresh token is required.");
		}
}
