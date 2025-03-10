namespace Auth.Domain.Persons.ValueObjects;

public sealed class ProfessionalProfile : ValueObject
{
    public string? Position { get; private set; }
    public string? CompanyName { get; private set; }
    public string? Affiliation { get; private set; }

    private ProfessionalProfile() { } // hide the ctor, needed by EF Core

    public static ProfessionalProfile Create(string? position, string? companyName, string? affiliation)
    {
        return new ProfessionalProfile()
        {
            Position = string.IsNullOrWhiteSpace(position) ? null : position.Trim(),
            CompanyName = string.IsNullOrWhiteSpace(companyName) ? null : companyName.Trim(),
            Affiliation = string.IsNullOrWhiteSpace(affiliation) ? null : affiliation.Trim()
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Position;
        yield return CompanyName;
        yield return Affiliation;
    }

    public override string ToString() =>
            $"{Position}{(string.IsNullOrEmpty(Position) || string.IsNullOrEmpty(CompanyName) ? "" : " @ ")}{CompanyName}".Trim();
}
