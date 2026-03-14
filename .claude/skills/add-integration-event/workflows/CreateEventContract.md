# Create Event Contract

## Pattern

**Reference:** `src/BuildingBlocks/Articles.Integration.Contracts/`

Create in `src/BuildingBlocks/Articles.Integration.Contracts/`:

```csharp
public record {EventName}Event({DtoType} {DtoName});
```

## DTO payload

If a new DTO is needed, create it in the same project:

```csharp
public record {DtoType}(
    int Id,
    string Title,
    // Include all fields the consumer needs
    // Don't leak EF/domain internals
);
```

## Existing DTOs

- `ArticleDto` — full article snapshot (Id, Title, Scope, Doi, Type, Stage, Journal, Actors, Assets, timestamps)
- `PersonDto` — person info
- `JournalDto` — journal metadata
- `ActorDto`, `AssetDto` — supporting data

Reuse existing DTOs when possible. Only create new ones if the event carries a genuinely different shape.

## Rules

- Events are simple records — no logic, no methods
- Payload is a DTO snapshot, not a domain entity
- Contracts must be stable — changing them affects all consumers
- Don't leak internal IDs or EF navigation properties
