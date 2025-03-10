using Articles.Abstractions.Enums;

namespace Articles.Abstractions;

public interface IPersonCreationInfo
{
		string Email { get; }
		string FirstName { get; }
		string LastName { get; }
		Gender Gender { get; }

		Honorific? Honorific { get; }
		string? PictureUrl { get; }
		string? CompanyName { get; }
		string? Position { get; }
		string? Affiliation { get; }
}