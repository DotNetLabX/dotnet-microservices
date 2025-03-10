namespace Production.Domain.Shared;

public class Person : Entity
{
		public required string FirstName { get; init; }
		public required string LastName { get; init; }
		public string FullName => FirstName + " " + LastName;

		public string? Title { get; init; }

		//todo use EmailAddress value object
		public required string Email { get; init; }
		
		public required string Affiliation { get; init; }

		public int? UserId { get; set; }

		public string TypeDiscriminator { get; init; } = null!; // EF discriminator
}
