namespace Blocks.Entitities;

public abstract class StringValueObject : IValueObject, IEquatable<StringValueObject>, IEquatable<string>
{
    public string Value { get; protected set; } = default!;

    public bool Equals(StringValueObject? other) => other is not null && Value.Equals(other.Value);
    public bool Equals(string? other) => Value.Equals(other);
    public override bool Equals(object? obj) => obj is StringValueObject other ? Equals(other) : obj is string str && Equals(str);

    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public static bool operator ==(StringValueObject? left, StringValueObject? right) => Equals(left, right);
    public static bool operator !=(StringValueObject? left, StringValueObject? right) => !Equals(left, right);
}