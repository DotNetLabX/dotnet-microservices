using Articles.Abstractions.Enums;

namespace Articles.IntegrationEvents.Contracts.Persons;

public record PersonDto(
		int Id, 
		string FirstName, 
		string LastName, 
		string Email, 
		Gender Gender, 
		string? Honorific,
		//ProfessionalProfile? ProfessionalProfile
		string? PictureUrl, 
		int? UserId
		);
