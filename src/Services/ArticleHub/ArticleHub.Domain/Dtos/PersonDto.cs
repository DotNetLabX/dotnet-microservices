namespace ArticleHub.Domain.Dtos;

public record PersonDto(
		int Id,
		string Email, 
		string FirstName, 
		string LastName,
		int? UserId
);