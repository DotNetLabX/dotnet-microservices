# adhoc-DomainInvariantGuards — Pipeline Summary

**Outcome:** ✅ Complete — reviewer APPROVED, one MEDIUM finding fixed on request, build green (0 errors).
**Branch:** adhoc-TechDebtRegistryFixes (shared with a concurrent TechDebt run, per owner decision; every commit scoped to this slug's files).
**Team mode:** Standard (architect done-check → reviewer). **Entry point:** Developer (plan pre-existed).

## What was built

Tightened domain invariants across three services so state/file transitions can't be bypassed:

| Step | Service | Change | Tech-debt row |
|------|---------|--------|---------------|
| 1 | Submission | `Article.Stage` + `Asset.File` → `private set`; new same-class static factory `Article.Create(...)`; `Journal.CreateArticle` delegates to it | sub-5, sub-6 |
| 2 | Review | `ReviewInvitation.Status` + `Asset.File` → `private set` | rev-5, rev-12 |
| 3 | Production | `Article.Stage` → `private set` | prod-13 |
| 4 | Production | New `IAssetStateMachine` Domain seam (interface + relocated `AssetStateMachineFactory` delegate); 10 API `using`s swapped Application→Domain; transition guard moved into `Asset.SetState`; endpoint-layer `CheckAndThrowStateTransition` deleted (single enforcement point) | prod-5 |
| 5/6 | Review + Production consumers | Track every uploaded `FileMetadata`; wrap the full post-head-guard body through `SaveChangesAsync` in try/catch; compensate all uploads-so-far via `TryDeleteAsync` on failure, then rethrow for redelivery | rev-6, prod-8 |
| 7 | (all) | Verification sweep — all hard-gate greps pass | — |

**Files:** 22 source (21 modified + 1 created: `Production.Domain/StateMachines/IAssetStateMachine.cs`).

## Pipeline flow

1. **Developer Phase 1 (analyze)** — found 2 compile-blocking gaps, stopped at the question checkpoint (no code).
2. **Architect** — resolved both: Q1 → same-class static `Article.Create(...)` factory (no new type; rejected an `InitializeStage` seam and a `SetStage` route); Q2 → the `IAssetStateMachine` Domain seam. Patched the plan; corrected Step 4's caller list (5 → 10 API files).
3. **Owner decision (Q2)** — confirmed adding the Domain seam despite the plan's "no new types" line (scoped that line to the domain *model*; the state-machine seam is the sanctioned mechanism).
4. **Critic** — code-grounded review of the two revised steps → **ACCEPT**, 0 blocking findings.
5. **Plan commit** `3c895b1`.
6. **Developer Phase 2 (implement)** — all 7 steps, build 0 errors, all hard-gate greps pass.
7. **Architect Step-1 done-check** → **PASS** (7/7, 0 Missing, skill-conformance log-verified).
8. **Reviewer Step-2 code review** → **APPROVED**, 1 MEDIUM / 0 CRITICAL/HIGH.
9. **Owner decision (MEDIUM)** — chose to fix now.
10. **Developer post-review fix** — explicit `DomainException` guard on `Asset.CreateAndAddFile` (idiom-matched); build 0 errors, both call sites unaffected.
11. **Team-lead independent build** — 42 pre-existing warnings, 0 errors.

## Decisions
- **#4 (Q1):** initialization is not a transition → static factory, no creation-time `ArticleStageChanged`.
- **#5 (Q2):** `IAssetStateMachine` Domain seam (owner-confirmed); "no new types" scoped to the domain model.
- **#1:** consumers compensate all uploads-so-far on any post-head-guard failure, then rethrow.
- **Post-review:** `Asset.CreateAndAddFile`'s `action`/`stateMachineFactory` coupling enforced by explicit `DomainException` (XOR guard), replacing a null-forgiving `!`.

## Findings
- **[MEDIUM]** `Asset.CreateAndAddFile` guard-dependency coupled by convention only → **fixed** (explicit XOR `DomainException` guard). No live bug existed.
- No CRITICAL/HIGH at any gate.

## Deviations
None from plan. One self-disclosed boy-scout cleanup (removed a now-unused `using Blocks.Exceptions;` after deleting its only consumer) — verified genuinely unused.

## Commits
- `3c895b1` — plan approval (Q1/Q2 resolutions).
- *(implementation commit — appended below at close.)*

## Follow-ups / notes
- **Tech-debt registry:** rows sub-5/sub-6/rev-5/rev-12/prod-13/prod-5/rev-6/prod-8 are resolved by this feature, but the registry files (`docs/tech-debt/`) were **not** edited — a concurrent TechDebt pipeline owns them on this shared branch. Recommend marking those rows resolved once that run settles, to avoid a mid-stream conflict.
- **Verify gate:** the project's `SubagentStop` verify set is empty (`commands_count=0`), so gate verdicts were vacuous passes. Build correctness was confirmed by the developer, the reviewer, and an independent team-lead build.
