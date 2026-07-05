---
name: domain-event-wiring
description: Write, create, or add a domain-event handler in this .NET repo, and wire the in-process domain-event dispatch a service needs to run it. THIS is the skill for authoring a domain-event handler here - the {Effect}On{Event}Handler that reacts to an aggregate event, MediatR (INotificationHandler) or FastEndpoints (IEventHandler). Use when creating or writing such a handler, adding a domain event to a service, or choosing a new service's publisher/interceptor. NOT for propagating an event across services (that is the add-integration-event skill) or a synchronous cross-service read (that is grpc-communication).
user-invocable: true
---

# Domain Event Wiring

Domain events are the aggregate-owned, in-process side channel. One event class raised on an aggregate, dispatched to one or more handlers inside the same service. Three decisions get you wired: which publisher, what triggers dispatch, and (for the interceptor path) which interceptor flavor. Then you write the handler.

Cross-service propagation is a different job. Domain events never leave the process; only their DTO projection does, through a `PublishIntegrationEventOn...` handler (boundaries-1, boundaries-2). That handler is shaped like a domain-event handler but belongs to the integration-event skill, not this one.

## The model does not change with the library (eventpub-2)

`IDomainEvent` is dual-typed so the domain never has to know which dispatch library runs:

```csharp
// Blocks.Domain/IDomainEvent.cs
public interface IDomainEvent : INotification, IEvent;   // INotification = MediatR, IEvent = FastEndpoints
```

An event is a record. Three real shapes exist in the repo:

```csharp
// Implements IDomainEvent directly — raised on an aggregate OR published manually.
public record JournalCreated(Journal Journal) : IDomainEvent;

// Derives a service-local DomainEvent base that carries the IArticleAction.
public record ArticleAcceptedForProduction(Article Article, IArticleAction action) : DomainEvent(action);

// A plain record — no IDomainEvent — for the FastEndpoints native manual path (see Decision 2).
public record UserCreated(User User, string RessetPasswordToken);
```

The aggregate owns its events and exposes them read-only; observers read, never inject (domain-1):

```csharp
// AggregateRoot<TPrimaryKey>
private List<IDomainEvent> _domainEvents = new();
public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
public void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
public void ClearDomainEvents() => _domainEvents.Clear();
```

Rule: because the event carries both marker interfaces, picking MediatR or FastEndpoints is a DI-time decision, never a modeling one (eventpub-2). If the event is raised on an aggregate and harvested by the interceptor, it MUST implement `IDomainEvent`; a manually published FastEndpoints event may be a plain record.

## Resolved for every existing service — copy the row that matches yours

| Service | Publisher (Decision 1) | Trigger (Decision 2) | Interceptor flavor (Decision 3) |
|---|---|---|---|
| Submission | MediatR | EF interceptor | post-save |
| Review | MediatR | EF interceptor | post-save |
| Production | MediatR (as event bus only) | EF interceptor | transactional |
| Auth | FastEndpoints | manual endpoint publish | none |
| Journals | FastEndpoints | manual endpoint publish | none |
| ArticleHub | none (read model) | none | none |

ArticleHub is a pure read model and registers no publisher at all (eventpub-1).

## Decision 1 - publisher implementation (eventpub-1, eventpub-4)

One producer seam, `IDomainEventPublisher.PublishAsync`, with two interchangeable implementations. Pick by what the service runs as its **event bus**, which is independent of its request framework.

```csharp
// Blocks.MediatR.DomainEventPublisher — Submission, Review, Production
public sealed class DomainEventPublisher(IMediator mediator) : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent @event, CancellationToken ct = default)
        => mediator.Publish(@event, ct);
}

// Blocks.FastEndpoints.DomainEventPublisher — Auth, Journals
public sealed class DomainEventPublisher : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent @event, CancellationToken ct = default)
        => @event.PublishAsync(Mode.WaitForAll, ct);
}
```

Registration (in the layer that owns composition — Application DI for Submission/Review, API DI for Production/Auth/Journals):

```csharp
// MediatR service — DomainEventPublisher resolves to Blocks.MediatR via a using
services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

// Production disambiguates because it references BOTH Blocks.MediatR and Blocks.FastEndpoints
services.AddScoped<IDomainEventPublisher, Blocks.MediatR.DomainEventPublisher>();

// FastEndpoints service — DomainEventPublisher resolves to Blocks.FastEndpoints via a using
services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
```

