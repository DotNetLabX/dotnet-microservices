namespace ArticleHub.Domain.Entities;

public class Person : Entity
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string FullName => FirstName + " " + LastName;

    public string? Honorific { get; init; }
    public required string Email { get; init; }

    public int? UserId { get; set; }
}
