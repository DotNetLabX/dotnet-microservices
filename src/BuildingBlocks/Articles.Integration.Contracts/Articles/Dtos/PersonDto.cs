namespace Articles.IntegrationEvents.Contracts.Articles.Dtos;

public record PersonDto(
		int Id, 
		string FirstName, 
		string LastName, 
		string Email, 
		string? Honorific,
		string? Affiliation,
		int? UserId
		);