The request framework and the event-dispatch library are independent decisions (eventpub-4). Production declares "FastEndpoints (no MediatR)" yet registers MediatR purely as the event bus — one `INotificationHandler`, zero `ISender`/`IRequestHandler`. If you take the MediatR publisher in a FastEndpoints service you must also add MediatR itself:

```csharp
// Required in a FastEndpoints service that uses the MediatR publisher (Production.API)
services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
services.AddScoped<IDomainEventPublisher, Blocks.MediatR.DomainEventPublisher>();
```

## Decision 2 - dispatch trigger (persistence-3; corrected eventpub-3)

Two legitimate triggers. Pick by whether the service writes through EF.

**EF interceptor (automatic harvest)** - the write-side EF services (Submission, Review, Production). The aggregate raises the event with `AddDomainEvent`, the repository calls `SaveChangesAsync`, and a `SaveChangesInterceptor` harvests the events off tracked aggregates and pushes each through `IDomainEventPublisher`. No endpoint code touches publishing.

```csharp
// Blocks.EntityFrameworkCore — the harvest the interceptor calls after save
var aggregates = ctx.ChangeTracker.Entries()
    .Select(a => a.Entity).OfType<IAggregateRoot>()
    .Where(a => a.DomainEvents.Any()).ToList();
var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();
aggregates.ForEach(a => a.ClearDomainEvents());
foreach (var domainEvent in domainEvents)
    await eventPublisher.PublishAsync(domainEvent, ct);
```

**Manual endpoint publish (explicit)** - Redis-backed Journals and Auth's Identity flow. There is no aggregate side channel to harvest (Journals' `Journal` extends the Redis `Entity`, not `AggregateRoot`; Auth's `User` is an `IdentityUser` created through `UserManager` inside a transaction), so the endpoint constructs the event and calls the FastEndpoints-inherited `PublishAsync` right after the write:

```csharp
// Journals CreateJournalEndpoint — event implements IDomainEvent but is published by hand
await _journalRepository.AddAsync(journal);
await PublishAsync(new JournalCreated(journal));

// Auth CreateUserEndpoint — event is a plain record on the FastEndpoints native path
await PublishAsync(new UserCreated(user, resetPasswordToken));
await transaction.CommitAsync(ct);
```

This manual path is correct, not a smell. The interceptor path covers only the EF write-side services; the killed absolute was "domain events are never dispatched manually" (eventpub-3, refuted). The true rule: EF write-side services dispatch via the interceptor; Redis-backed and Identity flows publish manually.

## Decision 3 - interceptor flavor (persistence-3)

Only relevant when Decision 2 is the interceptor path. Register one line in `{Svc}.Persistence/DependencyInjection.cs` and wire it onto the DbContext.

**Post-save (default, cheap)** - `DispatchDomainEventsInterceptor` (Submission, Review). Dispatches in `SavedChangesAsync`, after the save completes.

```csharp
services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
services.AddDbContext<SubmissionDbContext>((provider, options) =>
{
    options.AddInterceptors(provider.GetServices<ISaveChangesInterceptor>());
    options.UseSqlServer(provider.GetRequiredService<DbConnection>());
});
```

**Transactional** - `TransactionalDispatchDomainEventsInterceptor` (Production). Opens a transaction before the save, dispatches after the save, then commits, so event handlers can write in the same transaction as the trigger. It has two extra dependencies you must register: `TransactionProvider` and a bound `TransactionOptions`.

```csharp
services.AddScoped<ISaveChangesInterceptor, TransactionalDispatchDomainEventsInterceptor>();
services.AddScoped<TransactionProvider>();
// plus, in the API options composition:
services.AddAndValidateOptions<TransactionOptions>(configuration);
```

Rule: default to the cheap post-save interceptor; pay for the transactional flavor only where a handler must write atomically with the trigger. Production is the sole precedent (persistence-3).

## Writing the handler (eventpub-5)

