---
name: article-state-machine
description: Add data-driven article stage-transition validation to a service — the ArticleStageTransition seed table, the IArticleStateMachine plus Stateless wrapper, the ArticleStateMachineFactory DI registration, and the ValidateStageTransition guard in the aggregate. Use when a service must gate ArticleStage changes by legal action, or when adding a new stage, action, or transition to a service that already has the machine.
---

# Article State Machine

Builds the data-driven guard that decides whether an article may move stage. Legal moves are stored as rows in a cached `ArticleStageTransition` table and enforced by `ValidateStageTransition` before the aggregate mutates its `Stage`. This is the sanctioned way to enforce the lifecycle invariant (reference-model **domain-3**, CONFIRMED).

The whole point: **legal transitions are data, not a hardcoded `switch`.** Adding or removing a legal move is a seed-JSON edit, never a code change.

## When to use

- Standing up stage-transition validation in a new write-side service (Submission / Review / Production shape).
- Adding a new legal transition, action, or stage to a service that already has the machine (jump to "Adding a new transition").

## Binding rules

1. **Transitions are DATA.** They live in the `ArticleStageTransition` seed table, loaded through the cache — never a `switch` or `if`-ladder in the domain (**domain-3**).
2. **Validate before mutate.** `ValidateStageTransition` is the *first* statement of `SetStage`, before `Stage` is reassigned and before the no-op short-circuit (**domain-3**).
3. **The guard throw is a sanctioned second enforcement site.** The `DomainException` raised from `IArticleStateMachine.cs` lives outside the `Behaviors/` files by design (**domain-2** — invariant enforcement is normally confined to behavior files; the state-machine guard is the deliberate exception). Do not "fix" it by relocating the throw.
4. **The table is `ICacheable`** — a whole-table, process-wide cache with a type-only key (**persistence-5**). Cache-backed services must prewarm it at startup or the lookup returns null.
5. **The domain never constructs the machine.** A per-service `ArticleStateMachineFactory` delegate is resolved from DI and passed into the aggregate behavior.

## Phase 1 — Transition entity + seed data

The entity is a metadata row: a `(CurrentStage, ActionType, DestinationStage)` triple. `IMetadataEntity` makes it seedable; `ICacheable` marks it for the whole-table cache.

```csharp
// {Service}.Domain/Entities/ArticleStageTransition.cs
public class ArticleStageTransition : IMetadataEntity, ICacheable
{
    public ArticleStage CurrentStage { get; set; }
    public ArticleActionType ActionType { get; set; }
    public ArticleStage DestinationStage { get; set; }
}
```

The EF config derives from `MetadataConfiguration` — whose base `Configure` already calls `ToTable(typeof(T).Name)` and `SeedFromJsonFile()`. You add the composite key and the enum conversions:

```csharp
// {Service}.Persistence/EntityConfigurations/ArticleStageTransitionConfiguration.cs
internal class ArticleStageTransitionConfiguration : MetadataConfiguration<ArticleStageTransition>
{
    public override void Configure(EntityTypeBuilder<ArticleStageTransition> builder)
    {
        base.Configure(builder);

        builder.HasKey(e => new { e.CurrentStage, e.ActionType, e.DestinationStage });

        builder.Property(e => e.CurrentStage).IsRequired().HasEnumConversion();
        builder.Property(e => e.DestinationStage).IsRequired().HasEnumConversion();
        builder.Property(e => e.ActionType).IsRequired().HasEnumConversion();
    }
}
```

The seed file **must** be named after the entity type and sit under `Data/Master/` — `SeedFromJsonFile()` resolves `{AppContext.BaseDirectory}Data/Master/{EntityName}.json`. A row whose `CurrentStage` equals its `DestinationStage` is a same-stage reentry (e.g. re-uploading an asset). `//` comments are tolerated in the JSON.

