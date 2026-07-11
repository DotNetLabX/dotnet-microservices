# adhoc-DomainInvariantGuards

**Feature Spec:** None — ad-hoc refactoring pass. Binding input: tech-debt registry rows **sub-5, sub-6** (`docs/tech-debt/submission.md`), **rev-5, rev-6, rev-12** (`review.md`), **prod-5, prod-8, prod-13** (`production.md`). Scope ratified by owner 2026-07-11 (full scope incl. sweep-found siblings + consumer compensation).

## Context

The write-side aggregates expose bypass routes around their own guards: public `Stage`/`Status`/`File` setters beside guarded mutators (sub-5, rev-5, sub-6, prod-13, rev-12), the Asset transition guard living in the API layer instead of the domain (prod-5), and the two stage-handoff consumers migrating file bytes without the compensating delete their sibling endpoints demonstrate (rev-6, prod-8). `Review.Domain/Articles/Article.cs:16` (`Stage { get; private set; }`) is the repo's own correct instance — this pass makes the siblings match it. Grounded blast-radius facts (plan-time greps, 2026-07-11; Stage claim corrected post-Phase-1): repo-wide, `Stage` is written outside its owning aggregate in exactly two places — ArticleHub's read model (external, by-design, consumers-5, DO NOT TOUCH) and Submission's `Journal.CreateArticle` object-initializer (`Behaviors/Journal.cs:15`), which sets the initial `Created` stage from a *sibling* class and therefore breaks CS0272 once `Article.Stage` is `private set`; Step 1.2 resolves it via a same-class `Article.Create` factory (Q1). A dot-anchored grep (`.Stage =`) missed the initializer write — the correct sweep is `\bStage\s*=`. Zero `Adapt<Article>`/`Adapt<Asset>` calls target write-side domains; the `ReviewInvitation` creation initializer does not set `Status`. Every setter tightening fails at compile time if a caller was missed — no silent-break path exists.

## Scope

**In:** Submission.Domain (Article.Stage, Asset.File) · Review.Domain (ReviewInvitation.Status, Asset.File) · Production.Domain (Article.Stage, Asset transition guard) · Review + Production stage-handoff consumers (compensation).
**Out / DO NOT TOUCH:** `ArticleHub.Domain/Entities/Article.cs:9` `Stage { get; set; }` — sanctioned anemic read model (reference-model consumers-5, by-design); `Production.Domain/Assets/AssetCurrentFileLink.cs:8` `File { get; set; }` — link row, not an aggregate invariant; `Production.Domain/Articles/StageHistory.cs:7`; the `IFormFile File` members on UploadFileCommand DTOs (request contracts, not domain).
**Binding surfaces:** none of this is wire-visible — setter accessibility only; getters stay public, so DTO mapping and JSON serialization are unaffected; EF Core sets private setters natively. No migrations.

## Skill Mapping

| Step | Skill | Disposition | TDD | Feature-Specific Inputs | Gap? |
|------|-------|-------------|-----|------------------------|------|
| 1 | (none) | — | no | Submission Article.Stage + Asset.File accessibility; extract same-class `Article.Create` factory for initial-stage assignment (Q1) | |
| 2 | (none) | — | no | Review ReviewInvitation.Status + Asset.File accessibility | |
| 3 | (none) | — | no | Production Article.Stage accessibility | |
| 4 | nexus-dotnet:add-state-machine | Follow | no | Asset aggregate: add Domain seam (Phase 2 — `IAssetStateMachine` + relocate `AssetStateMachineFactory` delegate) then move guard into `Asset.SetState` (Phase 4); machine class + seed table already exist (Phases 1&3) | |
| 5 | file-storage-patterns | Follow | no | Review ArticleApprovedForReviewConsumer.CreateAssets | |
| 6 | file-storage-patterns | Follow | no | Production ArticleAcceptedForProductionConsumer.CreateAssets | |
| 7 | (none) | — | no | verification sweep (greps + build + evidence re-runs) | |

TDD is `no` throughout — owner decision 2026-07-11: no standing test estate; nets are mined on demand (`mine-verify-cover`), see Testing Strategy.

## Domain Model Changes

