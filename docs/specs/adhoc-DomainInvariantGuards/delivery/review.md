# adhoc-DomainInvariantGuards — Review

## Step 1 — Done-Check

Done-check method: `implementation.md` mapped against the revised plan's 7 steps; the plan's Step-7 acceptance greps re-executed at done-check (the plan mandates this); skill conformance scored against `.claude/audit/skill-invocations.log` (authoritative), scoped to `agent:developer, token:developer:implement`.

Pre-commitment predictions (made before reading implementation.md): (1) a stray `using Production.Application.StateMachines` left in the API after the 10-file swap; (2) the delegate not fully deleted from Application; (3) Step 5/6 `TryDeleteAsync` present but not inside a catch, or tracking only the current file. **All three predicted gaps came back clean.**

| Step | Disposition | Notes |
|------|-------------|-------|
| 1 — Submission: close setters + Q1 `Article.Create` factory | Implemented | `Article.Stage:15` + `Asset.File:15` → `private set` (grep-confirmed). Q1 fix per Decision #4: `Stage = ArticleStage.Created` now lives only in `Submission.Domain/Behaviors/Article.cs:18` (Article's own same-class factory), `Journal.CreateArticle` delegates to `Article.Create(...)`; `grep "Stage = ArticleStage" src/Services/Submission` → 1 hit, in the factory, not `Journal.cs`. No `InitializeStage` seam, no `SetStage` routing — `ArticleCreated` is the only event on creation. |
| 2 — Review: close setters | Implemented | `ReviewInvitation.Status:27` + Review `Asset.File:21` → `private set` (grep-confirmed). `InvitationStatus Status { get; set; }` sweep → 0 hits. Sole creation site does not set Status; `Accept`/`Decline` are members of `ReviewInvitation` itself. |
| 3 — Production: close Article.Stage | Implemented | Production `Article.Stage:17` → `private set` (grep-confirmed). Sole writer is `SetStage`; `FromReview` defaults to `None` then transitions. |
| 4 — Production: Asset guard into domain (Q2 seam) | Implemented | Seam created: `Production.Domain/StateMachines/IAssetStateMachine.cs:5` interface + `:10` relocated delegate; `AssetStateMachine : IAssetStateMachine` (`:11`); delegate fully removed from Application (`delegate.*AssetStateMachineFactory` in Production.Application → 0). All 10 API files swapped `using` → Domain (`using Production.Application.StateMachines` in Production.API → 0, a clean swap not an add). Guard in `Asset.SetState` (`Asset.cs:64-65` `CanFire` → `DomainException`); `CheckAndThrowStateTransition` deleted repo-wide (→ 0). Materialization (`action == null`) branch unchanged, exempt per Decision #3. Minor documented developer choice: `CreateAndAddFile`'s new factory param made optional/nullable so the 2-arg materialization call site is unchanged — in scope ("action != null branch only" honored), guard fires only on the transition branch. |
| 5 — Review consumer: compensate byte migration | Implemented | `catch (Exception)` at `:55`, `TryDeleteAsync` per uploaded file at `:60`, then rethrow (Decision #1). Whole post-head-guard body wrapped, so a `SaveChangesAsync`-stage failure still compensates every prior upload. |
| 6 — Production consumer: compensate byte migration | Implemented | Same shape against Azure Blob store: `catch (Exception)` at `:53`, `TryDeleteAsync` at `:58`, rethrow (Decision #1). |
| 7 — Verification sweep (hard gate) | Implemented | Developer re-ran all Step-7 greps; architect re-executed them at done-check — all match: `ArticleStage Stage { get; set; }` → exactly 1 (ArticleHub, sanctioned); Status/File public-setter sweeps → 0 in scope; `CheckAndThrowStateTransition` → 0; `dotnet build src/Articles.sln` → 0 errors (42 pre-existing warnings). |

Skill conformance (log-scored): plan's non-`None` mapped skills — Step 4 `nexus-dotnet:add-state-machine`, Steps 5-6 `file-storage-patterns` — both present in the log under `agent:developer, token:developer:implement` (lines 36-37). `## Skills Used` present and corroborated by the log (no fabrication). Steps 1-3, 7 map `Skill: (none)` — exempt. `TDD: no` throughout (owner decision) — no `tdd` invocation expected. Pass.

Plan-hygiene: `## Decisions` present and non-silent (5 rows, #1-#5). `Satisfies:` referents (td sub-5/sub-6/rev-5/rev-12/prod-13/prod-5/rev-6/prod-8) are the ratified in-scope registry rows named in the plan's binding-input — existence-validated, pass.

Boy-scout cleanup assessed: removed the now-unused `using Blocks.Exceptions;` from `AssetBaseEndpoint.cs` after deleting its only consumer (`CheckAndThrowStateTransition`); `BadRequestException`/`Blocks.Exceptions` grep in that file → 0, build green — a legitimate in-file dead-import removal on a file the step already modified, not scope creep. Disclosed in Deviations.

Deviations from plan: none. All 7 steps (sub-steps 1.1/1.2, 4.1–4.4) implemented as specified in the revised, architect-approved plan.

**Verdict: PASS**

*Status: COMPLETE — architect, 2026-07-11*

## Step 2 — Code Review

## Reviewed By
reviewer

## Verdict: APPROVED

## Pre-commitment Predictions
Before reading code, expected to find one of:
1. A guard call site among the 4 mutating Asset paths (Upload/Approve/Request/CancelRequest) left un-threaded with `_stateMachineFactory`, silently reverting to no-guard behavior. **Not found** — traced all 4 call sites (`UploadFileEndpoint.cs:34`, `ApproveDraftAssetEndpoint.cs:24`, `RequestAssetsEndpointBase.cs:28`, `CancelRequestFinalAssetsEndpoint.cs:33`); all pass `_stateMachineFactory` through to `SetState`/`CancelRequest`/`CreateAndAddFile`.
2. `CanFire`'s `_triggers[actionType]` dictionary indexer throwing `KeyNotFoundException` for an action type reachable from an endpoint but absent from the seed table. **Not found** — all 4 action types the endpoints exercise (Upload, Request, Approve, CancelRequest) appear in `AssetStateTransition.json`; the 5th enum value (`Download`) is never routed through `CanFire` (download is a pure read, no state mutation).
3. The consumer compensation catch block deleting from the wrong file store (source instead of destination) or using a pre-upload storage path. **Not found** — both consumers accumulate the metadata *returned by* the destination store's `UploadAsync` call and call `TryDeleteAsync` on that same destination service instance.
4. A design smell in the two new nullable-together parameters on `Asset.CreateAndAddFile` (`action`/`stateMachineFactory`). **Found** — see MEDIUM finding below; current call sites are consistent, but nothing at the type level prevents a future caller from passing `action` without `stateMachineFactory`, which would NRE instead of raising the intended `DomainException`.

## Findings

### [MEDIUM] `Asset.CreateAndAddFile`'s guard dependency is coupled to `action` by convention only, not by the type system
**File:** `src/Services/Production/Production.Domain/Assets/Behaviors/Asset.cs:81-91`
**Origin:** implementation
**Issue:** `CreateAndAddFile(FileMetadata uploadResponse, AssetTypeDefinition assetTypeDefinition, IArticleAction<AssetActionType>? action = null, AssetStateMachineFactory? stateMachineFactory = null)` guards the coupling only via a null-forgiving operator:
```csharp
if (action is not null)
    SetState(AssetState.Uploaded, action, stateMachineFactory!);   // :89
else
    State = AssetState.Uploaded;
```
Both new parameters are independently optional/nullable, but the method only works correctly if they are supplied *together* (both null, for materialization, or both non-null, for a guarded transition). That invariant is enforced nowhere — not by the signature, not by a guard clause. Today both call sites happen to respect it (`UploadFileEndpoint.cs:34` passes both `command` and `_stateMachineFactory`; `ArticleAcceptedForProductionConsumer.cs:129` passes neither), so there is no live bug. But a future caller that passes `action` while omitting `stateMachineFactory` (an easy mistake — the compiler only warns, and the plan's `!` operator explicitly suppresses even that) will hit a raw `NullReferenceException` inside `SetState` when `stateMachineFactory(this.State)` is invoked on a null delegate, instead of the intended, guard-appropriate `DomainException`. That is a materially worse failure mode for a pass whose entire point is making invariant violations fail predictably in the domain rather than silently or unpredictably.
**Fix:** Add an explicit guard at the top of the method, e.g. `if (action is not null && stateMachineFactory is null) throw new ArgumentNullException(nameof(stateMachineFactory), "stateMachineFactory is required when action is supplied.");` — converts a future silent NRE into an immediate, self-documenting failure at the call site that introduces the mismatch. (Not required to fix this cycle — MEDIUM, no live defect — but flagged because the whole pass exists to remove exactly this class of "guard bypassable under a plausible future edit" risk.)
**Confidence:** 80/100

## Positive Observations
- **Guard-fires-on-every-upload-path trace (focus area 1) is clean.** Every one of the 4 previously endpoint-guarded mutations (`UploadFileEndpoint` → `CreateAndAddFile`, `ApproveDraftAssetEndpoint` → `SetState`, `RequestAssetsEndpointBase` → `SetState`, `CancelRequestFinalAssetsEndpoint` → `CancelRequest`) now threads `_stateMachineFactory` into the domain call, and `CheckAndThrowStateTransition` is fully deleted (`grep -rn "CheckAndThrowStateTransition" src/Services/Production` → 0 hits, re-run fresh). The two direct `State = AssetState.None` factory-initializer writes and the one `State = AssetState.Uploaded` materialization write (`Asset.cs:42,58,91`) are the only assignments outside `SetState`, and all three are exempt per Decision #3 (initialization ≠ transition) — confirmed by grep, no 4th bypass path exists.
- **CanFire / seed-table correctness (focus area 2) verified independently, not just re-stated from implementation.md.** Read `AssetStateTransition.json` and `AssetStateTransitionCondition.json` directly: all 4 action types the endpoints route through `CanFire` (Upload, Request, Approve, CancelRequest) have transition rows; `Download` (the 5th `AssetActionType` enum member) is never passed to `CanFire` anywhere in the codebase (`DownloadFileEndpoint` is a pure read, no domain call), so the dictionary-indexer risk in `AssetStateMachine.CanFire`'s `_triggers[actionType]` (a pre-existing `KeyNotFoundException` shape, byte-for-byte unchanged by this pass) is not newly reachable.
- **Consumer compensation (focus area 3) correctly targets the destination store with the destination storage path in both consumers.** `ArticleApprovedForReviewConsumer` accumulates `fileMetadata` returned by `_reviewFileService.UploadAsync` and deletes via that same `_reviewFileService` instance; `ArticleAcceptedForProductionConsumer` does the identical thing against `azureBlobFileService`. Both wrap the *entire* post-head-guard body (not just the upload loop) in the try/catch, so a `SaveChangesAsync` failure or a domain-validation exception raised during `Article.FromSubmission`/`Article.FromReview` also triggers compensation — a stronger guarantee than the plan's minimum ask, and still `rethrow`s so MassTransit redelivery is preserved. `TryDeleteAsync` on both concrete `IFileService` implementations checked (`FileService.AzureBlob`, `FileService.MongoGridFS`) is internally exception-safe for its store's expected not-found case, matching the existing single-file `UploadFileCommandHandler`/`UploadFileEndpoint` compensating-delete pattern this pass extends to the multi-file consumer loops — not a new risk shape.
- **Q1 factory (`Article.Create`, focus area 4) is behavior-identical to the pre-change inline construction** — same 7 initializer members, single `ArticleCreated` event, `_articles.Add` retained in `Journal.CreateArticle`; accessibility (`public static`) matches both cited exemplars (`Review.Article.FromSubmission`, `Production.Article.FromReview`). Note: the plan/task framed this focus area as "the nullable factory-param choice in Article.Create," but Submission's `Article.Create` takes no factory parameter at all (Decision #4 explicitly rejects routing creation through `SetStage`/a state-machine factory) — the only nullable-factory-parameter design choice in this diff is on `Asset.CreateAndAddFile` (Production), addressed as the MEDIUM finding above.
- Fresh `dotnet build src/Articles.sln` (this review session) matches implementation.md's claimed 0 errors / 42 pre-existing warnings exactly.
- All Step-7 hard-gate greps re-run fresh in this review session reproduce implementation.md's and the architect's claimed outputs exactly (see Evidence).
- Broad re-sweep beyond the plan's own greps (`\bStage\s*=`, `\bStatus\s*=`, `\bFile\s*=` across Submission/Review/Production) found no additional writers outside each type's own declaring class — corroborates the "zero silent-break path" claim independent of the plan's narrower acceptance greps.

## Gaps
- **Carry-over finding (developer → reviewer/architect), confirmed, not a gap in this pass:** "No standing tests cover the relocated Asset guard or the consumer compensation paths." Confirmed accurate — no test project changes appear in the diff, and the plan's own Testing Strategy states `TDD: no` throughout as an explicit owner decision (2026-07-11), with a named post-merge follow-up (`mine-verify-cover` M1 on `Production.Domain Asset` behaviors). This review did not require test evidence per the plan's own waiver; treating it as confirmed-and-accepted, not an open gap requiring a fix cycle.
- No test exercises the `action`-without-`stateMachineFactory` edge case from the MEDIUM finding above (expected, since no such caller exists yet) — noted for whoever eventually adds the `ArgumentNullException` guard.
- Boundary/threshold tests: N/A — this pass has no `> N`/`>= N` threshold logic.
- Data-loading depth: `this.Article` is dereferenced in `SetState`'s guard (`this.Article.Stage`) — confirmed (per plan's own grounding, independently re-checked by reading all 4 call sites) that `_article` is loaded via `GetByIdWithAssetsAsync`/equivalent on every path reaching `SetState`, so no new null-navigation risk from moving the guard.

## Open Questions
None below the confidence cutoff — the one finding raised (nullable factory-param coupling) scored 80 and is reported as a Finding, not here.

## Evidence
| Check | Result | Command | Output |
|-------|--------|---------|--------|
| Build | pass | `dotnet build src/Articles.sln` (fresh, this session) | Build succeeded, 0 Error(s), 42 Warning(s) — all pre-existing NuGet advisory/version-constraint warnings, none introduced by this pass. Matches implementation.md exactly. |
| Stage public-setter sweep | pass (1 sanctioned hit) | `grep -rn "ArticleStage Stage { get; set; }" src/Services --include=*.cs` | `ArticleHub.Domain/Entities/Article.cs:9` only (sanctioned read-model exception) |
| ReviewInvitation.Status sweep | pass (0 hits) | `grep -rn "InvitationStatus Status { get; set; }" src/Services --include=*.cs` | no matches |
| Asset.File public-setter sweep | pass (0 hits) | `grep -rn "File File { get; set; }" src/Services/Submission src/Services/Review --include=*.cs` | no matches |
| Endpoint guard removal sweep | pass (0 hits) | `grep -rn "CheckAndThrowStateTransition" src/Services/Production --include=*.cs` | no matches |
| Delegate fully relocated | pass (0 hits) | `grep -rn "delegate.*AssetStateMachineFactory" src/Services/Production/Production.Application --include=*.cs` | no matches |
| API `using` fully swapped | pass (0 hits) | `grep -rn "using Production.Application.StateMachines" src/Services/Production/Production.API --include=*.cs` | no matches |
| Q1 compile-gap resolved | pass (1 hit, in factory) | `grep -rn "Stage = ArticleStage" src/Services/Submission --include=*.cs` | `Submission.Domain/Behaviors/Article.cs:18` only, inside `Article.Create` |
| Broad re-sweep, no missed writers | pass | `grep -rn "\bStage\s*=" \| "\bStatus\s*=" \| "\bFile\s*="` across Submission/Review/Production (reviewer-added, beyond plan's own greps) | Every hit outside migrations resolves to the declaring type's own partial-class member — no external writer found |
| Guard call-site trace | pass | Manual read + grep of all 4 mutating Asset call sites (`UploadFileEndpoint.cs:34`, `ApproveDraftAssetEndpoint.cs:24`, `RequestAssetsEndpointBase.cs:28`, `CancelRequestFinalAssetsEndpoint.cs:33`) | All 4 thread `_stateMachineFactory` into the domain call; no bypass path |
| Seed-table coverage | pass | Read `AssetStateTransition.json` + `AssetStateTransitionCondition.json` directly | All 4 action types the endpoints route through `CanFire` are present; `Download` never reaches `CanFire` |

*Status: COMPLETE — reviewer, 2026-07-11.*
