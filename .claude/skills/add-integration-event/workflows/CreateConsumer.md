# Create Consumer

MassTransit consumer that handles integration events in the target service.

## Pattern

**Reference:** `src/Services/ArticleHub/ArticleHub.API/Articles/Consumers/ArticleAcceptedForProductionConsumer.cs`

```csharp
public sealed class {EventName}Consumer({DbContext} _dbContext)
    : IConsumer<{EventName}Event>
{
    public async Task Consume(ConsumeContext<{EventName}Event> context)
    {
        var dto = context.Message.{DtoName};

        // Idempotency: check if already processed
        var existing = await _dbContext.{Entities}
            .SingleOrDefaultAsync(e => e.Id == dto.Id);

        if (existing != null)
        {
            // Update existing record
            existing.{Property} = dto.{Property};
        }
        else
        {
            // Create new record
            var entity = dto.Adapt<{Entity}>();
            await _dbContext.{Entities}.AddAsync(entity);
        }

        await _dbContext.SaveChangesAsync();
    }
}
```

## Rules

1. **Consumers must be idempotent** — events may be delivered more than once
2. Check for existing records before inserting
3. Use `sealed class` for consumers
4. Inject the DbContext directly (no repository layer needed for consumers)
5. Use Mapster for DTO → entity mapping

## Registration

Consumers are auto-discovered by MassTransit:
```csharp
config.AddConsumers(assembly);
```

No manual registration needed. Queue naming is handled by `SnakeCaseWithServiceSuffixNameFormatter`.

## Naming convention

`{EventName}Consumer.cs` — matches the event name without the "Event" suffix in the class name.

Example: `ArticleAcceptedForProductionEvent` → `ArticleAcceptedForProductionConsumer`

## Location

Consumers live in the consuming service's API project, typically:
- ArticleHub: `ArticleHub.API/Articles/Consumers/`
- Other services: `{Service}.API/Features/{Domain}/Consumers/` or `{Service}.Application/Features/{Domain}/Consumers/`
