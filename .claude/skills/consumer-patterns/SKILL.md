---
name: consumer-patterns
description: Write MassTransit integration-event consumers in this repo — the consumer-class shape and primary-constructor dependencies, the three idempotency variants and when each fits, local reference-data hydration, and the split between a read-model projection and a write-side domain mutation. Use when adding or editing an IConsumer for a cross-service integration event (an Article... or Journal... event under Articles.IntegrationEvents.Contracts).
user-invocable: true
---

# Consumer Patterns

A MassTransit consumer is how one service reacts to another service's integration event: it hydrates foreign-owned reference data into its own store, then either projects a read model or drives a domain mutation. Always five phases, in one order. The single real decision is which of three idempotency strategies to use (`consumers-2`).

Encodes reference-model rows `consumers-1` through `consumers-5` and `datasharing-2` (`docs/reference-model.md`).

## When to use

Adding or editing a class that implements `IConsumer<TEvent>` for a cross-service integration event (`Articles.IntegrationEvents.Contracts.*`). Not for domain-event handlers (in-process `INotificationHandler` / FastEndpoints `IEventHandler`) — those stay inside a service boundary and never hydrate foreign data. Copy the closest template from `references/` and fill each phase.

## The five phases

1. **Consumer class + primary-constructor dependencies.** Implement `IConsumer<TEvent>`. Inject persistence collaborators as **concrete** types — the service `DbContext`, `Repository<T>`, a `{X}Repository` — never a repository interface (the repo's no-interface-without-multiple-implementations guardrail). Non-persistence collaborators (`IFileService`, `FileServiceFactory`, `ArticleStateMachineFactory`) **are** injected as interfaces or delegates. (`consumers-1`)
2. **Idempotency check.** Guard against duplicate delivery before any write, and put it first so a redelivery is cheap. Pick one of the three strategies below. (`consumers-2`)
3. **Local reference-data hydration.** Foreign-owned data (Journal, Person) is replicated per-event, not queried on demand: look it up in the local store first and materialize from the event DTO only if missing — a `GetOrCreate{X}` helper. Materialize with Mapster `Adapt<T>()` or by hand; both are sanctioned (hand-construction wins when you also stamp a local-only field the event never carries). (`consumers-3`) Reference data that changes independently of the article flow (a Journal's name or abbreviation) is instead kept fresh by a dedicated `JournalCreated` / `JournalUpdated` shadow-row consumer pair, one per consuming write service — see `references/reference-data-consumer.md`. (`datasharing-2`)
4. **Domain mutation or projection write.** Write-side service: build the aggregate through its factory (`Article.FromSubmission`, `Article.FromReview`) so invariants run, then `AddAsync`. Read-model service (ArticleHub): the projection entity is a plain data bag — set its properties or `Adapt` the DTO directly, never add behavior to it. (`consumers-5`) When the stage change carries files, migrate the bytes here — `DownloadAsync` from the sending stage's store, `UploadAsync` into your own, because each service owns its storage. (`consumers-4`)
5. **Save.** One `SaveChangesAsync(context.CancellationToken)` at the end — the repository is the unit of work, there is no separate commit. Single-entity reference-data consumers call `SaveChangesAsync()` on the repository itself.

## Idempotency: pick one of three

The repo keeps three strategies on purpose — the Review consumer's `//insight - inbox pattern vs simple business rules` comment marks the variance as deliberate (`consumers-2`). Pick by what a duplicate delivery **means**:

| Strategy | Mechanism | Use when | Seen in |
|---|---|---|---|
| Silent skip-if-exists | look up by id, `if (existing is not null) return;` (or `UpsertAsync` for updates) | a redelivery is normal and harmless — reference-data replication | Journal `Created` / `Updated` consumers |
| Throw-if-exists | `EnsureNotExistsOrThrowAsync(id, ct)`, or `AnyAsync(...)` then `throw new BadRequestException(...)` | a duplicate signals a bug — the row should be created exactly once and you want it surfaced | article-creation consumers (Review, ArticleHub first stage) |
| ExistsAsync then return | `if (await repository.ExistsAsync(id)) return;` | same create-once intent, but a redelivery should be swallowed quietly, not thrown | Production |

Decision rule: reference-data upsert → **silent skip**. Article creation where a second delivery is a bug worth seeing → **throw**. Article creation where you would rather absorb the redelivery → **ExistsAsync return**. An update-existing projection (a later stage mutating a row an earlier stage created) inverts the guard: load-or-throw with `SingleOrThrowAsync`, then mutate — see `references/projection-consumer.md` variant B.

## Templates

- `references/write-side-consumer.md` — article-creation consumer (throw-if-exists), all five phases including actor/asset hydration and file migration; based on Review's `ArticleApprovedForReviewConsumer`, with the Production hand-construction and ExistsAsync-return variants inline.
- `references/projection-consumer.md` — ArticleHub read-model: variant A create-new (`AnyAsync` throw) and variant B update-existing (`SingleOrThrowAsync` then field set).
- `references/reference-data-consumer.md` — the `JournalCreated` (skip-if-exists) / `JournalUpdated` (upsert) shadow-row pair.

## What this skill does NOT do

- Domain-event handlers (in-process) — a different pattern (effect-named `INotificationHandler` in a feature folder), covered by the event-handler skills.
- Publishing integration events — that is the `PublishIntegrationEventOn{X}Handler` producer seam (`boundaries-2`), not a consumer.
- File-storage mechanics — the byte migration in phase 4 lives here, but the `IFileService` download/upload details do not; follow the `DownloadAsync`/`UploadAsync` loop in `references/write-side-consumer.md`, or the file-storage-patterns skill if it is present.
