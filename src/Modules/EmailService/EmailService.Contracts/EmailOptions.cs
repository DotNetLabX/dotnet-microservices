namespace EmailService.Contracts;

public class EmailOptions
{
		public string EmailServiceProvider { get; init; } = null!;
		public string EmailFromAddress { get; init; } = null!;
		public Smtp Smtp { get; init; } = null!;
}

public class Smtp
{
		public string Host { get; init; } = null!;
		public int Port { get; init; }
		public string Username { get; init; } = null!;
		public string Password { get; init; } = null!;
		public string DeliveryMethod { get; init; } = null!;
		public string PickupDirectoryLocation { get; init; } = null!;
		public bool UseSSL { get; init; }
}