```json
// {Service}.Persistence/Data/Master/ArticleStageTransition.json
[
    {"CurrentStage": "None", "ActionType": "CreateArticle", "DestinationStage": "Created"},
    {"CurrentStage": "Created", "ActionType": "UploadAsset", "DestinationStage": "ManuscriptUploaded"},
    {"CurrentStage": "ManuscriptUploaded", "ActionType": "UploadAsset", "DestinationStage": "ManuscriptUploaded"},
    {"CurrentStage": "ManuscriptUploaded", "ActionType": "SubmitDraft", "DestinationStage": "Submitted"},
    {"CurrentStage": "Submitted", "ActionType": "ApproveDraft", "DestinationStage": "InitialApproved"},
    {"CurrentStage": "Submitted", "ActionType": "RejectDraft", "DestinationStage": "InitialRejected"}
]
```

Each service seeds only its own slice of the lifecycle. The `ArticleStage` vocabulary is shared (`Articles.Abstractions`), range-partitioned per service:

```csharp
public enum ArticleStage : int
{
    None = 0,
    // Submission: 101-105
    Created = 101, ManuscriptUploaded = 102, Submitted = 103,
    InitialRejected = 104, InitialApproved = 105,
    // Review: 201-205
    UnderReview = 201, ReadyForDecision = 202, AwaitingRevision = 203,
    Rejected = 204, Accepted = 205,
    // Production: 300-305
    InProduction = 300, DraftProduction = 301, FinalProduction = 302,
    PublicationScheduled = 304, Published = 305
}
```

`ArticleActionType` is a per-service enum (`{Service}.Domain` — e.g. `CreateArticle, UploadAsset, SubmitDraft, ApproveDraft, RejectDraft`). Every action named in the seed JSON must be a member of it.

## Phase 2 — IArticleStateMachine + Stateless wrapper

The interface, the factory delegate, and the `ValidateStageTransition` guard live together in the **Domain** layer. The guard is the enforcement point — it is where the lifecycle `DomainException` is thrown (**domain-2**).

```csharp
// {Service}.Domain/StateMachines/IArticleStateMachine.cs
public interface IArticleStateMachine
{
    bool CanFire(ArticleActionType actionType);
}

public delegate IArticleStateMachine ArticleStateMachineFactory(ArticleStage articleStage);

public static class Extensions
{
    public static void ValidateStageTransition(this ArticleStateMachineFactory factory, ArticleStage articleStage, ArticleActionType actionType)
    {
        var stateMachine = factory(articleStage);

        if (!stateMachine.CanFire(actionType))
            throw new DomainException($"Action {actionType} not allowed in the {articleStage} article stage");
    }
}
```

The concrete wrapper lives in the **Application** layer. It builds a Stateless machine from the cached transition table: a real move becomes `Permit`, a same-stage row becomes `PermitReentry`. Keep the `//todo` verbatim — it is the recorded future direction (see "Known future direction").

```csharp
// {Service}.Application/StateMachines/ArticleStateMachine.cs
//todo - reimplement it directly with the database (cached), remove stateless library
public class ArticleStateMachine : IArticleStateMachine
{
    private StateMachine<ArticleStage, ArticleActionType> _stateMachine;
    public ArticleStateMachine(ArticleStage articleStage, IMemoryCache cache)
    {
        _stateMachine = new(articleStage);

        var transitions = cache.Get<List<ArticleStageTransition>>();
        foreach (var transition in transitions)
        {
            if (transition.CurrentStage != transition.DestinationStage)
                _stateMachine.Configure(transition.CurrentStage)
                    .Permit(transition.ActionType, transition.DestinationStage);
            else
                _stateMachine.Configure(transition.CurrentStage)
                    .PermitReentry(transition.ActionType);
        }
    }

    public bool CanFire(ArticleActionType actionType)
    {
        return _stateMachine.CanFire(actionType);
    }
}
```

## Phase 3 — Factory delegate DI registration

Register the `ArticleStateMachineFactory` as **Scoped** in the service's `DependencyInjection.cs`, alongside the other scoped collaborators (`IArticleAccessChecker`, `IDomainEventPublisher`). Two proven variants — pick by how the transition table reaches the wrapper.

