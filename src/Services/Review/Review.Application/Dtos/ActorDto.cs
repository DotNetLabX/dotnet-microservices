namespace Review.Application.Dtos;

public record ActorDto(UserRoleType Role, PersonDto Person, HashSet<ContributionArea> ContributionAreas);
