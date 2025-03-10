namespace Auth.API.Features.Users.RefreshToken;

public record RefreshTokenCommand(string RefreshToken);

public record RefreshTokenResponse(string Email, string JwtToken, string RefreshToken);
