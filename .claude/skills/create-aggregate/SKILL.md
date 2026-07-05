---
name: create-aggregate
description: Create a DDD aggregate with this repo's exact shape — state/behavior partial split, value objects, domain events, and either an EF Core configuration or Redis.OM attributes. Use when adding a new aggregate (or a rich entity that owns invariants) to a service's Domain layer, or when asked to model a new domain concept the write side must protect. Two variants — EF-backed AggregateRoot and Redis Entity — with a decision rule below.
user-invocable: true
---

# Create Aggregate

An aggregate is the write-side consistency boundary: it owns its child entities, enforces its own
invariants, and is the only thing that raises its domain events. This repo has **two aggregate
shapes** — pick one with the decision rule, then walk the phases. Every rule below cites a row in
`docs/reference-model.md` (the skeptic-verified registry); read that row if you need the evidence.

The canonical live examples are `src/Services/Submission/Submission.Domain` (EF variant) and
`src/Services/Journals/Journals.Domain` (Redis variant). Read them alongside the templates.

## Scope — this skill vs its neighbors

This skill builds the aggregate itself: state, behavior, value objects, domain events, and storage
mapping. It stops at the Domain + Persistence boundary. For the work around it, use the sibling skills:

- **Expose an operation over HTTP** (endpoint + command/query + validator) → `create-feature-slice`.
- **React to a domain event in-process** (an `{Effect}On{Event}` handler, publisher/interceptor wiring) → `domain-event-wiring`.
- **Propagate an event across service boundaries** (MassTransit contract + consumers) → `add-integration-event`.
- **Deeper DDD design questions** (one aggregate or two? where is the boundary?) → the `domain-patterns` plugin skill.

## Variant decision

Ask: **does this aggregate enforce write-side invariants that must hold atomically?**

- **Yes** → **EF-backed variant** (Submission / Review / Production shape). SQL Server / PostgreSQL,
  `AggregateRoot<int>` base, partial-class split, domain events drained by a `SaveChangesInterceptor`.
  This is the default for anything with behavior.
- **No — it is CRUD-shaped reference/config data** → **Redis variant** (Journals shape). Redis.OM
  document, `Blocks.Redis.Entity` base, no EF machinery at all, events raised at the endpoint. *(persistence-4)*

If unsure, choose EF-backed — it is the reversible choice; downgrading a rich aggregate to Redis later is cheap, upgrading a Redis bag to enforce invariants is not.

---

## EF-backed variant — phases

Run the phases in order. Files land under `src/Services/{Svc}/{Svc}.Domain` and `.../{Svc}.Persistence`.

**Folder layout — two forms, both live; do not pin one.** The namespace follows the folder, so match
whichever the target service already uses (read its Domain tree first):
- **Flat** (Submission, Auth): `{Svc}.Domain/Entities/{Aggregate}.cs` + `{Svc}.Domain/Behaviors/{Aggregate}.cs`,
  namespace `{Svc}.Domain.Entities`.
- **Aggregate-grouped** (Review, Production): `{Svc}.Domain/{Aggregate}s/{Aggregate}.cs` +
  `{Svc}.Domain/{Aggregate}s/Behaviors/{Aggregate}.cs` (e.g. `Production.Domain/Articles/Behaviors/Article.cs`),
  namespace `{Svc}.Domain.{Aggregate}s`.

The phase paths below use the flat form; translate them to the grouped form when that is the service's convention.

### Phase 1 — State file → `{Svc}.Domain/Entities/{Aggregate}.cs`
`public partial class {Aggregate} : AggregateRoot`. Declare **what the aggregate has**: scalar props
(`init` unless a behavior mutates them), owned references, and child collections as a private
`List<T>` exposed `IReadOnlyList<T>` via `.AsReadOnly()`. Add an `internal {Aggregate}() { }` ctor for
EF. No behavior methods here. Template: `templates/ef-state.cs`.

### Phase 2 — Behavior partial → `{Svc}.Domain/Behaviors/{Aggregate}.cs`
A **second `partial` file, same class name**, holding **what the aggregate does** *(domain-2)*. The
state and behavior files need not share a folder, but must share the class name. Each behavior method:
- mutates the private child collections (never expose a mutable collection),
- throws `DomainException` on an invariant violation — a plain domain exception, **no HTTP type** *(errors-2)*,
- calls `AddDomainEvent(...)` for every meaningful state change — the aggregate is the **only** caller *(domain-1)*,
- routes any stage change through `SetStage(...)`, which calls `stateMachineFactory.ValidateStageTransition(...)`
  **before** mutating — the state machine is the sanctioned second enforcement site *(domain-2, domain-3)*.

