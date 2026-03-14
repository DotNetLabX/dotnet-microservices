# Create Value Object

## Patterns

### StringValueObject (single string value)

**Reference:** `src/Services/Auth/Auth.Domain/Persons/ValueObjects/EmailAddress.cs`

```csharp
public class {Name} : StringValueObject
{
    internal {Name}(string value)
    {
        Value = value;
    }

    public static {Name} Create(string value)
    {
        Guard.ThrowIfNullOrWhiteSpace(value);
        // Additional validation
        return new {Name}(value);
    }

    // Optional: implicit conversions
    public static implicit operator {Name}(string value) => Create(value);
    public static implicit operator string({Name} obj) => obj.Value;
}
```

### SingleValueObject<T> (single typed value)

```csharp
public class {Name} : SingleValueObject<int>
{
    internal {Name}(int value) { Value = value; }
    public static {Name} Create(int value)
    {
        Guard.ThrowIfFalse(value > 0, "{Name} must be positive.");
        return new {Name}(value);
    }
}
```

### ValueObject (multi-property)

```csharp
public class {Name} : ValueObject
{
    public string {Prop1} { get; }
    public string {Prop2} { get; }

    internal {Name}(string prop1, string prop2) { ... }

    public static {Name} Create(string prop1, string prop2)
    {
        // Validate
        return new {Name}(prop1, prop2);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return {Prop1};
        yield return {Prop2};
    }
}
```

## EF Core mapping

Value objects are mapped with `OwnsOne`:
```csharp
builder.OwnsOne(e => e.Email, b =>
{
    b.Property(n => n.Value).HasColumnName(nameof(Person.Email)).HasMaxLength(MaxLength.C64);
    b.HasIndex(e => e.NormalizedEmail).IsUnique();
});
```

## Location

`{ServiceName}.Domain/{AggregateName}/ValueObjects/{ValueObjectName}.cs`
