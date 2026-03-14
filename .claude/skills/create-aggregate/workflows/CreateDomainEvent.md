# Create Domain Event

## Pattern

**Reference:** `src/Services/Submission/Submission.Domain/Events/`

### Event record

```csharp
public record {EventName}({AggregateName} {Aggregate}, IArticleAction Action) : DomainEvent(Action);
```

For non-article events (e.g., Journals):
```csharp
public record {EventName}({EntityName} {Entity}) : IDomainEvent;
```

### Raising events in aggregate

In the behavior partial class:
```csharp
public void {DomainMethod}({Parameters})
{
    // State changes
    AddDomainEvent(new {EventName}(this, action));
}
```

### Event handler

Handler naming convention: `{Action}On{Event}Handler.cs`

**MediatR handler** (Submission, Review):
```csharp
public class {Action}On{Event}Handler({Dependencies})
    : INotificationHandler<{EventName}>
{
    public async Task Handle({EventName} notification, CancellationToken ct)
    {
        // Handle the event
    }
}
```

**FastEndpoints handler** (Auth, Journals, Production):
```csharp
public class {Action}On{Event}Handler({Dependencies})
    : IEventHandler<{EventName}>
{
    public async Task HandleAsync({EventName} notification, CancellationToken ct)
    {
        // Handle the event
    }
}
```

### Common handler patterns

**Publish integration event:**
```csharp
public class Publish{IntegrationEvent}On{DomainEvent}Handler(IPublishEndpoint _publishEndpoint, {Repository} _repository)
    : INotificationHandler<{DomainEvent}>
{
    public async Task Handle({DomainEvent} notification, CancellationToken ct)
    {
        var entity = await _repository.GetFullByIdAsync(notification.{Aggregate}.Id);
        await _publishEndpoint.Publish(new {IntegrationEvent}(entity.Adapt<{Dto}>()), ct);
    }
}
```

**Add timeline entry:** Handled automatically by `ArticleTimeline` module's generic handlers.

## Location

- Events: `{ServiceName}.Domain/{AggregateName}/Events/{EventName}.cs`
- Handlers (MediatR): `{ServiceName}.Application/Features/{Domain}/{Feature}/{HandlerName}.cs`
- Handlers (FastEndpoints): `{ServiceName}.API/Features/{Domain}/{Feature}/{HandlerName}.cs`
