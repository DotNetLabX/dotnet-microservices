namespace Auth.API.Features.Users.Me;

public record MeResponse(int UserId, string Email, string FullName, IReadOnlyList<string> Roles);