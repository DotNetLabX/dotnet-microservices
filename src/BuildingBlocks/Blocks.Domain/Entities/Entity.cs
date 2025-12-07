using System.Reflection;

namespace Blocks.Entitities;

public interface IEntity : IEntity<int>;
public abstract class Entity : Entity<int>, IEntity;

public interface IEntity<TPrimaryKey> : IDomainObject
    where TPrimaryKey : struct
{
    TPrimaryKey Id { get; }
}
public abstract class Entity<TPrimaryKey> : IEntity<TPrimaryKey>, IEquatable<Entity<TPrimaryKey>>
    where TPrimaryKey : struct
{
    public virtual TPrimaryKey Id { get; init; }

    public virtual bool IsNew => EqualityComparer<TPrimaryKey>.Default.Equals(Id, default);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TPrimaryKey> left, Entity<TPrimaryKey> right) => Equals(left, right);

    public static bool operator !=(Entity<TPrimaryKey> left, Entity<TPrimaryKey> right) => !(left == right);

    public override string ToString() => $"[{GetType().Name} {Id}]";

    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is Entity<TPrimaryKey>))
            return false;

        return this.Equals((Entity<TPrimaryKey>)obj);
    }

    public bool Equals(Entity<TPrimaryKey>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        //Transient objects are not considered as equal
        if (IsNew && other.IsNew)
            return false;

        //Must have a IS-A relation of types or must be same type
        var typeOfThis = GetType();
        var typeOfOther = other.GetType();
        if (!typeOfThis.GetTypeInfo().IsAssignableFrom(typeOfOther) && !typeOfOther.GetTypeInfo().IsAssignableFrom(typeOfThis))
            return false;

        return Id.Equals(other.Id);
    }
}