**Cache-backed** (Submission, Review) — the table is prewarmed into `IMemoryCache`:

```csharp
services.AddScoped<ArticleStateMachineFactory>(provider => articleStage =>
{
    var cache = provider.GetRequiredService<IMemoryCache>();
    return new ArticleStateMachine(articleStage, cache);
});
```

**DbContext-backed** (Production) — the wrapper reads `dbContext.GetAllCached<ArticleStageTransition>()` instead of `cache.Get<...>()`:

```csharp
services.AddScoped<ArticleStateMachineFactory>(provider => articleStage =>
{
    var dbContext = provider.GetRequiredService<ProductionDbContext>();
    return new ArticleStateMachine(articleStage, dbContext);
});
```

For the cache-backed variant, the transition table must be loaded into `IMemoryCache` at startup — Submission does this with a `DatabaseCacheLoader` hosted service (**persistence-5**). Without the prewarm, `cache.Get<List<ArticleStageTransition>>()` returns null and the constructor throws.

## Phase 4 — ValidateStageTransition in the aggregate behavior

`SetStage` is the single mutation point for `Stage`. The guard is its **first** line — before the no-op short-circuit, before any field is touched:

```csharp
// {Service}.Domain/Behaviors/Article.cs
public void SetStage(ArticleStage newStage, IArticleAction<ArticleActionType> action, ArticleStateMachineFactory stateMachineFactory)
{
    stateMachineFactory.ValidateStageTransition(Stage, action.ActionType);

    if (newStage == Stage) // there is no transition to be done
        return;

    var currentStage = Stage;
    Stage = newStage;
    LastModifiedOn = action.CreatedOn;
    LastModifiedById = action.CreatedById;

    _stageHistories.Add(
        new StageHistory { ArticleId = Id, StageId = newStage, StartDate = DateTime.UtcNow });

    AddDomainEvent(
        new ArticleStageChanged(currentStage, newStage, action));
}
```

Higher-level behaviors thread the factory through to `SetStage` — they receive it as a parameter and never new-up the machine:

```csharp
public void Approve(Person editor, IArticleAction<ArticleActionType> action, ArticleStateMachineFactory stateMachineFactory)
{
    _actors.Add(new ArticleActor { Person = editor, Role = UserRoleType.REVED });

    SetStage(ArticleStage.InitialApproved, action, stateMachineFactory);

    AddDomainEvent(new ArticleApproved(this, action));
}
```

The command handler resolves the `ArticleStateMachineFactory` from DI and passes it in; the `ActionType` comes from the command via `action.ActionType`.

## Adding a new transition

The common repeat case — no C# change unless the enum members are new:

1. Add the row to `{Service}.Persistence/Data/Master/ArticleStageTransition.json` (`CurrentStage` = `DestinationStage` for a same-stage reentry).
2. If the action or stage is new, add the member — `ArticleActionType` in `{Service}.Domain`, `ArticleStage` in `Articles.Abstractions` (keep the per-service numeric range).
3. Reseed. The table is seed-on-configure metadata; the cache reloads it at startup.

That is the full change — the machine picks the new move up as data.

## Known future direction

The `//todo` on the Application-layer wrapper records the intended evolution: replace the Stateless library with direct DB-backed (cached) checks. Production is already partway — its wrapper reads from `dbContext.GetAllCached<ArticleStageTransition>()` — but still wraps Stateless. When you implement the todo, keep the `IArticleStateMachine.CanFire` seam and the `ValidateStageTransition` guard identical; only the wrapper internals change, so no call site or DI registration moves.

## What this skill does NOT do

- Create the `Article` aggregate, the `ArticleStage` / `ArticleActionType` enums, or the command and handler — the aggregate comes from the project-local `create-aggregate` skill; this skill assumes it already exists and only adds the transition guard to it.
- Build the cache prewarm loader — that is the caching/persistence setup (**persistence-5**), referenced here, not created here.
- Add cross-service integration events — `ArticleStageChanged` is an in-process domain event; the boundary between domain and integration events is out of scope.
