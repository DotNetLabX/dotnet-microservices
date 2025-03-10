using Blocks.Entitities;

namespace Production.Domain.Orders;

public abstract record ValueObject : IDomainObject;

public record Address : ValueObject
{
    public required string Value { get; set; }
    public required string City { get; set; }
    public string PostCode { get; set; } = default!;
}
