---
name: add-integration-event
description: Propagate a business event across .NET microservice boundaries via MassTransit. Adds the contract record plus flat DTOs in Articles.IntegrationEvents.Contracts, a PublishIntegrationEventOn{Event} handler that re-fetches the aggregate and publishes, and auto-discovered consumers in the receiving services. Variant-aware for MediatR (INotificationHandler) and FastEndpoints (IEventHandler) publishers. Use when a domain event must cross a service boundary; not for an in-process reaction (that is a domain event handler) or a synchronous cross-service read (that is gRPC).
user-invocable: true
---

# Add Integration Event

Move a business fact from the service that owns it to the services that need to react — asynchronously, over RabbitMQ, through MassTransit. A **domain event** stays in-process and carries the full aggregate; an **integration event** leaves the process and carries only a flat DTO projection (registry row `boundaries-1`). This skill adds the second kind on top of a domain event that already exists.

## When to use

- A domain event is already raised inside an aggregate (the aggregate calls `AddDomainEvent(...)`), and **another service** must react to it.
- The reaction is a state change the receiver owns (create a shadow row, initialize its own aggregate, bump a counter, migrate asset bytes).

Do **not** use this skill when the reaction stays inside the same service (write a domain event handler instead — the `{Effect}On{Event}Handler` in the feature folder) or when the caller needs an authoritative answer *right now* (use a gRPC client instead — integration events are fire-and-forget).

## The flow

```text
Aggregate (owning service)
  └─ raises DomainEvent  ─────────────────────────────►  in-process only, carries the full entity
        │
        ▼
  PublishIntegrationEventOn{DomainEvent}Handler   ◄── the ONLY sanctioned domain->integration handoff (boundaries-2)
        │   re-fetch aggregate by id  ->  Adapt to DTO (Mapster)  ->  IPublishEndpoint.Publish
        ▼
  RabbitMQ  ── pub/sub fan-out ──►  Consumer in Service A
                               ├──►  Consumer in Service B     (one event, N independent consumers — boundaries-5)
                               └──►  Consumer in Service C
```

Three edits, in order: the contract (Phase 1), the publisher handler in the owning service (Phase 2), one consumer per receiving service (Phase 3). MassTransit DI is already centralized (`Blocks.Messaging` `AddMassTransitWithRabbitMQ`) and every service calls it identically — you never touch messaging wiring.

---

## Phase 1 — Contract record + DTOs

Location: `src/BuildingBlocks/Articles.Integration.Contracts/`, in the sub-folder of the owning domain (`Articles/`, `Journals/`, `Persons/`).

**First, check the event does not already exist.** Grep the contracts folder for the exact name and for near-names before adding a record — reusing or renaming an existing event beats shipping a near-duplicate: `rg -n "record \w*Event" src/BuildingBlocks/Articles.Integration.Contracts`. Scan the hits for anything that already carries the fact (a differently-worded event on the same aggregate, a stage-change event you can extend); only add a new record when nothing covers it.

**Gotcha — the namespace is not the folder name.** The folder is `Articles.Integration.Contracts` but the project file is `Articles.IntegrationEvents.Contracts.csproj`, so the assembly and root namespace are `Articles.IntegrationEvents.Contracts`. Match the code, not the folder:

```csharp
// Articles/ArticleApprovedForReviewEvent.cs
using Articles.IntegrationEvents.Contracts.Articles.Dtos;

namespace Articles.IntegrationEvents.Contracts.Articles;

public record ArticleApprovedForReviewEvent(ArticleDto Article);
```

The house shape is a single wrapper record around one DTO. The DTO is a **flat record of primitives, enums, and nested DTOs — never a domain entity** (`boundaries-1`). Enums come from `Articles.Abstractions.Enums`, which is the only project the contracts project references:

```csharp
// Articles/Dtos/ArticleDto.cs
using Articles.Abstractions.Enums;

namespace Articles.IntegrationEvents.Contracts.Articles.Dtos;

public record ArticleDto(
    int Id,
    string Title,
    ArticleType Type,
    ArticleStage Stage,
    JournalDto Journal,          // nested DTO, not the Journal entity
    PersonDto SubmittedBy,       // nested DTO, not the Person entity
    List<ActorDto> Actors,
    List<AssetDto> Assets);
```

Rules for this phase:

