# DomainInvariantGuards — Implementation

## Files Created

- `src/Services/Production/Production.Domain/StateMachines/IAssetStateMachine.cs` — Domain-layer `IAssetStateMachine` interface (`CanFire(ArticleStage, AssetType, AssetActionType)`) + the relocated `AssetStateMachineFactory` delegate, mirroring the sibling `IArticleStateMachine.cs`. Resolves Q2 / Decision #5 (Step 4.1).

## Files Modified

- `src/Services/Submission/Submission.Domain/Entities/Article.cs` — `Stage { get; set; }` → `private set` (Step 1.1, td sub-5).
- `src/Services/Submission/Submission.Domain/Entities/Asset.cs` — `File { get; set; }` → `private set` (Step 1.1, td sub-6).
- `src/Services/Submission/Submission.Domain/Behaviors/Article.cs` — added `static Article Create(string title, ArticleType type, string scope, Journal journal, IArticleAction action)` factory: builds the Article (including the now-domain-internal `Stage = ArticleStage.Created` initializer) and raises `ArticleCreated`. Resolves Q1 / Decision #4 (Step 1.2).
- `src/Services/Submission/Submission.Domain/Behaviors/Journal.cs` — `CreateArticle` now delegates to `Article.Create(...)` and keeps `_articles.Add(article)`; removed the inline object-initializer construction that set `Stage` from a sibling class (would no longer compile once `Stage` is `private set`).

- `src/Services/Review/Review.Domain/Invitations/ReviewInvitation.cs` — `Status { get; set; }` → `private set` (Step 2, td rev-5). No caller update needed: `Accept()`/`Decline()` (`Behaviors/ReviewInvitation.cs`) are members of `ReviewInvitation` itself; the sole creation site (`Article.cs:130 CreateInvitation`) does not set `Status`.
- `src/Services/Review/Review.Domain/Assets/Asset.cs` — `File { get; set; }` → `private set` (Step 2, td rev-12). No caller update needed: both `File = File.CreateFile(...)` sites are `Asset`'s own `CreateFile` members.
- `src/Services/Production/Production.Domain/Articles/Article.cs` — `Stage { get; set; }` → `private set` (Step 3, td prod-13). No caller update needed: sole writer is `Behaviors/Article.cs:42` inside `SetStage`; `FromReview` defaults to `None` and transitions via `SetStage`.

