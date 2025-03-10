using Articles.Abstractions.Enums;

namespace ArticleHub.Domain.Dtos;

public record ActorDto(
		string Role, 
		PersonDto Person
);
