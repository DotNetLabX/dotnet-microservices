using Articles.Abstractions.Enums;

namespace Production.Application.Dtos;

public record ContributorDto(UserRoleType Role, PersonDto Person);
