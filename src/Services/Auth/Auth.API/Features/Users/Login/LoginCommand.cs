namespace Auth.API.Features.Users.Login;

public record LoginCommand(string Email, string Password);
public record LoginResponse(string Email, string JWTToken, string RefreshToken);