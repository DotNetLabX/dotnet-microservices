using Articles.Abstractions.Enums;
using Auth.Domain.Persons.ValueObjects;
using Auth.Domain.Users;

namespace Auth.Domain.Persons;

public partial class Person : AggregateRoot
{
		public required string FirstName { get; set; }
		public required string LastName { get; set; }
		public string FullName => $"{FirstName} {LastName}";

		public required Gender Gender { get; set; }
		public HonorificTitle? Honorific { get; set; }
		public required EmailAddress Email { get; init; }
		public string? PictureUrl { get; set; } = null!;

		public ProfessionalProfile? ProfessionalProfile { get; set; }
		
		public int? UserId { get; set; }
		public User? User { get; set; }
}