Name it by its effect: `{Effect}On{Event}Handler`. Colocate it in the feature folder it serves. It is discovered by assembly scan (MediatR's `RegisterServicesFromAssembly`, or FastEndpoints auto-registration) - no manual DI line. Match the interface to the publisher the service runs.

**MediatR variant** - `INotificationHandler<TEvent>`, method `Handle`:

```csharp
// Production.API/Features/Articles/InitializeFromReview/NotifyProductionOfficeOnArticleAcceptedHandler.cs
public class NotifyProductionOfficeOnArticleAcceptedHandler(
    ProductionDbContext dbContext, IEmailService emailService, IOptions<EmailOptions> emailOptions)
    : INotificationHandler<ArticleAcceptedForProduction>
{
    public async Task Handle(ArticleAcceptedForProduction notification, CancellationToken ct)
    {
        foreach (var officer in dbContext.ProductionOfficers.ToList())
            await emailService.SendEmailAsync(BuildNotificationEmail(notification.Article.Title, officer, emailOptions.Value.EmailFromAddress));
    }
}
```

**FastEndpoints variant** - `IEventHandler<TEvent>`, method `HandleAsync`:

```csharp
// Auth.API/Features/Users/CreateAccount/SendConfirmationEmailOnUserCreatedHandler.cs
public class SendConfirmationEmailOnUserCreatedHandler(
    IEmailService emailService, IHttpContextAccessor httpContextAccessor, IOptions<EmailOptions> emailOptions)
    : IEventHandler<UserCreated>
{
    public async Task HandleAsync(UserCreated notification, CancellationToken ct)
    {
        var emailMessage = BuildConfirmationEmail(notification.User, /* reset link */, emailOptions.Value.EmailFromAddress);
        await emailService.SendEmailAsync(emailMessage);
    }
}
```

Sanctioned exception: ArticleTimeline reacts generically to many aggregates' stage-change events through an abstract generic base (`AddTimelineEventHandler`) in a dedicated `EventHandlers/` folder, because it is not owned by a single feature (eventpub-5). Do not follow the feature-folder rule for that cross-cutting case.

## Steps

1. **Define the event record.** Implement `IDomainEvent` if an aggregate will raise it (interceptor path) or you want it usable by either library; a plain record is fine only for a manually published FastEndpoints event. Colocate under the aggregate's `Events/` folder.
2. **Emit it.** Interceptor path: call `AddDomainEvent(...)` inside the aggregate behavior method. Manual path: construct it and call `PublishAsync(...)` in the endpoint after the write.
3. **Confirm the publisher is registered** (Decision 1). New service: register the implementation matching its event bus; add `AddMediatR(...)` too if it is a FastEndpoints service using the MediatR publisher.
4. **Confirm the trigger** (Decision 2/3). EF write-side: register the interceptor and wire `AddInterceptors` onto the DbContext, adding `TransactionProvider` + `TransactionOptions` for the transactional flavor. Manual: the `PublishAsync` call from step 2 is the trigger.
5. **Write the handler** in the feature folder, named `{Effect}On{Event}Handler`, using the interface for the service's publisher (`INotificationHandler` / `IEventHandler`). No DI line.
6. **Verify.** `rg "AddScoped<IDomainEventPublisher" src/Services/{Svc}` shows the publisher; for the EF path `rg "AddScoped<ISaveChangesInterceptor" src/Services/{Svc}` shows the interceptor; build the service.

## What this skill does NOT do

- **Cross-service propagation.** A `PublishIntegrationEventOn...` handler that maps to a DTO and publishes via MassTransit is the integration-event skill's job (boundaries-2), even though it is shaped like a domain-event handler.
- **Creating the aggregate** or its behavior methods - that is the aggregate skill; this skill assumes the aggregate exists and only wires the event out of it.
- **Editing the Blocks.* building blocks** (`IDomainEvent`, the publishers, the interceptors, the change-tracker harvest). They are stable infrastructure; this skill consumes them.

## Lessons and changes

If a wiring case contradicts or extends this skill - a new publisher/interceptor pairing, a framework gotcha, an event shape not covered here - capture it in the running feature's `docs/specs/{slug}/delivery/lessons.md` under the Developer heading so the learner can fold it back; outside a pipeline run, log it in `docs/skill-backlog.md`. Changes to this skill are recorded in its `CHANGELOG.md`.