**Owner rule — the action parameter is ALWAYS LAST (authoritative):** order every behavior method's
parameters **entity/domain args → domain services/delegates (e.g. `ArticleStateMachineFactory`) →
`action` last**. This is what Review and Production do —
`AssignTypesetter(Typesetter typesetter, ArticleStateMachineFactory stateMachineFactory, IArticleAction action)`,
`Accept(ArticleStateMachineFactory stateMachineFactory, IArticleAction action)`. Submission's four `Article`
behaviors put `action` before the factory (e.g. `Approve(Person editor, IArticleAction action,
ArticleStateMachineFactory factory)`) — that predates the rule and is **known drift, not a pattern to copy**.
Template: `templates/ef-behavior.cs`.

**Action type — generic vs non-generic:** the `action` parameter type differs by service — Submission
takes the generic `IArticleAction<ArticleActionType>`, Review and Production take the non-generic
`IArticleAction`. Match the service you are in; don't assume the generic form.

### Phase 3 — Value objects → `{Svc}.Domain/ValueObjects/{ValueObject}.cs`
Wrap any primitive that has an invariant (email, number, extension) so an invalid value is
**structurally impossible to construct** *(domain-5)*: derive from `StringValueObject` (or
`SingleValueObject<T>` for a struct), make the ctor **private**, expose a **static `Create(...)`** that
runs `Guard.ThrowIf*` **first**, then returns `new`. Template: `templates/value-object.cs`.

### Phase 4 — Domain events → `{Svc}.Domain/Events/{Event}.cs`
Declare a per-service `record DomainEvent(IArticleAction Action) : DomainEvent<IArticleAction>(Action);`
base, then concrete events carrying the **full aggregate** + the action. Events stay in-process and
carry domain entities *(boundaries-1)*; they are raised only from inside the aggregate *(domain-1)*.
Reuse the shared `ArticleStageChanged` (Articles.Abstractions.Events) for stage changes rather than
re-declaring one. Template: `templates/domain-event.cs`.

### Phase 5 — EF configuration → `{Svc}.Persistence/EntityConfigurations/{Aggregate}EntityConfiguration.cs`
Aggregate → `AuditedEntityConfiguration<{Aggregate}>`; call `base.Configure(builder)` **first** (it does
key, id, audit columns, and JSON seeding), then map properties/relationships. Column lengths come from
`MaxLength.*` constants, not literals *(conventions-4)*. Audit is mandatory; the `RowVersion` concurrency
token is opt-in and off by default *(persistence-2)*. For any **behavior-driving enum metadata** model it
as `EnumEntity<{Enum}>` *(domain-4)* and configure it with `EnumEntityConfiguration<{EnumEntity}, {Enum}>`.
Templates: `templates/enum-entity.cs`, `templates/ef-configuration.cs`.

Then add/apply the migration with the commands in the root `CLAUDE.md`.

### Repositories
The generic `Repository<{Aggregate}>` (repository-as-unit-of-work — `SaveChangesAsync` lives on it,
*persistence-1*) covers plain CRUD. When the aggregate needs eager-loaded reads, **subclass** it —
`class {Aggregate}Repository(...DbContext dbContext) : Repository<{Aggregate}>(dbContext)` overriding
`Query()` to `.Include(...)` its children (see `Submission.Persistence/Repositories/ArticleRepository.cs`).
Registration is assembly-scan, not per-type: Submission and Review call
`services.AddDerivedTypesOf(typeof(Repository<>))` (Blocks.Core), which auto-registers every `Repository<T>`
subclass — a new custom repository needs **no** DI line. Production, Journals, and ArticleHub register only
`AddScoped(typeof(Repository<>))`; there, register a custom or cached repository (e.g. `AssetTypeRepository`)
by hand.

---

## Redis variant — phases

For the CRUD-shaped case. Files land under `src/Services/{Svc}/{Svc}.Domain`. Template: `templates/redis-entity.cs`.

1. **Document** → `{Svc}.Domain/{Aggregate}s/{Aggregate}.cs`: `public partial class {Aggregate} : Blocks.Redis.Entity`,
   decorated `[Document(StorageType = StorageType.Json, Prefixes = new[] { nameof({Aggregate}) })]`. Mark queryable
   fields `[Indexed]` / `[Searchable]`; maintain a normalized copy of any searched text on set.
2. **Construction** → a `static Create(...)` factory partial when materializing from another source (e.g. gRPC).
   Value objects (Phase 3 above) still apply if a field has an invariant.
