// {Svc}.Domain/ValueObjects/{ValueObject}.cs
// A value object is structurally impossible to construct in an invalid state (domain-5).
// Lifted from src/Services/Submission/Submission.Domain/ValueObjects/EmailAddress.cs.

using Blocks.Core;                 // Guard
using Newtonsoft.Json;
// StringValueObject / SingleValueObject<T> come from Blocks.Entities (global using in {Svc}.Domain).

namespace {Svc}.Domain.ValueObjects;

// String-wrapping VO -> StringValueObject. A struct-wrapping VO -> SingleValueObject<T> (e.g. int, decimal).
public class {ValueObject} : StringValueObject
{
    [JsonConstructor]
    private {ValueObject}(string value) => Value = value;   // ctor is PRIVATE — no public path to an invalid value

    public static {ValueObject} Create(string value)
    {
        Guard.ThrowIfNullOrWhiteSpace(value);               // Guard.ThrowIf* runs FIRST, before construction
        Guard.ThrowIfFalse(IsValid(value), "Invalid {ValueObject} format.");

        return new {ValueObject}(value);
    }

    private static bool IsValid(string value) => /* invariant check */ true;
}