- Reference no persistence, no MassTransit, no domain project — the contract stays a POCO carrying only `Articles.Abstractions` enums.
- Every nested type is its own `record` DTO in `Dtos/`. If the event needs `Journal`, add a `JournalDto`; do not reach for the domain `Journal`.
- Reuse an existing DTO rather than cloning it (`ArticleDto`, `JournalDto`, `PersonDto`, `AssetDto`, `ActorDto` already exist).

## Phase 2 — Publisher handler (the domain->integration handoff)

This handler is the **only** place a domain event is allowed to trigger cross-service traffic (`boundaries-2`). Name it `PublishIntegrationEventOn{DomainEvent}Handler` after the **domain event it handles**, not the integration event it publishes (handling `ArticleApproved`, publishing `ArticleApprovedForReviewEvent`, the class is `PublishIntegrationEventOnArticleApprovedHandler`). Colocate it in the feature folder that raises the domain event, named by effect (`eventpub-5`).

The body is always the same three moves: **re-fetch the full aggregate by id, `Adapt` it to the DTO (Mapster), `Publish` via `IPublishEndpoint`.** Re-fetch rather than trusting the entity on the notification — the domain event may carry a partially-loaded graph, and the DTO must be complete.

Only two tokens differ between the two supported services shapes:

| | MediatR service (Submission, Review) | FastEndpoints service (Journals, Auth) |
|---|---|---|
| Handler interface | `INotificationHandler<TDomainEvent>` | `IEventHandler<TDomainEvent>` |
| Method name | `Handle(notification, ct)` | `HandleAsync(notification, ct)` |
| Lives in | the `.Application` project | the `.API` project |
| Auto-registered by | `AddMediatR` assembly scan | FastEndpoints event-handler discovery |

Both interfaces and `Adapt` arrive through the project's `GlobalUsings`, so the file only needs `using MassTransit;`, the contract usings, and the domain-events using.

**MediatR variant** (`Submission.Application/Features/ApproveArticle/`):

```csharp
using MassTransit;
using Articles.IntegrationEvents.Contracts.Articles;
using Articles.IntegrationEvents.Contracts.Articles.Dtos;
using Submission.Domain.Events;

namespace Submission.Application.Features.ApproveArticle;

public class PublishIntegrationEventOnArticleApprovedHandler(
    ArticleRepository _articleRepository, IPublishEndpoint _publishEndpoint)
    : INotificationHandler<ArticleApproved>
{
    public async Task Handle(ArticleApproved notification, CancellationToken ct)
    {
        var article = await _articleRepository.GetFullArticleByIdAsync(notification.Article.Id);

        var articleDto = article.Adapt<ArticleDto>();

        await _publishEndpoint.Publish(new ArticleApprovedForReviewEvent(articleDto), ct);
    }
}
```

**FastEndpoints variant** (`Journals.API/Features/Journals/Create/`):

```csharp
using MassTransit;
using Blocks.Redis;
using Articles.IntegrationEvents.Contracts.Journals;
using Journals.Domain.Journals.Events;

namespace Journals.API.Features.Journals.Create;

public class PublishIntegrationEventOnJournalCreatedHandler(
    Repository<Journal> _journalRepository, IPublishEndpoint _publishEndpoint)
    : IEventHandler<JournalCreated>
{
    public async Task HandleAsync(JournalCreated notification, CancellationToken ct)
    {
        var journal = await _journalRepository.GetByIdAsync(notification.Journal.Id);

        var journalDto = journal.Adapt<JournalDto>();

        await _publishEndpoint.Publish(new JournalCreatedEvent(journalDto), ct);
    }
}
```

No manual registration either way — the handler is discovered by the scan the service already runs.

## Phase 3 — Consumer(s) in the receiving services

For each service that must react, add a consumer implementing `IConsumer<TEvent>`. **Place it in the project whose `DependencyInjection.cs` calls `AddMassTransitWithRabbitMQ(configuration, Assembly.GetExecutingAssembly())`** — the `.Application` project for Submission and Review, the `.API` project for Journals, Production, and ArticleHub. That call runs `config.AddConsumers(assembly)`, so a consumer in the scanned assembly is wired automatically — there is **no manual consumer registration** anywhere in the repo (`boundaries-5`). One event fans out to as many consumers as you write; each receiving service adds its own.

Read the event off `context.Message`. Take persistence collaborators as concrete types (`{Name}DbContext`, `Repository<T>`, `{X}Repository`), and pick a duplicate-delivery strategy — the house has three (silent skip-if-exists, throw-if-exists, silent-return-if-exists); use whichever the receiving service already uses.

