# Reference Scorecard — dotnet-microservices

**Date:** 2026-07-07 · **Baseline:** commit `e7b3ef9` · **Author:** architecture assessment (code-grounded)

## Purpose and method

This repository's goal is to be a **reference** — a worked model of how to build a .NET
microservices system (DDD + Clean Architecture + Vertical Slice + CQRS, event-driven integration) —
and the quarry from which the `nexus-dotnet` skill estate is mined. This scorecard tells a reader
what to copy with confidence, what is deliberately out of scope, and what is known debt, so the
reference can be consumed critically rather than blindly.

Scores are grounded in code-verified evidence, not impressions:

- `docs/reference-model.md` — a 75-row pattern registry where every claim was re-executed against
  live source by an adversarial verifier (invented virtues killed before counting).
- Ten comparative skill evaluations (Phase B, 2026-07-05) + ~45 source-verified defect findings
  routed to and fixed in the `nexus-dotnet` plugin (1.4.0).
- Three independent code-grounded review passes over the mining and consolidation plans.
- External research (`docs/kb/research/dotnet-agent-skill-estates.md`) comparing this estate to the
  strongest public .NET agent-skill estates.

**Honesty note:** the system has been read and mined exhaustively but not executed under load, and
it ships zero tests by design — these are code-reading scores, not runtime scores.

## A. Codebase scorecard

Read with the dual lens: this is a **teaching reference**, and several "gaps" are deliberate scope
choices, marked as such below.

| Category | Score | Evidence |
|---|---|---|
| Domain modeling (DDD) | **9/10** | Aggregate state/behavior partial split; value-object factories with guard clauses; domain events raised only via `AddDomainEvent` inside the aggregate; the state machine is data-driven (legal transitions as cached seed data, never a hardcoded switch) with a validate-before-mutate guard. Docked one point: Submission's four `Article` behaviors drift from the owner-binding action-parameter rule (known, recorded). |
| Architecture & boundaries | **8/10** | Clean layering and vertical slices coexist coherently (layering is structural, slicing organizational); domain events (in-process) vs integration events (cross-service) strictly split; synchronous gRPC used only where a gate must be authoritative at a state transition — otherwise event-replicated local reads. The sync-vs-async decision rule is one of the repo's best exports. |
| CQRS / application layer | **8/10** | MediatR pipeline order **AssignUserId → Validation → Logging** is load-bearing and consistently wired; the acting user is stamped server-side (`[JsonIgnore]` on `ArticleCommandBase.CreatedById`), never bound from the wire — a provenance/security pattern worth copying verbatim; `ICommand`/`IQuery` markers carry no behavior branching. |
| Messaging & consumers | **8/10** | Three deliberate consumer idempotency variants (skip-if-exists / throw-if-exists / exists-check-return) each matched to a use case; consumers auto-discovered; independent-refresh reference data via shadow-row event pairs; cross-stage file bytes physically migrated. No transactional outbox — fine at course scope, the first addition for production use. |
| Authorization & security | **8/10** | Two-layer endpoint gate (role + per-service resource check against local `ArticleActors`); closed `UserRoleType` enum range-partitioned by domain, surfaced as `Role` string constants; the sanctioned single-layer exception is authentication-only read models (ArticleHub). JWT enforces issuer + signing key and deliberately skips audience; no secrets-management or rate-limiting story (scope). |
| Persistence | **8/10** | Two-level entity-configuration ladder (`AuditedEntityConfiguration<T> : AuditedEntityConfiguration<T,int> : EntityConfiguration<T,TKey>`); repository-as-unit-of-work base; type-keyed reference-data cache with startup prewarm; JSON seeding from `Data/Master` + `Data/Test`; opt-in RowVersion. |
| Consistency & hygiene | **6/10** | Real debt, all recorded: a stray nested `Directory.Packages.props` under `Review.API` silently overrides the root (MSBuild nearest-wins); Dockerfiles pin `sdk:8.0` on a net9.0 stack; misspelled `DependecyInjection.cs` filenames; the `Articles.IntegrationEvents.Contracts` namespace ≠ `Articles.Integration.Contracts` folder trap; a `//todo` where the gRPC error-mapping interceptor should be. Variant diversity across services (FastEndpoints / Carter / Minimal APIs; SQL Server / PostgreSQL / Redis.OM; MediatR / FastEndpoints event buses) is **deliberate pedagogy**, not inconsistency — each fork is documented as a variant. |
| Testing | **2/10** | Zero test projects — a **deliberate course-scope choice**, but the largest single gap as engineering practice; it is why the pattern registry needed adversarial re-verification instead of a test run. Unblock path exists: `mine-verify-cover-dotnet` (Mine→Verify→Cover with a Stryker mutation gate) on a rule-rich aggregate. |
| Observability | **4/10** | Correlation-id chain (header → `Activity` → `TraceIdentifier`) + console logging; no metrics/tracing stack (deliberate scope; recorded as a deferred gap). |
| Knowledge capture / docs | **9/10** | Rare and exemplary: per-service CLAUDE.md files, a skeptic-verified 75-row pattern registry, an architecture reference, a research KB, decision records. Most production repos never reach this level; for a reference repo it is the load-bearing asset. |

