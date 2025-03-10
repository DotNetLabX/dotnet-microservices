namespace Blocks.Entitities;

/// <summary>
/// use those entities if you want to store multiple tenants in the same database
/// tenant entities will have a composed primary key {TenantId, EntitiyId}
/// </summary>
public interface IMultitenancy
{
		public int TenantId { get; set; }
}

public abstract class TenantEntity : Entity, IMultitenancy
{
		public int TenantId { get; set; }
}

public abstract class AggregateTenantEntity : AggregateRoot<int>, IMultitenancy
{
		public int TenantId { get; set; }
}