- `src/Services/Production/Production.Application/StateMachines/AssetStateMachine.cs` — class now `: IAssetStateMachine`; deleted the `AssetStateMachineFactory` delegate declaration (moved to Domain); added `using Production.Domain.StateMachines;` (Step 4.1, td prod-5).
- 10 Production.API files — swapped `using Production.Application.StateMachines;` → `using Production.Domain.StateMachines;` (the delegate's new home); none referenced the concrete `AssetStateMachine` class, so this is the only change needed in each: `Features/_Shared/AssetBaseEndpoint.cs`, `Features/Assets/RequestAssets/_Shared/RequestAssetsEndpointBase.cs`, `Features/Assets/UploadFiles/_Shared/UploadFileEndpoint.cs`, `Features/Assets/ApproveAssets/Draft/ApproveDraftAssetEndpoint.cs`, `Features/Assets/UploadFiles/UploadSupplementaryFile/UploadSupplementaryFileEndpoint.cs`, `Features/Assets/RequestAssets/Final/RequestFinalAssetsEndpoint.cs`, `Features/Assets/RequestAssets/Supplementary/RequestSupplementaryAssetsEndpoint.cs`, `Features/Assets/RequestAssets/CancelRequest/CancelRequestFinalAssetsEndpoint.cs`, `Features/Assets/UploadFiles/UploadFinalFile/UploadFinalFileEndpoint.cs`, `Features/Assets/UploadFiles/UploadDraftFile/UploadDraftFileEndpoint.cs` (Step 4.1, td prod-5).
- `src/Services/Production/Production.Domain/Assets/Behaviors/Asset.cs` — `SetState` gained the validate-before-mutate guard as its first statement (`stateMachineFactory(this.State).CanFire(this.Article.Stage, this.Type, action.ActionType)` → `throw new DomainException(...)`) plus a new `AssetStateMachineFactory stateMachineFactory` parameter; `CancelRequest` and `CreateAndAddFile`'s `action != null` branch thread the factory through to `SetState`. Materialization (`action == null` branch) is unchanged — still the direct `State = Uploaded` assignment, exempt per Decision #3 (Step 4.2, td prod-5).
- `src/Services/Production/Production.API/Features/Assets/ApproveAssets/Draft/ApproveDraftAssetEndpoint.cs` — removed `CheckAndThrowStateTransition(asset, command.ActionType)`; `SetState` call now passes `_stateMachineFactory` (Step 4.2/4.3).
- `src/Services/Production/Production.API/Features/Assets/RequestAssets/_Shared/RequestAssetsEndpointBase.cs` — same: removed the endpoint guard call, `SetState` now passes `_stateMachineFactory` (Step 4.2/4.3).
- `src/Services/Production/Production.API/Features/Assets/RequestAssets/CancelRequest/CancelRequestFinalAssetsEndpoint.cs` — same: removed the endpoint guard call, `CancelRequest` now passes `_stateMachineFactory` (Step 4.2/4.3).
- `src/Services/Production/Production.API/Features/Assets/UploadFiles/_Shared/UploadFileEndpoint.cs` — same: removed the endpoint guard call, `CreateAndAddFile` now passes `_stateMachineFactory` as its 4th argument (Step 4.2/4.3).
- `src/Services/Production/Production.API/Features/_Shared/AssetBaseEndpoint.cs` — deleted the `CheckAndThrowStateTransition` method (single enforcement point now lives in `Asset.SetState`); removed the now-unused `using Blocks.Exceptions;` (boy-scout — `BadRequestException` was only used by the deleted method) (Step 4.3, td prod-5).

- `src/Services/Review/Review.Application/Features/Articles/InitializeFromSubmission/ArticleApprovedForReviewConsumer.cs` — `Consume` wraps the actor/asset/journal creation through `SaveChangesAsync` in try/catch; `CreateAssets` now accumulates each successfully uploaded `FileMetadata` into a caller-supplied `List<FileMetadata>`; on any exception the catch block calls `_reviewFileService.TryDeleteAsync` for every uploaded file, then rethrows (Step 5, td rev-6, Decision #1).
- `src/Services/Production/Production.API/Features/Articles/InitializeFromReview/ArticleAcceptedForProductionConsumer.cs` — same shape: `Consume` wraps journal/actor/asset creation through `SaveChangesAsync` in try/catch (after the `ExistsAsync` head-guard); `CreateAssets` accumulates uploaded `FileMetadata`; catch block calls `azureBlobFileService.TryDeleteAsync` for every uploaded file, then rethrows (Step 6, td prod-8, Decision #1).

## Key Decisions

- Step 1.2 implemented exactly per the plan's Decision #4: same-class factory, no `InitializeStage` seam, no `SetStage` routing — `ArticleCreated` is the only event raised, no new `ArticleStageChanged` on creation.
- Step 4.4 (seed-table verification): confirmed `AssetStateTransition.json` / `AssetStateTransitionCondition.json` (`Production.Persistence/Data/Master/`) already cover all four action types the endpoint paths exercise (Upload, Request, CancelRequest, Approve) across the states/stages those endpoints reach. No gap found — the guard's data and `CanFire` algorithm are byte-for-byte unchanged from the pre-existing endpoint-layer check; only the call site moved (endpoint → domain). Decision #3's fallback (assign initial state in the Create factory) was not needed since materialization already takes the exempt `action == null` direct-assignment branch, unchanged by this pass.
- `CreateAndAddFile`'s new `stateMachineFactory` parameter is optional (`AssetStateMachineFactory? stateMachineFactory = null`) so the materialization call site in `ArticleAcceptedForProductionConsumer.CreateAssets` (2-arg call, `action` also null/omitted) needs no signature change — the guard only fires on the `action is not null` branch, matching the plan's "action != null branch only" instruction.
- Steps 5/6 consumer compensation: wrapped the whole post-head-guard body (asset/actor/journal creation through `SaveChangesAsync`) in one try/catch per Decision #1, rather than only the upload loop — a failure anywhere after uploads start (including `SaveChangesAsync` itself) must still compensate every file uploaded so far in the message, not just mid-loop failures. `CreateAssets` now takes a `List<FileMetadata> uploadedFiles` accumulator parameter populated as each upload succeeds; the catch block iterates it and calls `TryDeleteAsync` per file before rethrowing.

## Skills Used

| Step | Skill(s) invoked | Notes |
|------|------------------|-------|
| 1 | None | plan-mapped `Skill: (none)`; accessibility tightening + same-class factory extraction, no pattern skill mapped |
| 2 | None | plan-mapped `Skill: (none)`; accessibility tightening only |
| 3 | None | plan-mapped `Skill: (none)`; accessibility tightening only |
| 4 | nexus-dotnet:add-state-machine | Invoked before implementing (Phase 2 Domain seam + Phase 4 guard-in-behavior), per plan's `Follow nexus-dotnet:add-state-machine` mapping |
| 5 | file-storage-patterns | Invoked before implementing; applied Phase 4 (compensating-delete) shape to the multi-file loop per Decision #1 |
| 6 | file-storage-patterns | Same skill/notes as Step 5 |
| 7 | None | plan-mapped `Skill: (none)`; verification sweep only, no code changes |

## Carry-Over Findings

| Title | Severity | For | Evidence | Note |
|-------|----------|-----|----------|------|
| No standing tests cover the relocated Asset guard or the consumer compensation paths | low | reviewer/architect | Plan's own Testing Strategy — owner-decided `TDD: no` throughout this pass | Matches the plan's post-merge recommendation: run `mine-verify-cover` M1 on `Production.Domain Asset` behaviors (guard just moved — highest-value net) and optionally Submission `Article` behaviors. Not a gap in this pass — carrying forward the plan's own recommendation for visibility. |

## KB Changes

None — no `docs/kb/` entry covers these units (per plan's KB Impact section; unchanged by implementation).

## Deviations from Plan

None. All 7 steps (including sub-steps 1.1/1.2 and 4.1–4.4) implemented as specified in the revised, architect-approved plan. One in-file boy-scout cleanup (removed the now-unused `using Blocks.Exceptions;` from `AssetBaseEndpoint.cs` after deleting `CheckAndThrowStateTransition`, its only consumer) — noted in Files Modified, not a deviation from any plan instruction.

## Step 7 — Verification Sweep Results

All hard-gate greps re-run after Step 6, matching the plan's specified outputs exactly:

- `grep -rn "ArticleStage Stage { get; set; }" src/Services --include=*.cs` → exactly 1 hit: `ArticleHub.Domain/Entities/Article.cs:9` (sanctioned, untouched).
- `grep -rn "InvitationStatus Status { get; set; }" src/Services --include=*.cs` → 0 hits.
- `grep -rn "File File { get; set; }" src/Services/Submission src/Services/Review --include=*.cs` → 0 hits.
- `grep -rn "CheckAndThrowStateTransition" src/Services/Production --include=*.cs` → 0 hits.
- Registry-row evidence re-runs (sub-5, sub-6, rev-5, rev-12, prod-13, prod-5, rev-6, prod-8) — all re-confirmed `private set` / guard-in-domain / `TryDeleteAsync`-in-catch; none reproduced the original finding. Q1 sweep (`Stage = ArticleStage` repo-wide in Submission) confirmed the only hit is inside `Article`'s own factory (`Behaviors/Article.cs:18`), not `Journal.cs`.
- `dotnet build src/Articles.sln` → Build succeeded, 0 Error(s), 42 warnings (all pre-existing NuGet advisory/version-constraint warnings, none introduced by this pass).
- Debug-artifact sweep (TODO/HACK/FIXME) across all 21 files touched this pass → 0 hits.

## Post-Review Fix

**Finding [MEDIUM]** (Step 2 review): `Asset.CreateAndAddFile` (`src/Services/Production/Production.Domain/Assets/Behaviors/Asset.cs`) takes `action` and `stateMachineFactory` as two independently-nullable parameters that must be supplied together — the coupling was enforced only by a null-forgiving `!` on `stateMachineFactory!` inside the `action is not null` branch. A future caller passing one without the other would hit a raw `NullReferenceException` instead of the intended `DomainException`.

**Fix:** added an explicit guard as the first statement of `CreateAndAddFile`, matching the plain `if (...) throw new DomainException(...)` idiom already used twice in this same file (`SetState`, `CancelRequest`) — `Blocks.Core.Guard`'s helpers were checked and don't fit (they throw `ArgumentException`/`ArgumentNullException`/`NotFoundException`, never `DomainException`, and the reviewer's stated intent was specifically a `DomainException` for this coupled invariant):

```csharp
if ((action is null) != (stateMachineFactory is null))
    throw new DomainException($"{nameof(action)} and {nameof(stateMachineFactory)} must be supplied together");
```

Scoped to `CreateAndAddFile` only — no other method touched.

**Self-verification of both existing call sites (both compile and pass through the new guard with zero behavior change):**
- `ArticleAcceptedForProductionConsumer.cs:129` — `asset.CreateAndAddFile(uploadedMetadata, assetType);` — 2-arg call, both `action` and `stateMachineFactory` omitted (both null) → guard condition `(true) != (true)` = `false` → no throw, unaffected (materialization path).
- `UploadFileEndpoint.cs:34` — `asset.CreateAndAddFile(fileMetadata, assetType, command, _stateMachineFactory);` — 4-arg call, both supplied → guard condition `(false) != (false)` = `false` → no throw, unaffected (guarded upload path).

**Build result:** `dotnet build src/Articles.sln` → Build succeeded, 0 Error(s), 42 warnings (same pre-existing NuGet advisory/version-constraint warnings as before this fix — none introduced).

*Status: COMPLETE — developer, 2026-07-11 (post-review fix applied).*
