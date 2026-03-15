# ArticleTimeline: Service-Agnostic Refactoring

## Context

The ArticleTimeline module is meant to be a reusable cross-cutting module that tracks article lifecycle events. Currently it has three critical problems:

1. **Hard coupling**: `ArticleTimeline.Application.csproj` directly references `Production.Persistence.csproj` — cannot reuse in Review or Submission
2. **Dispatch mismatch**: Production uses `Blocks.FastEndpoints.DomainEventPublisher` (resolves `IEventHandler<T>`), but timeline handlers implement `INotificationHandler<T>` (MediatR) — **handlers never fire**
3. **Missing event coverage**: `ArticleStageChanged<AssetActionType>` (most stage transitions) and article arrival in Production have no timeline tracking

**Goal**: ArticleTimeline depends only on `Articles.Abstractions`. All services can plug it in. All Production events are tracked through two consistent mechanisms: stage transitions and asset actions.

### Design decisions

- **All stage transitions flow through `ArticleStageChanged`** — no per-event exception handlers. When an article enters a service, it transitions from `None` to its initial stage via `SetStage()`, just like Review's `FromSubmission()` already does. This keeps the timeline system uniform.
- **`TypesetterAssigned` does not need its own timeline handler** — `AssignTypesetter` already calls `SetStage(InProduction)` which raises `ArticleStageChanged(Accepted→InProduction)`. The stage transition template covers it.
- **`ArticleAcceptedForProduction` does not need its own timeline handler** — `FromReview()` will call `SetStage(Accepted)` which raises `ArticleStageChanged(None→Accepted)`. The `ArticleAcceptedForProduction` domain event remains for the email notification handler only.
- **Asset actions are Production-specific** — the `AssetActionExecuted` handler moves from ArticleTimeline.Application to Production.API since assets only exist in Production.

---

## Phase 1: Shared ArticleStageChanged event in Articles.Abstractions

**NEW** `src/BuildingBlocks/Articles.Abstractions/Events/ArticleStageChanged.cs`
```csharp
namespace Articles.Abstractions.Events;

public record ArticleStageChanged(
    ArticleStage CurrentStage,
    ArticleStage NewStage,
    IArticleAction Action
) : DomainEvent<IArticleAction>(Action);
```

This matches the shape that Review and Submission already use locally. Non-generic — eliminates the `<ArticleActionType>` vs `<AssetActionType>` mismatch entirely.

---

## Phase 2: Timeline master data updates

**EDIT** `src/Modules/ArticleTimeline/ArticleTimeline.Persistence/MasterData/TimelineTemplate.json`
- Add template for article entering Production:
```json
{"SourceType": "StageTransition", "SourceId": "None->Accepted", "TitleTemplate": "Article accepted for production", "DescriptionTemplate": "Article has been accepted for production."}
```

**EDIT** `src/Modules/ArticleTimeline/ArticleTimeline.Persistence/MasterData/TimelineVisibility.json`
- Add visibility rules for `None->Accepted` (POF, TSOF, CORAUT)

No changes to SourceType enum — all timeline entries use the existing `StageTransition` and `ActionExecuted` source types.

---

## Phase 3: Switch Production to MediatR domain event publisher

This fixes the dispatch mismatch — timeline handlers (INotificationHandler) will actually fire.

**EDIT** `src/Services/Production/Production.API/Production.API.csproj`
- Add reference to `Blocks.MediatR`

**EDIT** `src/Services/Production/Production.API/DependencyInjection.cs`
- Change `IDomainEventPublisher` registration from FastEndpoints to MediatR:
  ```csharp
  services.AddScoped<IDomainEventPublisher, Blocks.MediatR.DomainEventPublisher>();
  ```
