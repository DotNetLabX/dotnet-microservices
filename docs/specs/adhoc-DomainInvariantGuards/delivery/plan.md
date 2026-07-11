# adhoc-DomainInvariantGuards

**Feature Spec:** None — ad-hoc refactoring pass. Binding input: tech-debt registry rows **sub-5, sub-6** (`docs/tech-debt/submission.md`), **rev-5, rev-6, rev-12** (`review.md`), **prod-5, prod-8, prod-13** (`production.md`). Scope ratified by owner 2026-07-11 (full scope incl. sweep-found siblings + consumer compensation).

## Context

The write-side aggregates expose bypass routes around their own guards: public `Stage`/`Status`/`File` setters beside guarded mutators (sub-5, rev-5, sub-6, prod-13, rev-12), the Asset transition guard living in the API layer instead of the domain (prod-5), and the two stage-handoff consumers migrating file bytes without the compensating delete their sibling endpoints demonstrate (rev-6, prod-8). `Review.Domain/Articles/Article.cs:16` (`Stage { get; private set; }`) is the repo's own correct instance — this pass makes the siblings match it. Grounded blast-radius facts (plan-time greps, 2026-07-11): the only `.Stage =` assignment outside domain internals repo-wide is ArticleHub's read model (by-design, consumers-5); zero `Adapt<Article>`/`Adapt<Asset>` calls target write-side domains; the `ReviewInvitation` creation initializer does not set `Status`. Every setter tightening fails at compile time if a caller was missed — no silent-break path exists.

## Scope

**In:** Submission.Domain (Article.Stage, Asset.File) · Review.Domain (ReviewInvitation.Status, Asset.File) · Production.Domain (Article.Stage, Asset transition guard) · Review + Production stage-handoff consumers (compensation).
**Out / DO NOT TOUCH:** `ArticleHub.Domain/Entities/Article.cs:9` `Stage { get; set; }` — sanctioned anemic read model (reference-model consumers-5, by-design); `Production.Domain/Assets/AssetCurrentFileLink.cs:8` `File { get; set; }` — link row, not an aggregate invariant; `Production.Domain/Articles/StageHistory.cs:7`; the `IFormFile File` members on UploadFileCommand DTOs (request contracts, not domain).
**Binding surfaces:** none of this is wire-visible — setter accessibility only; getters stay public, so DTO mapping and JSON serialization are unaffected; EF Core sets private setters natively. No migrations.

## Skill Mapping

| Step | Skill | Disposition | TDD | Feature-Specific Inputs | Gap? |
|------|-------|-------------|-----|------------------------|------|
| 1 | (none) | — | no | Submission Article.Stage + Asset.File accessibility | |
| 2 | (none) | — | no | Review ReviewInvitation.Status + Asset.File accessibility | |
| 3 | (none) | — | no | Production Article.Stage accessibility | |
| 4 | nexus-dotnet:add-state-machine | Follow | no | Asset aggregate, AssetStateMachineFactory, guard-in-behavior phase only (machine + table already exist) | |
| 5 | file-storage-patterns | Follow | no | Review ArticleApprovedForReviewConsumer.CreateAssets | |
| 6 | file-storage-patterns | Follow | no | Production ArticleAcceptedForProductionConsumer.CreateAssets | |
| 7 | (none) | — | no | verification sweep (greps + build + evidence re-runs) | |

TDD is `no` throughout — owner decision 2026-07-11: no standing test estate; nets are mined on demand (`mine-verify-cover`), see Testing Strategy.

## Domain Model Changes

Accessibility tightenings only (no new types, no schema change): `Article.Stage` → `private set` (Submission, Production) · `ReviewInvitation.Status` → `private set` · `Asset.File` → `private set` (Submission, Review) · `Asset.SetState` gains the validate-before-mutate guard (Production).

## Data Model Changes

None. No migrations.

## Implementation Steps

**Step 1 — Submission: close the setters.** `Satisfies: td sub-5, td sub-6.`
Files: `src/Services/Submission/Submission.Domain/Entities/Article.cs` (`:15` Stage), `src/Services/Submission/Submission.Domain/Entities/Asset.cs` (`:15` File) → `private set`. Sanctioned mutation paths stay as-is: `Behaviors/Article.cs SetStage` and the asset file-creation behavior — both partial-class members of the same types, unaffected by the accessibility change. Pattern exemplar: `src/Services/Review/Review.Domain/Articles/Article.cs:16`.
Accept: `grep -n "Stage { get" …/Entities/Article.cs` → `private set`; same for `File { get` in `Asset.cs`; `dotnet build src/Articles.sln` exit 0.