3. **Persistence** → inject `Blocks.Redis.Repository<{Aggregate}>`; `AddAsync` assigns the Id from a Redis INCR
   counter; use `ReplaceAsync` when updating nested collections.
4. **Events** → raise from the endpoint via FastEndpoints `PublishAsync(new {Aggregate}Created({aggregate}))` —
   there is no interceptor to drain an aggregate queue here. The event record is still a plain `IDomainEvent`
   carrying the full entity.

There is **no EF configuration, DbContext, interceptor, or migration** for a Redis aggregate — the
attributes are the schema *(persistence-4)*.

---

## Special case — an aggregate that can't extend `AggregateRoot<T>`

When the type already inherits a framework base class it can't also extend `AggregateRoot<int>` (C#
single inheritance). The repo's one case is Auth's `User : IdentityUser<int>` — it implements
`IAggregateRoot` **manually**: declare the audit quartet (`CreatedById`, `CreatedOn`, `LastModifiedById`,
`LastModifiedOn`) **and** the domain-event side channel (`_domainEvents`, read-only `DomainEvents`,
`AddDomainEvent`, `ClearDomainEvents`) by hand — the exact members `AggregateRoot<T>` would have supplied.
Copy them verbatim from `src/Services/Auth/Auth.Domain/Users/User.cs`. Everything else (partial split,
value objects, events, EF config) is unchanged. This is the **only** sanctioned reason to hand-roll the
side channel (domain-1).

## Promote an existing entity to an aggregate root (in place)

Sometimes a type already exists as a plain entity and now needs to own invariants and raise events. This
is a **type change, not a scaffold** — edit the existing class, don't create a new file:
1. Change the base from `Entity<int>` / `Entity` to `AggregateRoot` on the existing class.
2. Split behavior into a `Behaviors/{Aggregate}.cs` partial if it isn't split already.
3. Swap its EF config base from `EntityConfiguration<{Aggregate}>` to `AuditedEntityConfiguration<{Aggregate}>`.
4. **Flag the schema consequence at plan time:** `AuditedEntityConfiguration` adds the audit columns
   (`CreatedOn`, `CreatedById`, `LastModifiedOn`, `LastModifiedById`) — promoting an existing table is a
   **migration that adds NOT-NULL columns to rows that already exist**. Decide the default/backfill before
   running it; don't let the migration surprise you. (SQL-backed services only — a Redis document has no
   such column cost.)

---

## Binding rules (apply to both variants)

- **Domain events are an aggregate-owned side channel** — only `AddDomainEvent` (EF) or endpoint
  `PublishAsync` (Redis) raises them; observers read `DomainEvents`, never inject. *(domain-1)*
- **State vs behavior are separate files**, same class name, both `partial`; the state machine is the
  one sanctioned extra enforcement site. *(domain-2)*
- **Behavior-driving enum metadata is an `EnumEntity`**, a seeded queryable entity — not `[Description]`
  attributes + reflection. *(domain-4)*
- **Value objects: private ctor + static `Create` + `Guard.ThrowIf*` first.** *(domain-5)*
- **Invariant violations throw `DomainException`** (from `Blocks.Domain`) — the Domain layer never
  references `Blocks.Exceptions` or any HTTP type. *(errors-2)*
- **Action parameter is always last** in behavior methods — order them entity/domain args → domain
  services/delegates → `action` (owner feedback; Submission's four `Article` behaviors are known drift).

## Verify (finalize — run before reporting done)

1. Build the touched Domain + Persistence projects (`dotnet build`).
2. `AddDomainEvent` appears **only** inside the aggregate's own `Entities/` + `Behaviors/` files —
   grep the service; a hit in a handler or consumer violates domain-1.
3. The Domain project references **no** HTTP exception type — grep `{Svc}.Domain` for `Blocks.Exceptions`,
   `HttpException`, `BadRequestException`; expect zero (errors-2).
4. Each new value object has a `private {ValueObject}(` ctor **and** a `static {ValueObject} Create(` (domain-5).
5. State and behavior files both declare `partial class {Aggregate}` (domain-2).
6. EF variant only: the migration was added and applies cleanly.

## Templates

`templates/ef-state.cs`, `templates/ef-behavior.cs`, `templates/value-object.cs`,
`templates/domain-event.cs`, `templates/enum-entity.cs`, `templates/ef-configuration.cs`,
`templates/redis-entity.cs`. Each is real code from the live examples with `{Placeholders}`; copy,
rename, and strip the guidance comments.
