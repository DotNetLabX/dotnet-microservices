---
name: domain-patterns
description: DDD patterns used in this codebase — aggregates, entities, value objects, domain events, partial class behavior split, and event dispatch. Loaded when designing or implementing domain models, creating aggregates, or working with domain events.
user-invocable: false
---

# Domain Patterns

## AggregateRoot

**File:** `src/BuildingBlocks/Blocks.Domain/Entities/AggregateRoot.cs`

- Extends `Entity<TPrimaryKey>`, adds audit fields (`CreatedById`, `CreatedOn`, `LastModifiedById`, `LastModifiedOn`)
- Private `_domainEvents` list exposed as `IReadOnlyList<IDomainEvent>`
- Methods: `AddDomainEvent()`, `ClearDomainEvents()`
- Convenience form: `AggregateRoot` (non-generic, PK = int)
- **Exception:** Auth `User` extends `IdentityUser<int>` and implements `IAggregateRoot` manually

## Entity

**File:** `src/BuildingBlocks/Blocks.Domain/Entities/Entity.cs`

- `Entity<TPrimaryKey>` with `Id` property and equality by ID
- `IsNew` returns true when ID is default value

## Value Objects

**File:** `src/BuildingBlocks/Blocks.Domain/ValueObjects/`

Three base types:
- `ValueObject` — multi-property, implements `GetEqualityComponents()`
- `StringValueObject` — single string `Value` property
- `SingleValueObject<T>` — single typed `Value` property

Pattern: static `Create()` factory with validation, internal constructor:
```csharp
public class EmailAddress : StringValueObject
{
    internal EmailAddress(string value) { Value = value; }
    public static EmailAddress Create(string value)
    {
        Guard.ThrowIfNullOrWhiteSpace(value);
        return new EmailAddress(value);
    }
}
```

Real examples: `Auth.Domain/Persons/ValueObjects/EmailAddress.cs`, `Production.Domain/Assets/ValueObjects/AssetName.cs`

## Partial class behavior split

All services split aggregate state from behavior:
- `{Aggregate}.cs` — properties, backing collections (`private readonly List<T> _items = new()`)
- `Behaviors/{Aggregate}.cs` — domain methods that mutate state and raise events

Backing collection pattern:
```csharp
private readonly List<Asset> _assets = new();
public IReadOnlyList<Asset> Assets => _assets.AsReadOnly();
```

## Domain events

**Interface:** `IDomainEvent : INotification, IEvent` (dual MediatR + FastEndpoints)

**Base record:** `DomainEvent<TAction>(TAction Action)` where `TAction : IArticleAction`

Service-specific alias:
```csharp
public record DomainEvent(IArticleAction Action) : DomainEvent<IArticleAction>(Action);
```

Events are simple records:
```csharp
public record ArticleApproved(Article Article, IArticleAction Action) : DomainEvent(Action);
```

Journals uses `IDomainEvent` directly: `public record JournalCreated(Journal Journal) : IDomainEvent;`

## Event dispatch

Two interceptor variants in `Blocks.EntityFrameworkCore/Interceptors/`:

| Interceptor | Used by | Behavior |
|-------------|---------|----------|
| `DispatchDomainEventsInterceptor` | Submission, Review | Dispatches after SaveChanges completes |
| `TransactionalDispatchDomainEventsInterceptor` | Production | Wraps save + dispatch in a single transaction |

Both scan `ChangeTracker` for aggregates with pending events, clear them, then publish via `IDomainEventPublisher`.

## IDomainEventPublisher implementations

| Implementation | Used by | Mechanism |
|---------------|---------|-----------|
| `Blocks.MediatR/DomainEventPublisher` | Submission, Review | `IMediator.Publish()` |
| `Blocks.FastEndpoints/DomainEventPublisher` | Auth, Journals, Production | `IEvent.PublishAsync(Mode.WaitForAll)` |
