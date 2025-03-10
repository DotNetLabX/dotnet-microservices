using Blocks.Entitities;

namespace Production.Domain.Orders;

public record PhoneNumber(int CountryCode, long Number) : ValueObject;