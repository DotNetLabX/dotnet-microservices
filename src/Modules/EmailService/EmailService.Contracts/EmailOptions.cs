using System.ComponentModel.DataAnnotations;

namespace EmailService.Contracts;

public class EmailOptions
{
    [Required]
    public string EmailServiceProvider { get; init; } = null!;
    [Required]
    public string EmailFromAddress { get; init; } = null!;
    [Required]
    public Smtp Smtp { get; init; } = null!;
}

public class Smtp
{
    [Required]
    public string Host { get; init; } = null!;
    [Required]
    public int Port { get; init; }
    [Required]
    public string Username { get; init; } = null!;
    [Required]
    public string Password { get; init; } = null!;
    public string DeliveryMethod { get; init; } = null!;
    public string PickupDirectoryLocation { get; init; } = null!;
    public bool UseSSL { get; init; } = true;
}
