namespace Blocks.Entitities;

public abstract class StringValueObject : IValueObject, IEquatable<StringValueObject>, IEquatable<string>
{
		public string Value { get; protected set; } = default!;

		public bool Equals(StringValueObject? other) => Value.Equals(other?.Value);
		public bool Equals(string? other) => Value.Equals(other);

		public override int GetHashCode() => Value.GetHashCode();
		public override string ToString() => Value.ToString();		
}