**Rich consumer** — initialize the receiver's own aggregate from the DTO (`Review.Application/Features/Articles/InitializeFromSubmission/`):

```csharp
using Articles.IntegrationEvents.Contracts.Articles;
using MassTransit;

namespace Review.Application.Features.Articles.InitializeFromSubmission;

public class ArticleApprovedForReviewConsumer(
    ReviewDbContext _dbContext,
    ArticleRepository _articleRepository,
    Repository<Person> _personRepository,
    Repository<Journal> _journalRepository)
    : IConsumer<ArticleApprovedForReviewEvent>
{
    public async Task Consume(ConsumeContext<ArticleApprovedForReviewEvent> context)
    {
        var articleDto = context.Message.Article;

        // idempotency: throw-if-exists is this service's choice
        await _articleRepository.EnsureNotExistsOrThrowAsync(articleDto.Id, context.CancellationToken);

        // materialize local entities from the DTO (GetOrCreate for foreign reference data), then:
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
```

**Simple consumer** — the same event, a different receiver, a tiny reaction (`Journals.API/Features/Journals/Consumers/`). This is the fan-out: `ArticleApprovedForReviewEvent` is consumed independently by Review, Journals, and ArticleHub.

```csharp
using Articles.IntegrationEvents.Contracts.Articles;
using Blocks.Redis;
using MassTransit;

namespace Journals.API.Features.Journals.Consumers;

public class ArticleApprovedForReviewConsumer(Repository<Journal> _journalRepository)
    : IConsumer<ArticleApprovedForReviewEvent>
{
    public async Task Consume(ConsumeContext<ArticleApprovedForReviewEvent> context)
    {
        var journal = await _journalRepository.GetByIdOrThrowAsync(context.Message.Article.Journal.Id);

        journal.ArticlesCount += 1;

        await _journalRepository.UpdateAsync(journal);
    }
}
```

---

## Binding rules

1. **Flat DTOs, never entities** — the event and every nested type is a `record` of primitives, enums, and DTOs; a domain entity must not appear in `Articles.IntegrationEvents.Contracts` (`boundaries-1`).
2. **One handoff seam** — the domain-to-integration bridge is a `PublishIntegrationEventOn{DomainEvent}Handler` that re-fetches, `Adapt`s, and `Publish`es; a `_publishEndpoint.Publish` call belongs nowhere else (`boundaries-2`).
3. **Name by the domain event, colocate by feature** — the class is named for the domain event it handles and lives in that feature's folder, following the `{Effect}On{Event}Handler` convention (`eventpub-5`).
4. **No manual consumer registration** — consumers are auto-discovered by the assembly scan in `AddMassTransitWithRabbitMQ`; add the consumer to the scanned project and stop (`boundaries-5`).
5. **Namespace over folder** — write `Articles.IntegrationEvents.Contracts...`, not `Articles.Integration.Contracts...`; the csproj name wins.

## Verify

- Contract carries no entity: `rg -n "using .*Domain" src/BuildingBlocks/Articles.Integration.Contracts` returns nothing.
- Exactly one publish site per event, inside a publisher handler: `rg -n "_publishEndpoint.Publish" src/Services` — every hit sits in a `PublishIntegrationEventOn*` file.
- No stray consumer registration: `rg -n "AddConsumer\b|RegisterConsumer" src/Services` returns nothing (only the shared `AddConsumers(assembly)` in `Blocks.Messaging` should exist).
- The publisher handler namespace matches its project (`.Application` for MediatR services, `.API` for FastEndpoints services), and the consumer sits in the project that calls `AddMassTransitWithRabbitMQ`.
- **Final gate — it all compiles:** `dotnet build` succeeds, so the contract project, the owning service, and every receiving service that now references the event and its consumer build clean.

## What this skill does NOT do

- **In-process reactions** — if the handler stays in the same service, it is a domain event handler (`create-domain-event-handler`), not an integration event.
- **Synchronous cross-service reads** — an authoritative live query is gRPC (`Articles.Grpc.Contracts`), not a published event.
- **Messaging wiring** — `AddMassTransitWithRabbitMQ`, the endpoint-name formatter, and RabbitMQ options live in `Blocks.Messaging` and are already called by every service; adding an event never edits them.
