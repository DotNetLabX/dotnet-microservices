namespace Blocks.Core.Security;

public record JwtOptions
{
		public required string Issuer { get; init; }

		public required string Audience { get; init; }

		public required string Secret { get; init; }

		public int ValidForInMinutes { get; set; }
		
		//todo introduce the next property, the token is invalid without it
		//public SigningCredentials SigningCredentials { get; set; }

		public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

		public TimeSpan ValidFor => TimeSpan.FromMinutes(ValidForInMinutes);

		public DateTime Expiration => IssuedAt.Add(ValidFor);

}
