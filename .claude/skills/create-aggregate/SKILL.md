---
name: create-aggregate
description: Creates a DDD aggregate root with behavior split, value objects, and domain events. Use when adding a new aggregate or entity to a service's domain model.
---

# Create Aggregate

Creates a DDD aggregate root following the partial class behavior split pattern used in this codebase.

## Steps

1. **Create the aggregate state file** — follow `workflows/CreateAggregate.md`
2. **Create value objects** (if needed) — follow `workflows/CreateValueObject.md`
3. **Create domain events** (if needed) — follow `workflows/CreateDomainEvent.md`
4. **Create EF Core entity configuration** in the Persistence project
5. **Create/update the repository** in the Persistence project
6. **Add migration:** `dotnet ef migrations add {Name} -p Services/{Svc}/{Svc}.Persistence -s Services/{Svc}/{Svc}.API`
7. **Verify the build:** `dotnet build`

## Arguments

Pass the aggregate name: `/create-aggregate ArticleRevision`

## Special cases

- **Auth `User`:** Cannot extend `AggregateRoot<T>` due to `IdentityUser` — must implement `IAggregateRoot` manually with inline `_domainEvents` list
- **Journals:** Uses Redis.OM `Entity` base, not `AggregateRoot`. Decorated with `[Document]`, `[Indexed]`, `[Searchable]`
