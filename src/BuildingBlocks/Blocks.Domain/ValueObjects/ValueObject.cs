namespace Blocks.Entitities;

public interface IValueObject : IDomainObject;

public abstract class ValueObject : IEquatable<ValueObject>, IValueObject
{
		protected abstract IEnumerable<object?> GetEqualityComponents();
		
		public override bool Equals(object? obj)
		{
				if (obj is null || obj.GetType() != GetType())
						return false;

				var other = (ValueObject)obj;

				return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
		}

		public override int GetHashCode()
		{
				return GetEqualityComponents()
						.Aggregate(1, (current, obj) =>
								current * 23 + (obj?.GetHashCode() ?? 0));
		}

		public bool Equals(ValueObject? other)
		{
				return 
						other is not null && GetType() == other.GetType()
						&& GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
		}

		public static bool operator ==(ValueObject? a, ValueObject? b) => Equals(a, b);
		public static bool operator !=(ValueObject? a, ValueObject? b) => !Equals(a, b);
}