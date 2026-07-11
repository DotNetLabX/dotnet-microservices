# Run Report — td-run-1 (mine-verify-repo)

**Repo:** `D:\src\dotnet-microservices` · **Commit:** `96dde60` · **Dates:** metric layer 2026-07-10, mine + verify + registry 2026-07-11 · **Orchestrator:** architect (standalone) · **Purpose:** feed the evidence-cited repository assessment (`docs/architecture/assessment.md`).

## Pipeline actually run

Metric layer (deterministic, agent-executed) → area scope (top-5 by honest fallback ranking) → 5 clean-room per-area miners × 4 lenses + 1 global structure-graph pass → consolidation (+ 4 reference-model run-report leads injected as candidates, kept out of miner prompts to preserve clean-room) → fresh skeptic re-executed **all 66** evidence commands → registry write → architect-side triage (advisory).

## Tooling preflight & degraded signals (C1 §7 — every degrade named)

- lizard 1.23.0 **present** → complexity computed.
- Code Maat **absent** (no JRE) → git-only fallback used.
- **ownership signal DEGRADED:** single maintainer (3 aliases, 1 person) — column dropped.
- **coupling signal WEAK:** 24 commits; repo-wide bulk commits inflate co-occurrence — reported, not ranked on.
- **hotspot filter DID NOT FIRE:** μ=2.668, σ=0.983 → threshold 5.62 changes AND >16 changes/lifetime; max per-file revisions = 5. Ranking fell back to complexity-weighted size with revisions as tiebreaker.
- churn reported as **context only** (absolute churn is a poor predictor; 3 bulk commits touch 704/557/554 files).

## Bot filter

`git shortlog -sne --all` → 3 author aliases, all the same human; **0 bot commits excluded** of 24 analyzed.

## Scope

Scanned: **buildingblocks, review, submission, production, auth** (+ the global structure pass over all 49 csproj / 145 edges / both slns / gateway).
Skipped (cold rank): journals, articlehub, modules-articletimeline, modules-email, modules-filestorage, apigateway — with two mitigations: the gateway was covered by glb-5, and one modules lead (mod-1) was verified into `modules.md`. **Known unscanned lead:** ArticleHub's commented-out `UseSnakeCaseNamingConvention()` with in-code incompatibility note (reference-model run-3) — parked for a future run.

## Verify gate results (C3)

- **66 findings** entered the gate (62 miner + 4 injected leads. Note: bb-13/bb-14/glb-5/mod-1 are the injected leads; all 66 rows carry re-execution output; **0 verdicts dropped**).
- **CONFIRMED 59 · IMPRECISE 6 (corrected in place) · WRONG 1.** Mined→confirmed survival rate: **65/66 = 98.5%** (with 6 scope corrections).
- **The WRONG kill — prod-6** (registry-excluded per C3, logged here with the refuting output): claim was that the ArticleTimeline module's write can escape the trigger's transaction. Refuted: `AddTimelineEventHandler.cs:23-24` enlists via `UseTransaction(await _transactionProvider.GetCurrentTransaction(ct))` on the same scoped `TransactionProvider`/`DbConnection` the `TransactionalDispatchDomainEventsInterceptor` used (`interceptor:28 BeginTransactionAsync`), and Production `appsettings.json:38` sets `"UseSingleTransaction": true` — commit/rollback is shared. Residual caveat (not the finding's claim): if `UseSingleTransaction` were `false`, `GetCurrentTransaction` would begin a transaction nobody commits, silently rolling back timeline writes. Config is `true` in the shipped settings.
- **Severity recalibration:** 37 downgrades (teaching-repo context: demo-labeled config, dev-gated paths, demo-scale perf), 0 upgrades. Held: **sub-5 high** (public `Article.Stage` setter bypasses an existing guard on the flagship aggregate); auth-8, rev-5, rev-6, prod-8, glb-1, glb-5, sub-6, prod-2, prod-5, bb-1, bb-10, sub-11, rev-9, prod-11 medium.
- Notable re-execution wins: auth-1's nested-lambda shadowing settled empirically with a scratch net9.0 build (compiles; outer conjunct redundant; no behavioral defect); glb-5 sharpened — submission's cluster (4403) routes to **ArticleHub's live port**, a cross-route, not just a dead one; glb-1's Articles.sln half of the arch-reference lead **refuted** (clean).

## Registry delta (C4)

New files: `metrics.md`, `buildingblocks.md` (14 rows), `submission.md` (12), `review.md` (11), `production.md` (11 + 1 WRONG logged here), `auth.md` (11), `global.md` (5), `modules.md` (1), this report. **65 registry rows total**, all `status: verified`, provenance-stamped.

## Triage state (C5)

Architect-adjudicated 2026-07-11, advisory. Split: **accepted 44 · by-design 8 · deferred 13**. `by-design` rows cite `reference-model.md` rows (persistence-2, persistence-5, layering-5, modules-1/2) or in-code demo markers (auth-4/5/6/7). **Owner ratification 2026-07-11 (Laurentiu):** the 10 test-coverage deferrals are confirmed with a named route — test estates are mined on demand via `mine-verify-cover` (M1 create when a class needs a net, M2 across accepted refactors); the same decision excludes Testing and CI/CD from the assessment averages. Remaining owner-pending: the 3 staging deferrals (production compose exclusion, access-checker duplication policy, consumer parallelization) and ratification of the 44 accepted / 8 by-design rows.

## Budget

7 staged agents (1 metric, 5 miners + 1 global on sonnet, 1 skeptic) ≈ 647k subagent tokens marginal spend; orchestration in-session. Hotspot-first capping was moot (no hot areas); the top-5 scope cap held.

## Changelog

- 2026-07-11 — td-run-1 complete: initial registries created (65 rows), 1 WRONG kill logged, triage advisory pending owner.