- Add MediatR registration for Production.API assembly (ArticleTimeline already registers MediatR for its own assembly; this adds Production's handlers):
  ```csharp
  services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
  ```

**EDIT** `src/Services/Production/Production.API/Features/Articles/InitializeFromReview/NotifyProductionOfficeOnArticleAcceptedHandler.cs`
- Change from `IEventHandler<ArticleAcceptedForProduction>` (FastEndpoints) to `INotificationHandler<ArticleAcceptedForProduction>` (MediatR)
- Rename `HandleAsync` → `Handle` (MediatR convention)

---

## Phase 4: Move AssetActionExecuted handler to Production

This is the only Production-specific timeline handler needed — asset actions don't map to stage transitions.

**NEW** `src/Services/Production/Production.API/Features/Articles/Timeline/AddTimelineWhenAssetActionExecutedHandler.cs`
- Moved from ArticleTimeline.Application (where it currently lives but will be deleted)
- Handles `AssetActionExecuted` : `AddTimelineEventHandler<AssetActionExecuted, IArticleAction<AssetActionType>>`
- `GetSourceType() => SourceType.ActionExecuted`
- `GetSourceId() => $"{eventModel.action.Action}"`

---

## Phase 5: Decouple ArticleTimeline.Application from Production

**EDIT** `src/Modules/ArticleTimeline/ArticleTimeline.Application/ArticleTimeline.Application/ArticleTimeline.Application.csproj`
- **Remove** `<ProjectReference>` to `Production.Persistence.csproj`
- The shared event type is available transitively: ArticleTimeline.Domain → Articles.Abstractions

**EDIT** `src/Modules/ArticleTimeline/ArticleTimeline.Application/ArticleTimeline.Application/EventHandlers/AddTimelineWhenArticleStageChangedEventHandler.cs`
- Change from `ArticleStageChanged<ArticleActionType>` (Production-specific generic) to `Articles.Abstractions.Events.ArticleStageChanged` (shared non-generic)
- Update: `AddTimelineEventHandler<ArticleStageChanged, IArticleAction>`
- Remove Production.Domain imports, add `using Articles.Abstractions.Events;`

**DELETE** `src/Modules/ArticleTimeline/ArticleTimeline.Application/ArticleTimeline.Application/EventHandlers/AddTimelineWhenActionExecutedEventHandler.cs`
- Moved to Production.API in Phase 4

**DELETE** `src/Modules/ArticleTimeline/ArticleTimeline.Domain/Events/DomainEvent.cs`
- Unused legacy type (`Service.Domain.Events.DomainEvent<TActiontype>` with MediatR INotification). The actual base is `Articles.Abstractions.DomainEvent<TAction>`.

---

## Phase 6: All services raise the shared ArticleStageChanged

### Production

**EDIT** `src/Services/Production/Production.Domain/Articles/Behavior/Article.cs`
- In `SetStage<TActionType>`, change the event from:
  ```csharp
  AddDomainEvent(new ArticleStageChanged<TActionType>(currentStage, newStage, action));
  ```
  to:
  ```csharp
  AddDomainEvent(new Articles.Abstractions.Events.ArticleStageChanged(currentStage, newStage, action));
  ```
  The `action` (typed `IArticleAction<TActionType>`) implicitly converts to `Articles.Abstractions.IArticleAction`.

- In `FromReview()`, replace direct stage assignment with `SetStage()` to generate a timeline entry for the article's arrival in Production:
  ```csharp
  // Before:
  var article = new Article { Stage = articleDto.Stage, ... };

  // After: Stage defaults to None (0), then transitions via SetStage
  var article = new Article { /* Stage omitted — defaults to None */ ... };
  article.SetStage(articleDto.Stage, action);  // raises ArticleStageChanged(None → Accepted)
  ```
  This follows the same convention as Review's `FromSubmission()` which calls `SetStage(UnderReview, ...)`.

**DELETE** `src/Services/Production/Production.Domain/Articles/Events/ArticleStageChanged.cs`
- The generic `ArticleStageChanged<TActionType>` is no longer needed. No other consumers exist.

### Review

**DELETE** `src/Services/Review/Review.Domain/Articles/Events/ArticleStageChanged.cs`
- Local non-generic version replaced by shared one

**EDIT** `src/Services/Review/Review.Domain/Articles/Behaviors/Article.cs`
- Change `using Review.Domain.Articles.Events;` → add `using Articles.Abstractions.Events;`
- The `new ArticleStageChanged(...)` call resolves to the shared type. Review's `IArticleAction` (alias for `Review.Domain.Shared.IArticleAction : IArticleAction<ArticleActionType>`) implicitly converts to `Articles.Abstractions.IArticleAction`.

### Submission

**DELETE** `src/Services/Submission/Submission.Domain/Events/ArticleStageChanged.cs`
- Local non-generic version replaced by shared one

**EDIT** `src/Services/Submission/Submission.Domain/Behaviours/Article.cs`
- Add `using Articles.Abstractions.Events;`
- The `new ArticleStageChanged(...)` call resolves to the shared type. Submission's `IArticleAction<ArticleActionType>` implicitly converts to `Articles.Abstractions.IArticleAction`.

**Note on global usings**: Submission has `global using Submission.Domain.Events;` which previously brought `ArticleStageChanged` into scope. After deleting the local file, the namespace still exists (has `ArticleCreated`, `ArticleApproved`, etc.) — no conflict. The `using Articles.Abstractions.Events;` in the behavior file resolves the shared version.

---

## Deferred / Future

**Download tracking (GAP 4)**: Not included. Downloads are read-only. Can be added later by raising `AssetActionExecuted` from `DownloadFileEndpoint` + adding a template row.

**Publish/SchedulePublication (GAP 6)**: Templates for `FinalProduction→Published` already exist. When the endpoint is implemented and calls `SetStage(Published, ...)`, the shared handler picks it up automatically.

**Review/Submission integration**: After this refactoring, enabling timeline in Review/Submission is just:
1. Add `AddArticleTimeline()` to their Program.cs
2. Add stage transition templates for their stage ranges (Created→Submitted, UnderReview→Accepted, etc.)
3. Both already use MediatR — handlers fire automatically

---

## Verification

1. `dotnet build` — verify all projects compile (especially Production.API, ArticleTimeline.Application, Review.Domain, Submission.Domain)
2. Verify `FromReview` flow: should produce timeline entry for `None→Accepted` stage transition (article entering Production)
3. Verify Production's `AssignTypesetter` flow: should produce timeline entry for `Accepted→InProduction` stage transition
4. Verify `UploadDraftFile` flow: should produce timeline entries for `AssetActionExecuted(Upload)` AND `InProduction→DraftProduction` stage transition
5. Verify `ApproveDraftAsset` flow: should produce timeline entries for `AssetActionExecuted(Approve)` AND `DraftProduction→FinalProduction` stage transition

---

## Critical files

| File | Action | Why |
|------|--------|-----|
| `BuildingBlocks/Articles.Abstractions/Events/ArticleStageChanged.cs` | NEW | Foundation — shared event all services raise |
| `Modules/ArticleTimeline/ArticleTimeline.Application/.../ArticleTimeline.Application.csproj` | EDIT | Remove Production.Persistence reference |
| `Modules/ArticleTimeline/ArticleTimeline.Application/.../AddTimelineWhenArticleStageChangedEventHandler.cs` | EDIT | Use shared event type |
| `Modules/ArticleTimeline/ArticleTimeline.Application/.../AddTimelineWhenActionExecutedEventHandler.cs` | DELETE | Moved to Production |
| `Modules/ArticleTimeline/ArticleTimeline.Domain/Events/DomainEvent.cs` | DELETE | Unused legacy type |
| `Modules/ArticleTimeline/ArticleTimeline.Persistence/MasterData/TimelineTemplate.json` | EDIT | New `None→Accepted` template |
| `Modules/ArticleTimeline/ArticleTimeline.Persistence/MasterData/TimelineVisibility.json` | EDIT | Visibility rules for `None→Accepted` |
| `Services/Production/Production.API/DependencyInjection.cs` | EDIT | Switch to MediatR publisher + register MediatR |
| `Services/Production/Production.API/Production.API.csproj` | EDIT | Add Blocks.MediatR reference |
| `Services/Production/Production.API/.../NotifyProductionOfficeOnArticleAcceptedHandler.cs` | EDIT | IEventHandler → INotificationHandler |
| `Services/Production/Production.API/.../Timeline/AddTimelineWhenAssetActionExecutedHandler.cs` | NEW | Production-specific handler (moved from ArticleTimeline) |
| `Services/Production/Production.Domain/Articles/Behavior/Article.cs` | EDIT | Raise shared event + `FromReview` uses `SetStage` |
| `Services/Production/Production.Domain/Articles/Events/ArticleStageChanged.cs` | DELETE | Replaced by shared event |
| `Review/Review.Domain/Articles/Events/ArticleStageChanged.cs` | DELETE | Replaced by shared event |
| `Review/Review.Domain/Articles/Behaviors/Article.cs` | EDIT | Import shared event |
| `Submission/Submission.Domain/Events/ArticleStageChanged.cs` | DELETE | Replaced by shared event |
| `Submission/Submission.Domain/Behaviours/Article.cs` | EDIT | Import shared event |
