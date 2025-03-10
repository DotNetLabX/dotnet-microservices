namespace Submission.Application.Dtos;

public record PersonDto(string FirstName, string LastName, string Email, string? Affiliation, int? UserId);