**Step 2 — Review: close the setters.** `Satisfies: td rev-5, td rev-12.`
Files: `src/Services/Review/Review.Domain/Invitations/ReviewInvitation.cs` (`:27` Status), `src/Services/Review/Review.Domain/Assets/Asset.cs` (`:21` File) → `private set`. Grounded: the only `new ReviewInvitation` site (`Review.Domain/Articles/Behaviors/Article.cs:130`) does not set Status (default `= Open`); `Accept()/Decline()` are partial-class members of ReviewInvitation itself.
Accept: definition-line greps show `private set` on both; build green.

**Step 3 — Production: close Article.Stage.** `Satisfies: td prod-13.`
File: `src/Services/Production/Production.Domain/Articles/Article.cs` (`:17`) → `private set`. Grounded (critic-corrected): the **sole** `Stage` writer is `Behaviors/Article.cs:42` inside SetStage; the `FromReview` factory (`:14-23`) does not assign Stage — it defaults to `None` and transitions via SetStage (in-code comment at `:28`). Domain-internal either way; compile-safe.
Accept: definition-line grep shows `private set`; build green.

**Step 4 — Production: move the Asset transition guard into the domain.** `Satisfies: td prod-5.` **Follow nexus-dotnet:add-state-machine** (validate-before-mutate guard phase; the AssetStateMachine, factory, and seeded transition table already exist).
Feature inputs: **relocate the body of `AssetBaseEndpoint.CheckAndThrowStateTransition` (`:24`) into `Asset.SetState`** (`src/Services/Production/Production.Domain/Assets/Behaviors/Asset.cs:61`) — the guard is multi-arg and factory-keyed on State: `_stateMachineFactory(asset.State).CanFire(asset.Article.Stage, asset.Type, actionType)`; adapt `asset.` → `this.` and `actionType` → `action.ActionType`. Do NOT mirror `Article.AssignTypesetter`'s single-arg guard — wrong shape for Asset (critic finding). The guard reads `this.Article`, already loaded on every path that reaches `SetState` today (no new NRE). All call sites (plan-time grep, critic-verified complete): internal `Behaviors/Asset.cs:74` (CancelRequest), `:85` (CreateAndAddFile); endpoints `ApproveDraftAssetEndpoint.cs:24-25`, `RequestAssetsEndpointBase.cs:28-29`, `CancelRequestFinalAssetsEndpoint.cs:33`, `UploadFileEndpoint.cs:31`. Once the domain guards, delete the endpoint-layer `CheckAndThrowStateTransition` calls and retire the definition at `AssetBaseEndpoint.cs:22` (single enforcement point — CLAUDE.md "no bypassing domain rules").
Grounding (critic): the consumer materialization path calls `CreateAndAddFile` with `action == null`, which takes the direct `State = Uploaded` branch (`Asset.cs:87`) and never reaches `SetState` — the new guard cannot fire on materialization; Decisions #3's premise is already the code's shape.
Wire note: response **status stays 400** (both `BadRequestException` and `DomainException` map to BadRequest — `GlobalExceptionMiddleware.cs:19,21`); only the 400 body message text changes from "Action not allowed" to the domain guard's message.
Verification sub-step (numbered, not optional): confirm the seeded `AssetStateTransition`/`Condition` rows cover the transitions the endpoint paths exercise (upload/approve/request/cancel). If a legal transition is missing, the pre-sanctioned fallback applies (Decisions #3).
Accept: `grep -rn "CheckAndThrowStateTransition" src/Services/Production --include=*.cs` → 0 hits; `grep -n "CanFire\|throw new DomainException" src/Services/Production/Production.Domain/Assets/Behaviors/Asset.cs` → both present inside SetState; build green.

**Step 5 — Review consumer: compensate the byte migration.** `Satisfies: td rev-6.` **Follow file-storage-patterns** (compensating-delete + stage-migration-in-consumer sections).
Feature inputs: `src/Services/Review/Review.Application/Features/Articles/InitializeFromSubmission/ArticleApprovedForReviewConsumer.cs`, `CreateAssets` upload loop (`:64-87`), single terminal `SaveChangesAsync` (`:49` region). Track every uploaded `FileMetadata`; on any failure before the save completes, `TryDeleteAsync` **all** files uploaded so far, then rethrow (Decisions #1). Idempotency grounding: the consumer's `EnsureNotExistsOrThrowAsync` head-guard plus the never-reached save mean MassTransit redelivery re-enters cleanly after compensation.
Accept: `grep -n "TryDeleteAsync" …/ArticleApprovedForReviewConsumer.cs` → ≥1 hit inside a catch; build green.

**Step 6 — Production consumer: compensate the byte migration.** `Satisfies: td prod-8.` **Follow file-storage-patterns** (same sections as Step 5).
Feature inputs: `src/Services/Production/Production.API/Features/Articles/InitializeFromReview/ArticleAcceptedForProductionConsumer.cs`, `CreateAssets` (`:98-119`), target store Azure Blob. Same all-uploaded-so-far compensation + rethrow; idempotency grounding: `ExistsAsync → return` head-guard (consumers-2).
Accept: `grep -n "TryDeleteAsync" …/ArticleAcceptedForProductionConsumer.cs` → ≥1 hit inside a catch; build green.

**Step 7 — Verification sweep (hard gate, all greps zero-or-exact).**
- `grep -rn "ArticleStage Stage { get; set; }" src/Services --include=*.cs` (excluding obj) → **exactly 1** hit: `ArticleHub.Domain/Entities/Article.cs` (sanctioned).
- `grep -rn "InvitationStatus Status { get; set; }" src/Services --include=*.cs` → 0.
- `grep -rn "File File { get; set; }" src/Services/Submission src/Services/Review --include=*.cs` → 0.
- `grep -rn "CheckAndThrowStateTransition" src/Services/Production --include=*.cs` → 0.
- Re-run each in-scope registry row's evidence command (sub-5, sub-6, rev-5, rev-12, prod-13, prod-5, rev-6, prod-8) — none may reproduce.
- `dotnet build src/Articles.sln` exit 0.
Report all grep outputs in implementation.md; the architect re-executes them at done-check.

## Cross-Service Changes

None. No contract, event, or gRPC change; consumer changes are internal to each consumer's body.

## Migration Notes

None required (no model shape change — accessibility only).

## Testing Strategy

No standing tests (owner decision 2026-07-11 — nets mined on demand). Post-merge recommendation per the registry's M2 composition: run `mine-verify-cover` M1 on `Production.Domain Asset` behaviors (the guard just moved — highest-value net) and optionally Submission `Article` behaviors. Manual smoke: docker-compose up + Postman flow submit→approve-for-review exercises Step 5's consumer path end-to-end.

## KB Impact

None — no `docs/kb/` entry covers these units (KB holds only research entries today).

## Decisions

| # | Decision | Why | Rejected alternative | Status |
|---|----------|-----|----------------------|--------|
| 1 | Consumer compensation deletes **all** files uploaded so far in the message, then rethrows | mid-loop failure orphans every prior upload; redelivery restarts the whole message | sibling endpoint pattern (compensate current file only) — leaves orphans in the multi-file loop | decided |
| 2 | `AssetBaseEndpoint.CheckAndThrowStateTransition` is retired once the domain guards | single enforcement point; duplicate endpoint gate invites drift | keep endpoint check as fast-fail duplicate | decided |
| 3 | If the seeded transition table lacks a legal-init transition for the materialization/upload path, initial state is assigned in the Create factory (domain-internal); `SetState` stays transitions-only | initialization is not a transition; mirrors how Article materialization sets Stage in its factory | force-add an init transition row to seed data (schema-visible change for an internal concern) | deferred (developer verifies the table first — Step 4 sub-step) |

## Open Questions

None — scope, bb-1 behavior (separate batch), and review mode resolved at the 2026-07-11 checkpoint.

## Plan Review

Code-grounded critic, 2026-07-11: **ACCEPT** — all 8 registry rows covered both directions; Step-4 caller list verified complete against fresh greps; blast-radius claims confirmed against live source; acceptance greps checked for false-pass/false-fail. 4 LOW findings, all folded above: (1) Production `Stage` writer count corrected (sole writer `:42`; the FromReview factory does not set Stage — also corrected in registry prod-13); (2) `:74` caller relabeled CancelRequest; (3) Step 4 now points at `CheckAndThrowStateTransition` as the body to relocate (multi-arg, not the single-arg AssignTypesetter shape) + strengthened acceptance grep; (4) wire note added — status 400 preserved, body message text changes. Informational, out of scope by design: `Submission Article.SubmittedOn/SubmittedById/SubmittedBy` are public `get; set;` where Review's exemplar uses `init` — a mutability smell below the registry bar, recorded here for owner awareness only.
