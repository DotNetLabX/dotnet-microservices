using Production.Domain.Shared;

namespace Production.Domain.Articles;

public partial class Typesetter : Person
{
    public bool? IsDefault { get; set; }
    public string? CompanyName { get; set; }
}
