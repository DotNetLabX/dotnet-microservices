# Create Publisher

The publisher is a domain event handler that maps the domain event to an integration event and publishes it via MassTransit.

## Pattern

**Reference:** `src/Services/Review/Review.Application/Features/Articles/AcceptArticle/PublishIntegrationEventOnArticleAcceptedHandler.cs`

### MediatR services (Submission, Review)

```csharp
public class Publish{IntegrationEvent}On{DomainEvent}Handler(
    IPublishEndpoint _publishEndpoint,
    {Repository} _repository)
    : INotificationHandler<{DomainEvent}>
{
    public async Task Handle({DomainEvent} notification, CancellationToken ct)
    {
        // Re-load with full includes for the DTO
        var entity = await _repository.GetFullByIdAsync(notification.{Aggregate}.Id);

        // Map to DTO and publish
        await _publishEndpoint.Publish(
            new {IntegrationEvent}Event(entity.Adapt<{DtoType}>()),
            ct);
    }
}
```

### FastEndpoints services (Auth, Journals, Production)

```csharp
public class Publish{IntegrationEvent}On{DomainEvent}Handler(
    IPublishEndpoint _publishEndpoint)
    : IEventHandler<{DomainEvent}>
{
    public async Task HandleAsync({DomainEvent} notification, CancellationToken ct)
    {
        await _publishEndpoint.Publish(
            new {IntegrationEvent}Event(notification.{Entity}.Adapt<{DtoType}>()),
            ct);
    }
}
```

## Naming convention

`Publish{IntegrationEventName}On{DomainEventName}Handler.cs`

Example: `PublishIntegrationEventOnArticleAcceptedHandler.cs`

## Location

- MediatR services: alongside the domain event in `{Service}.Application/Features/{Domain}/{Feature}/`
- FastEndpoints services: in `{Service}.API/Features/{Domain}/{Feature}/`
