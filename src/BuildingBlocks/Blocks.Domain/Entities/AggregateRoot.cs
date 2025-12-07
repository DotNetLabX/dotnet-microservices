using Blocks.Domain;
using Blocks.Domain.Entities;

namespace Blocks.Entitities;

public interface IAggregateRoot : IAggregateRoot<int>;

public abstract class AggregateRoot : AggregateRoot<int>, IAggregateRoot, IAuditedEntity;


public interface IAggregateRoot<TPrimaryKey> : IAuditedEntity<TPrimaryKey>
    where TPrimaryKey : struct
{
    public IReadOnlyList<IDomainEvent> DomainEvents { get; }
    public void AddDomainEvent(IDomainEvent eventItem);
    public void ClearDomainEvents();
}

public abstract class AggregateRoot<TPrimaryKey> : Entity<TPrimaryKey>, IAggregateRoot<TPrimaryKey>
    where TPrimaryKey : struct
{
    //insight - audited properties are required only in the aggregates because when we are saving the other entities they are going to be part of an aggregate
    // therefore they are going to inherit the same audited values
    public TPrimaryKey CreatedById { get; init; }
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
    public TPrimaryKey? LastModifiedById { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    #region Domain Events
    //talk - if you want your collections to be trully imutable we need to use ImmutableList, the others can be cast back to its original class
    private List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    public void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);    
    public void ClearDomainEvents() => _domainEvents.Clear();
    #endregion
}