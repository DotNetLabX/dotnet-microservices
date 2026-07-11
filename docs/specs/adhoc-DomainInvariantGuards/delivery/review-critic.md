# adhoc-DomainInvariantGuards — Critic Review (persisted by team-lead)

> The critic writes no verdict file by design (ADR); the team lead persists its findings here verbatim as the durable record. Owner of fixes: architect (plan.md). This pass was requested by the owner as an independent code-grounded check of the two post-Phase-1 revised steps.

## Mode: Plan Review (code-grounded, scoped to Steps 1.2 and 4.1)
## Verdict: ACCEPT
## Agent: critic (agentId a8e1953a0885fe81b)

## Pre-commitment Predictions (expected vs found)
1. **Factory signature mismatch** (1.2) — *Not found.* Signature covers all 7 initializer members; behavior-preserving.
2. **Event/behavior drift** (1.2) — *Not found.* Single `ArticleCreated(Article, IArticleAction)` preserved; `_articles.Add` retained in `Journal.CreateArticle`.
3. **Incomplete/incorrect 10-file list** (4.1) — *Not found.* Grep matches the list exactly; DI "no change" claim verified true.
4. **Layering violation** (4.1) — *Not found.* Domain.csproj has no Application reference; all seam types Domain-visible.
5. **Return-type source-incompatibility** (4.1) — *Not found.* Only public member is `CanFire`; no caller holds the concrete type.

## Cross-reference Matrix
| Verification ask | Source evidence | Status |
|---|---|---|
| 1.2 behavior-preserving (same props, single `ArticleCreated`, `_articles.Add`) | `Journal.cs:9-23`; `ArticleCreated.cs:3` | COVERED |
| 1.2 adds no new type | `Behaviors/Article.cs` (no existing `Create`) | COVERED |
| 1.2 matches `FromSubmission`/`FromReview` idiom | `Review…/Behaviors/Article.cs:154`; `Production…/Behaviors/Article.cs:12` | COVERED |
| 1.2 call site correctly rewired | `Journal.cs:7-24` | COVERED |
| 4.1 10-file list complete & correct | `using Production.Application.StateMachines` grep = 10 hits | COVERED |
| 4.1 DI needs no change | `DependencyInjection.cs:5-6,25-29` | COVERED |
| 4.1 guard-move keeps invariant in domain | `AssetBaseEndpoint.cs:22-26` | COVERED |
| 4.1 layering holds (Domain ↛ Application) | `Production.Domain.csproj:13-20`; `IArticleStateMachine.cs` precedent | COVERED |
| 4.1 interface signature matches concrete | `AssetStateMachine.cs:53` | COVERED |
| 4.1 return-type widening source-compatible | grep: no bare `AssetStateMachine` refs in API | COVERED |

## Findings
No CRITICAL, HIGH, or MEDIUM findings. Two informational notes (do not affect the verdict; no developer action required):

### [LOW] `AssetState`/`AssetType`/`AssetActionType` file location vs namespace
`Production.Domain/Articles/Enums/Enums.cs:15` declares `enum AssetState` in namespace `Production.Domain.Assets.Enums` (folder is `Articles/Enums/`). Locating by folder could briefly mislead. **Impact: none** — the plan gives the correct namespace. **Suggestion:** the new `IAssetStateMachine.cs` should `using Production.Domain.Assets.Enums;` (namespace, not path). *(Carried to developer as an implementation note.)*

### [LOW] Production CLAUDE.md "no Application layer" is stale (not a plan defect)
`Services/Production/CLAUDE.md` says "No separate Application layer" but `Production.Application` exists. Doc drift, flagged for owner awareness; outside this pass's scope. No plan impact.

## Gap Analysis
- No unaddressed compile path. Step 1.2 reproduces every initializer member; Step 4.1 seam is source-compatible on every reference path.
- Step ordering correct: 4.1 (seam) precedes 4.2 (guard move); 1.2 precedes closing `Article.Stage` in 1.1.
- Acceptance greps sound and non-false-passing (all three Step-4.1 greps map to real current state: 1, 10, absent).
- `## Decisions` present and non-silent (5 rows).

## Open Questions
None.