Accessibility tightenings, plus two structural seams needed to keep the guards inside the domain. **"No new types" is scoped to the domain *model* — aggregates, entities, value objects (Article/Asset/ReviewInvitation): none are added, no schema change.** Two non-model seams are the sanctioned exceptions, each the only feasible mechanism for an already-ratified row (owner-confirmed 2026-07-11):
- Accessibility: `Article.Stage` → `private set` (Submission, Production) · `ReviewInvitation.Status` → `private set` · `Asset.File` → `private set` (Submission, Review).
- **Seam Q1 (Submission):** a same-class static factory `Article.Create(...)` on `Article`'s partial class — a method on an existing type (not a new type); relocates the initial-stage assignment inside `Article` so `private set` holds. Mirrors Review `Article.FromSubmission` / Production `Article.FromReview`.
- **Seam Q2 (Production, prod-5):** a new Domain interface `IAssetStateMachine` + the `AssetStateMachineFactory` delegate relocated into `Production.Domain/StateMachines/` — the state-machine seam that lets `Asset.SetState` (Domain) hold the guard without a Domain→Application dependency inversion. Mirrors the existing `IArticleStateMachine` seam one folder over. This is the sanctioned carve-out from "no new types": a state-machine seam, not a domain-model type.
- `Asset.SetState` gains the validate-before-mutate guard (Production).

## Data Model Changes

None. No migrations.

## Implementation Steps

