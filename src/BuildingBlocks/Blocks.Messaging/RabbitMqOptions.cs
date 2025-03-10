namespace Blocks.Messaging;

public class RabbitMqOptions
{
		public string Host { get; set; } = "localhost"; // Default to localhost
		public string UserName { get; set; } = "guest";  // Default RabbitMQ username
		public string Password { get; set; } = "guest";  // Default RabbitMQ password
		public string VirtualHost { get; set; } = "/"; // Default RabbitMQ virtual host
}