**Verdict: as a reference model ~9/10; as a production system ~7/10.** The delta is exactly tests,
outbox, observability, and the hygiene list — all *known and recorded*, which is itself part of what
makes the reference trustworthy: the gaps are labeled, not hidden.

## B. Skill-estate scorecard

The patterns above are operationalized as agent skills in the `nexus-dotnet` plugin. The honest way
to score the estate is before and after the mining arc, because the arc is the story: clean-room
skills were regenerated from the verified registry, evaluated 10/10 wins against the shipped 1.3.1
set, and the ~45 verified defects were fixed upstream (1.4.0); the four mined skills without shipped
counterparts are being ported (see `adhoc-SkillEstateConsolidation` in the plugin repo), after which
the local copies retire.

| Category | Mined/ported set (13) | Shipped @1.3.1 | Shipped @1.4.0 |
|---|---|---|---|
| Factual accuracy | **9** — every claim skeptic-verified against live source; zero errors found in evaluation | **3** — ~45 verified defects: fictional identifiers (`UserRoleType.Editor`), a legacy Send API asserted as universal, a wire-settable `CreatedById` (security regression), non-compiling scaffolds | **9** — all corrected; on the value-object constructor census it now *leads* the mined set |
| Decision guidance | **9** — decision-rule-first (gRPC's when-NOT-to-use gate, idempotency decision tables, explicit variant axes) | **5** — jumps to mechanics; one skill keyed its variant off the wrong axis entirely | **8** — decision layers added, axis corrected |
| Verifiability | **8** — grep-shaped verify sections per skill | **5** — estate lint existed, but in-skill checks thin (the CPM verification grep structurally could not catch its own live counterexample) | **7** — checks fixed; executable `scripts/` remain the open frontier |
| Coverage | **7** — 13 deep, backend-only | **8** — 33 skills incl. frontend (Vue/Pinia/Tailwind), refactoring, meta-skills, a mutation-testing adapter | **8 → 9** once the four ports land (state machine, file storage, consumers, infra conventions) |
| Lifecycle & maintainability | **4** — no versioning; measurably drifted behind the plugin within one day | **9** — version-keyed releases, CHANGELOGs, CI backstop, 484 lint tests, twin sync | **9** |

**Verdict:** the mined set won on **content** (clean-room, source-verified — that was its job); the
plugin wins on **system** (release discipline, tests, reach). Consolidation merges the winning column
of each; the projected post-consolidation estate (~9/10) is the strongest artifact of the whole
program: verified content inside a versioned pipeline. Against the public state of the art, the
external research found **no** public .NET estate (awesome-copilot, dotnet/skills, morphir-dotnet)
doing phased, variant-aware, source-verified skills at all; the one practice worth importing back is
morphir-style executable verification scripts (tracked as the deferred `verification-scripts` gap).

## C. What would move the needle

1. **Testing beachhead** — run `mine-verify-cover-dotnet` on one rule-rich aggregate (e.g. the
   Submission `Article`) to establish the first mutation-gated suite; unblocks the deferred
   testing-strategy skill with lived material.
2. **Finish the estate consolidation** — port the four gap skills, retire the locals, keep one
   versioned estate in the plugin (in flight: `adhoc-SkillEstateConsolidation`).
3. **Hygiene sweep** (small solo pass) — remove the nested `Directory.Packages.props` stray, align
   Dockerfile TFMs, fix the misspelled filenames — cheap polish for a repo whose job is to be read.
4. **Production-hardening appendix** (only if the reference should also teach production posture) —
   transactional outbox, OpenTelemetry, CI: each currently a labeled, deliberate omission.

## Provenance

`docs/reference-model.md` (verified registry) · `docs/architecture-reference.md` (inventory + design
notes) · `docs/skill-evals/` (evaluation records incl. the Phase B verdicts) ·
`docs/plugin-feedback/` (routed, now-applied defect lists) ·
`docs/kb/research/dotnet-agent-skill-estates.md` (external estate research) · plugin repo:
`docs/specs/adhoc-DotnetFeedbackApply/` and `docs/specs/adhoc-SkillEstateConsolidation/`.
