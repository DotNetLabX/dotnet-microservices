namespace Blocks.Entitities;

public abstract class SingleValueObject<T> : IValueObject, IEquatable<SingleValueObject<T>>, IEquatable<T>
		where T : struct
{
    public T Value { get; protected set; } = default!;

		public override string ToString()  => Value.ToString()!;
		public override int GetHashCode()  => Value.GetHashCode();

		public virtual bool Equals(SingleValueObject<T>? other)
    {
        if (other is null)
            return false;

        return Value.Equals(other.Value);
    }
    public bool Equals(T other) => Value.Equals(other);
    public override bool Equals(object? other)  => Equals(other as SingleValueObject<T>);
}