**Step 1 — Submission: close the setters.** `Satisfies: td sub-5, td sub-6.`
1.1 — Files: `src/Services/Submission/Submission.Domain/Entities/Article.cs` (`:15` Stage), `src/Services/Submission/Submission.Domain/Entities/Asset.cs` (`:15` File) → `private set`. `Behaviors/Article.cs SetStage` and the asset file-creation behavior are partial-class members of the same types — unaffected. Pattern exemplar for the `private set` shape: `src/Services/Review/Review.Domain/Articles/Article.cs:16`.
1.2 — **Compile-gap fix (Q1).** `Journal.CreateArticle` (`Submission.Domain/Behaviors/Journal.cs:9-24`) sets `Stage = ArticleStage.Created` in an object initializer from `Journal` — a *sibling* class — which fails CS0272 once Stage is `private set` (`private set` is scoped to the declaring class, not the namespace). Add a same-class static factory `Article.Create(string title, ArticleType type, string scope, Journal journal, IArticleAction action)` to `Submission.Domain/Behaviors/Article.cs` (a method on the existing `Article` partial class — no new type) that constructs the article — including the `Stage = ArticleStage.Created` initializer, now legal *inside* `Article` — and raises the existing `ArticleCreated` domain event; `Journal.CreateArticle` calls `Article.Create(...)` and keeps `_articles.Add(article)`. **Behavior-preserving:** identical property set, the same single `ArticleCreated` event, no creation-time `ArticleStageChanged`, no state-machine, no new seed row. Do NOT use an `InitializeStage` setter-seam (bespoke — CLAUDE.md "no inventing patterns") and do NOT route creation through `SetStage` (would add a `None→Created` seed row + a creation-time `ArticleStageChanged` — beyond this pass; see Decisions #4). Pattern exemplars (same-class creation factory): Review `Article.FromSubmission` (`Review.Domain/Articles/Behaviors/Article.cs:154`), Production `Article.FromReview` (`Production.Domain/Articles/Behaviors/Article.cs:12`).
Accept: `grep -n "Stage { get" …/Entities/Article.cs` → `private set`; same for `File { get` in `Asset.cs`; `grep -n "static Article Create" …/Submission.Domain/Behaviors/Article.cs` → present; `grep -rn "Stage = ArticleStage" src/Services/Submission --include=*.cs` → matches only inside `Article`'s own factory, not `Journal.cs`; `dotnet build src/Articles.sln` exit 0.

**Step 2 — Review: close the setters.** `Satisfies: td rev-5, td rev-12.`
Files: `src/Services/Review/Review.Domain/Invitations/ReviewInvitation.cs` (`:27` Status), `src/Services/Review/Review.Domain/Assets/Asset.cs` (`:21` File) → `private set`. Grounded: the only `new ReviewInvitation` site (`Review.Domain/Articles/Behaviors/Article.cs:130`) does not set Status (default `= Open`); `Accept()/Decline()` are partial-class members of ReviewInvitation itself.
Accept: definition-line greps show `private set` on both; build green.

**Step 3 — Production: close Article.Stage.** `Satisfies: td prod-13.`
File: `src/Services/Production/Production.Domain/Articles/Article.cs` (`:17`) → `private set`. Grounded (critic-corrected): the **sole** `Stage` writer is `Behaviors/Article.cs:42` inside SetStage; the `FromReview` factory (`:14-23`) does not assign Stage — it defaults to `None` and transitions via SetStage (in-code comment at `:28`). Domain-internal either way; compile-safe.
Accept: definition-line grep shows `private set`; build green.

**Step 4 — Production: move the Asset transition guard into the domain.** `Satisfies: td prod-5.` **Follow nexus-dotnet:add-state-machine** (Phase 2 Domain seam + Phase 4 validate-before-mutate guard; the `AssetStateMachine` class and seeded transition table already exist — Phases 1&3).

4.1 — **Domain seam (Phase 2 — Q2, owner-confirmed).** Create `src/Services/Production/Production.Domain/StateMachines/IAssetStateMachine.cs` mirroring the sibling `IArticleStateMachine.cs`: `interface IAssetStateMachine { bool CanFire(ArticleStage articleStage, AssetType assetType, AssetActionType actionType); }` plus `public delegate IAssetStateMachine AssetStateMachineFactory(AssetState assetState);` (all param/return types are Domain-visible — `ArticleStage` from Abstractions, the rest from `Production.Domain.Assets.Enums`). In `Production.Application/StateMachines/AssetStateMachine.cs`: make the class `: IAssetStateMachine` (its existing `CanFire` already matches the signature), add `using Production.Domain.StateMachines;`, and **delete** the delegate declaration at `:10` (it moves to Domain; its return type becomes the interface, not the concrete class — consumers only call `CanFire`, so this is source-compatible). Repoint the delegate's namespace in the **10 API files** that inject it (grep-verified against a `AssetStateMachineFactory` name-grep; none reference the concrete `AssetStateMachine`, so swap `using Production.Application.StateMachines;` → `using Production.Domain.StateMachines;`, do not add): `Features/_Shared/AssetBaseEndpoint.cs`, `Features/Assets/RequestAssets/_Shared/RequestAssetsEndpointBase.cs`, `Features/Assets/UploadFiles/_Shared/UploadFileEndpoint.cs`, `Features/Assets/ApproveAssets/Draft/ApproveDraftAssetEndpoint.cs`, `Features/Assets/UploadFiles/UploadSupplementaryFile/UploadSupplementaryFileEndpoint.cs`, `Features/Assets/RequestAssets/Final/RequestFinalAssetsEndpoint.cs`, `Features/Assets/RequestAssets/Supplementary/RequestSupplementaryAssetsEndpoint.cs`, `Features/Assets/RequestAssets/CancelRequest/CancelRequestFinalAssetsEndpoint.cs`, `Features/Assets/UploadFiles/UploadFinalFile/UploadFinalFileEndpoint.cs`, `Features/Assets/UploadFiles/UploadDraftFile/UploadDraftFileEndpoint.cs`. `Production.Application/DependencyInjection.cs` needs **no** using change — it already imports both namespaces (`:5-6`) and registers `AssetStateMachineFactory` the same way it registers `ArticleStateMachineFactory` (`:18-29`).

4.2 — **Move the guard (Phase 4).** Relocate the body of `AssetBaseEndpoint.CheckAndThrowStateTransition` (`:22-26`) into `Asset.SetState` (`Production.Domain/Assets/Behaviors/Asset.cs:61`): add an `AssetStateMachineFactory stateMachineFactory` parameter and the guard `stateMachineFactory(this.State).CanFire(this.Article.Stage, this.Type, action.ActionType)` → `throw new DomainException(...)` on false. Multi-arg, factory-keyed on State — do NOT mirror `Article.AssignTypesetter`'s single-arg guard (critic finding). Thread the factory through the internal callers `CancelRequest` (`:69`) and `CreateAndAddFile` (`:77`, the `action != null` branch only). Throw `DomainException`, not `BadRequestException`. `this.Article` is loaded on every path reaching `SetState` today (no new NRE). Materialization is exempt: the consumer path calls `CreateAndAddFile` with `action == null`, which takes the direct `State = Uploaded` branch (`:87`) and never reaches `SetState` — the guard cannot fire on materialization (Decisions #3; initialization ≠ transition).
Wire note: response **status stays 400** (both `BadRequestException` and `DomainException` map to BadRequest — `GlobalExceptionMiddleware.cs:19,21`); only the 400 body message text changes from "Action not allowed" to the domain guard's message.

4.3 — **Retire the endpoint check (grep-before-delete, hard sub-step).** Once the domain guards, delete every endpoint-layer `CheckAndThrowStateTransition` call and the definition at `AssetBaseEndpoint.cs:22` (single enforcement point — CLAUDE.md "no bypassing domain rules"). Zero remaining references required (acceptance grep below).

4.4 — **Verify the seed table (numbered, not optional).** Confirm the seeded `AssetStateTransition`/`Condition` rows cover the transitions the endpoint paths exercise (upload/approve/request/cancel). If a legal transition is missing, the pre-sanctioned fallback applies (Decisions #3).

Accept:
- `grep -n "interface IAssetStateMachine" …/Production.Domain/StateMachines/IAssetStateMachine.cs` → present; `grep -n "delegate IAssetStateMachine AssetStateMachineFactory" …/IAssetStateMachine.cs` → present.
- `grep -rn "delegate.*AssetStateMachineFactory" src/Services/Production/Production.Application --include=*.cs` → **0 hits** (delegate no longer in Application); `grep -n "AssetStateMachine : IAssetStateMachine" …/Production.Application/StateMachines/AssetStateMachine.cs` → present.
- `grep -rn "using Production.Application.StateMachines" src/Services/Production/Production.API --include=*.cs` → **0 hits** (all 10 API files swapped to Domain).
- `grep -rn "CheckAndThrowStateTransition" src/Services/Production --include=*.cs` → **0 hits**.
- `grep -n "CanFire\|throw new DomainException" …/Production.Domain/Assets/Behaviors/Asset.cs` → both present inside `SetState`.
- `dotnet build src/Articles.sln` exit 0.

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
| 3 | If the seeded transition table lacks a legal-init transition for the materialization/upload path, initial state is assigned in the Create factory (domain-internal); `SetState` stays transitions-only | initialization is not a transition; mirrors how Article materialization sets Stage in its factory | force-add an init transition row to seed data (schema-visible change for an internal concern) | deferred (developer verifies the table first — Step 4.4) |
| 4 | Q1: resolve the `Journal.CreateArticle` compile-gap with a same-class `Article.Create` factory (Step 1.2) | initialization ≠ transition (Decisions #3); mirrors Review/Production creation factories; behavior-preserving; no new type | (a) `InitializeStage` setter-seam — bespoke, invents a pattern; (b) route creation through `SetStage` — adds a `None→Created` seed row + a creation-time `ArticleStageChanged`, beyond an accessibility pass | decided (architect, within ratified scope) |
| 5 | Q2: add Domain `IAssetStateMachine` + relocate the `AssetStateMachineFactory` delegate to satisfy prod-5 (Step 4.1) | Domain cannot reference Application (Clean Arch), so `Asset.SetState` cannot hold the guard without a Domain seam; mirrors the existing `IArticleStateMachine`; only feasible path | pass a pre-computed `bool` from the endpoint — recreates the bypass prod-5 exists to close | decided (owner-confirmed 2026-07-11; "no new types" scoped to the domain model) |

## Open Questions

None — scope, bb-1 behavior (separate batch), and review mode resolved at the 2026-07-11 checkpoint. Two Phase-1 compile-gaps (Q1 `Journal.CreateArticle`, Q2 Asset Domain seam) resolved 2026-07-11 — Q1 by architect within ratified scope, Q2 owner-confirmed; see Decisions #4–#5 and `questions.md`.

## Plan Review

Code-grounded critic, 2026-07-11: **ACCEPT** — all 8 registry rows covered both directions; Step-4 caller list verified complete against fresh greps; blast-radius claims confirmed against live source; acceptance greps checked for false-pass/false-fail. 4 LOW findings, all folded above: (1) Production `Stage` writer count corrected (sole writer `:42`; the FromReview factory does not set Stage — also corrected in registry prod-13); (2) `:74` caller relabeled CancelRequest; (3) Step 4 now points at `CheckAndThrowStateTransition` as the body to relocate (multi-arg, not the single-arg AssignTypesetter shape) + strengthened acceptance grep; (4) wire note added — status 400 preserved, body message text changes. Informational, out of scope by design: `Submission Article.SubmittedOn/SubmittedById/SubmittedBy` are public `get; set;` where Review's exemplar uses `init` — a mutability smell below the registry bar, recorded here for owner awareness only.

**Post-Phase-1 revision, 2026-07-11.** Developer Phase-1 analysis surfaced two build-breaking gaps the plan-time grounding missed, both resolved and re-grounded against live source: (Q1) a dot-anchored `.Stage =` grep missed `Journal.CreateArticle`'s object-initializer write from a *sibling* class — Step 1 gains the same-class `Article.Create` factory (1.2), Context note corrected; (Q2) Step 4's "machine + table already exist" premise held for the Application `AssetStateMachine` class + seed table but not the Domain interface/delegate seam (`IAssetStateMachine` never existed; `AssetStateMachineFactory` sat in Application) — Step 4 gains the Phase-2 Domain seam (4.1), and its caller list was corrected 5→10 API files against a `AssetStateMachineFactory` name-grep (DI file needs no change, already imports both namespaces). "No new types" scoped to the domain model with the two non-model seams carved out as sanctioned (Domain Model Changes; Decisions #4–#5). Owner confirmed Q2's scope-wording reading 2026-07-11. No source code written by the architect — implementation is the developer's Phase 2.
