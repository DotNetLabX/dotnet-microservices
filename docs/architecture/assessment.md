# Repository Assessment — dotnet-microservices (Articles / DotNetLabX)

**What this is.** An architect-authored, evidence-cited evaluation of this repository across the
categories that matter for what it is. Every factual claim cites a verified source: the tech-debt
registry (`docs/tech-debt/`, td-run-1 — 65 skeptic-re-executed rows, 1 WRONG kill logged), the
verified self reference model (`docs/reference-model.md`, 75 rows / 16 dimensions), the corrected
codebase inventory (`docs/architecture-reference.md` + its cross-check), the deterministic metric
layer (`docs/tech-debt/metrics.md` — lizard 1.23.0, git-only fallback, every degraded signal named),
and targeted session greps recorded in the provenance block. Grades are architect judgment over that
evidence — advisory, not a gate.

**Date:** 2026-07-11 · **Author:** architect (Claude) · **Status:** current as of commit `96dde60`.
**Grade scale:** A = exemplary for this repo's context · B = sound, minor gaps · C = workable, real
debt · D = material risk, needs a plan · F = failing its purpose. Scores anchor to grades:
A ≈ 9–10 · B ≈ 7–8 · C ≈ 5–6 · D ≈ 3–4 · F ≈ 1–2. Trend arrows only where work is verifiably in
flight. **Independence note:** `docs/reference-scorecard.md` covers adjacent ground and was
deliberately **not read** for this assessment (commissioning instruction) — grades here are
independent; reconciliation is an explicit deferred follow-up, not a silent duplicate.

**Purpose (this drives the weights).** This repo exists to be a **reference implementation**, twice
over. First: the teaching codebase for a Udemy course on DDD + Vertical Slice + CQRS, evolving a
modular monolith into microservices over an article-lifecycle domain (root `CLAUDE.md`). Second,
newer: the **grounding substrate for an AI-agent skill estate** — 13 repo-exact skills mined from
its skeptic-verified self-model, staged for the nexus-dotnet plugin (`architecture-reference.md`
§14). It is explicitly *not* a production service: one author by construction, and pedagogical
variance is deliberate — three endpoint frameworks, two domain-event buses, three idempotency
strategies are course content, not drift (reference-model endpoints-1, eventpub-1, consumers-2).
Quality here means: **the demonstrated patterns are real, correct, and copyable; the repo runs for
students; the documented model matches the code.** Production-ops concerns (CI, bus factor) are
discounted — but not zeroed, because a reference that can't run or verify itself teaches worse.
**Owner premise (2026-07-11):** the repo is complete in its *implemented* features; features not
present (an outbox, refresh-token rotation, a standing test suite) were deliberately never planned —
a reference shows the way, and grading it against production completeness would defeat its purpose.
Consequently, **unimplemented-by-plan is not a defect here; only defects in what is implemented
count against a grade.** Marked in-code demo boundaries are treated as exactly that — boundaries.

---

## Scorecard

| # | Category | Grade | Score | Weight | Trend | One-line basis |
|---|----------|-------|-------|--------|-------|----------------|
| 1 | Architecture & service boundaries | A− | 9.0 | 15% | → | 0 cycles / 0 layering violations / 0 cross-service edges over 145 verified edges; dead machinery deductions |
| 2 | Domain modeling quality | B | 7.5 | 15% | → | rich verified aggregate estate — undercut by public-setter bypasses on the flagship invariants |
| 3 | Messaging & data-consistency mechanics | B | 7.5 | 13% | → | verified event/replication/hydration patterns; uncompensated byte migration (outbox: out of planned scope) |
| 4 | Documentation & agent grounding | A− | 8.5 | 14% | ↗ | 75-row verified self-model + 13 repo-exact skills + 65-row debt registry; flow map still missing |
| 5 | Correctness risks | C+ | 6.0 | 11% | → | nothing data-corrupting confirmed; gateway cross-route, broken focused sln, swallowed init failure |
| 6 | Security & identity posture | B+ | 8.0 | 9% | → | verified two-layer gates; marked demo shortcuts = reference boundaries; one unmarked half-enforced invariant (role expiry) |
| 7 | Testing & safety nets | F | 1.5 | excluded¹ | → | zero test projects (triple-verified); nets to be mined on demand via `mine-verify-cover` (owner route) |
| 8 | Error-handling discipline | B+ | 8.0 | 7% | → | one middleware, typed hierarchy, DomainException→400 verified; RpcException falls through to 500 |
| 9 | API surface & contracts | B− | 7.0 | 7% | → | clean contract packages + `:verb` convention; the gateway (the composed surface) misroutes 3 of 4 |
| 10 | CI/CD & runability | D | 3.0 | excluded¹ | → | `.github/workflows/` exists **empty**; compose lacks production-api; net9.0 past STS end-of-life |
| 11 | Dependency & platform hygiene | B+ | 8.0 | 5% | → | CPM with zero inline versions, current packages; the nested CPM override is the one live defect (net9.0 = curriculum pin) |
| 12 | Complexity concentration | A | 9.5 | 4% | → | max CCN 10 repo-wide, zero functions ≥15 — verifiably readable, as a teaching repo must be |
| | **Unweighted mean** | B+ | **7.9** | — | | simple average of the 10 scored categories *(history: 7.6 pre-premise-regrade · 6.7 all-in)* |
| | **Purpose-weighted average** | **B+** | **7.9** | 100% | | weights below — the two averages coincide *(history: 7.6 · 6.9 all-in)* |

¹ Excluded from **both** averages by owner decision (Laurentiu, 2026-07-11): this is a reference
project for others, not a production service, and test estates are mined **on demand** via the nexus
miner (`mine-verify-cover` — M1 create when a class needs a net, M2 across accepted refactors). The
rows stay graded and visible: the facts don't vanish, they're just not the product being graded.

**Weighting rationale.** The purpose sets the weights: the *product* is demonstrated patterns, so
the pattern-bearing categories dominate — architecture (15%), domain modeling (15%), messaging
(13%), and agent grounding (14%, the repo's second product). Correctness (11%) is next: students
copy what they see, so demonstrated defects are product defects. Security (9%) matters exactly as
far as the auth code is copyable. Error handling (7%), API surface (7%), dependency hygiene (5%),
and complexity (4%) are instrumental. **Testing & safety nets and CI/CD & runability are graded but
excluded from the averages** — owner decision 2026-07-11: a reference project is consumed by
reading and selective copying, not by operating it, and the owner's standing route for verification
is on-demand mining (`mine-verify-cover`) rather than a maintained suite. **Knowledge & bus factor
is deliberately not a scored category:** a single-author course repo is single-author by
construction; the gradeable part — how well the one head is externalized — is scored inside
category 4, where this repo is unusually strong. No category from the SDK exemplar's weight set was
imported.

**The honest headline.** Under the owner's reference premise the two averages coincide at **7.9
(B+)**. The weights barely move the number because the repo is *uniformly* strong on
pattern-bearing categories, and exactly one scored category sits below B−: **correctness (C+),
which the premise deliberately does not excuse** — a gateway that routes submission traffic to the
wrong live service, a focused solution that doesn't build, and a dead failure path are defects in
*implemented* code, not missing features. Grade history, full transparency: 6.7/6.9 scored
production-style → 7.6/7.6 after excluding testing+CI (owner decision) → **7.9/7.9** after the
reference-premise regrade of messaging, security, and dependency hygiene (owner premise; correctness
intentionally not regraded). The one-line summary: **a genuinely well-architected, verifiably
documented reference whose demonstrated patterns are real — one config-shaped afternoon away from
its own bar.**

**Residual evidence gaps (path (a) was taken; these remain).** The security lens proper was out of
scope for td-run-1 (skill contract — security-adjacent facts were mined, a `/security-review` was
not run): category 6 is medium-confidence. Six cold areas (journals, articlehub, the three modules,
apigateway) got no dedicated miner — glb-5 and mod-1 partially cover them; categories 3 and 5 could
still move ±0.5 on a full pass. Churn/ownership metrics carry near-zero signal on a 24-commit
single-author history — the metric layer says so loudly and ranked by complexity-weighted size
instead (`metrics.md`, degraded-signals section).

---

## 1. Architecture & service boundaries — A− (9.0/10 · weight 15%)

**What holds (all skeptic-verified):** dependency direction is enforced at the build graph, not by
convention — zero Domain→Persistence/Application references (layering-1), zero cross-service
project references across all 22 service csproj (layering-4), and the td-run-1 global pass
independently rebuilt the full graph and confirmed **0 cycles, 0 layering violations, 0
wrong-direction edges over 49 nodes / 145 edges** (`docs/tech-debt/global.md`, clean-results
block). BuildingBlocks form a shallow single-concern graph (layering-2); the shared kernel is
applied by domain fit, not blanket (layering-3); the Application layer is a controlled per-service
skip (layering-5); module composition follows a real decision rule — Contracts+providers when
swappable, embedded slice when single-consumer (modules-1..5). High in-degree tops (Blocks.Core 17)
are the declared shared kernel, not god nodes.

**What doesn't:** dead structure ships alongside the live: the unused multitenancy machinery in
every consumer's dependency closure (bb-4), two dead gRPC helper siblings (bb-6), two zero-consumer
provider projects (glb-2 by-design, glb-3 an untested prototype), a dead entry breaking
`Submission.sln` (glb-1), and Auth.API's sanctioned-but-tense triple reference (auth-3 — its
CLAUDE.md waives the Application layer, yet `Auth.Application` still exists).

## 2. Domain modeling quality — B (7.5/10 · weight 15%)

**What holds:** the aggregate estate is real and verified — event side-channel owned by the
aggregate (domain-1), state/behavior partial split (domain-2, with its corrected scope), stage
transitions as *data* through a Stateless-backed machine validated before mutation (domain-3),
`EnumEntity` for behavior-bearing enum metadata (domain-4), value objects whose invalid states are
unconstructible (domain-5), and a genuinely contrasting anemic read model on ArticleHub
(consumers-5).

**What doesn't:** the flagship invariants are bypassable in exactly the way the course teaches
students not to do: `Article.Stage` is a public `get; set;` around the state-machine guard —
td-run-1's **only HIGH** finding (sub-5); `ReviewInvitation.Status` likewise (rev-5); `Asset.File`
is the lone public setter on its aggregate (sub-6); and Production guards Article transitions in
the domain but Asset transitions only in the API layer (prod-5). Smaller dents: a value object that
throws a framework exception before its own guard (sub-4), mixed timestamp sourcing inside one
method (sub-7), and the manual-publish split the reference model's own gate killed a false virtue
over (eventpub-3 WRONG kill: Journals/Auth publish domain events from endpoints, not via
interceptor).

## 3. Messaging & data-consistency mechanics — B (7.5/10 · weight 13%)

**What holds:** the cross-service estate is the richest verified part of the repo — flat DTO-only
integration contracts (boundaries-1), the single-purpose `PublishIntegrationEventOn*` handoff
(boundaries-2), centralized MassTransit wiring with 3-way fan-out (boundaries-5), three
*deliberately different* idempotency strategies marked pedagogical in-code (consumers-2),
replicate-eagerly + hydrate-lazily + one authoritative gRPC gate (datasharing-1..3), a pure
event-fed read model with zero gRPC reach-back (datasharing-4), and — notably — the one mined
transaction-consistency accusation was **killed WRONG at the verify gate**: the ArticleTimeline
module's writes provably enlist in the trigger's transaction (`run-report.md`, prod-6 refutation).

**What doesn't:** both stage-handoff consumers migrate asset bytes with no compensation under
at-least-once redelivery, while their own sibling endpoints demonstrate the correct `TryDeleteAsync`
pattern (rev-6, prod-8) — an inconsistency *inside implemented code*, and the category's real
deduction: the repo already teaches the right pattern three times and skips it twice.
**Out of planned scope, not graded (owner premise):** there is no outbox anywhere (`grep -rni
"outbox" src` → nothing; MassTransit's outbox APIs unused) — integration events publish after commit
with no delivery guarantee. Zero-cost recommendation: a reference marks its boundaries best in
writing — one line in the architecture notes ("no outbox demonstrated; production systems add one")
closes the copyability trap without implementing anything.

## 4. Documentation & agent grounding — A− (8.5/10 · weight 14% · ↗)

The repo's second product, and its most distinctive strength. What exists is not prose but
*verified registries*: a 75-row self reference model across 16 dimensions where every row carries a
re-executed evidence command and the gate has real teeth (1 invented virtue killed, 14 rows
scope-corrected — `reference-model.md` run reports); a corrected inventory whose stale claims were
audited line-by-line (`architecture-reference-crosscheck.md`); 13 phased, variant-aware, repo-exact
skills that beat the shipped generic skills 10/10 in comparative evaluation
(`architecture-reference.md` §14); per-service `CLAUDE.md` files (6/6 verified present); and — as
of this run — a 65-row skeptic-gated tech-debt registry plus a deterministic metric layer. Trend ↗:
the skill backlog carries researched, dated pending items (`docs/skill-backlog.md`).

**Gaps:** `docs/architecture/README.md` is a 13-line note, not an architecture doc; zero
`CLAUDE.md` under `src/Modules/*` (verified absent — a reference-model run-2 lead); the KB holds a
single research entry; and no artifact anywhere maps the **cross-service article-lifecycle flow**
(draft→submit→review→production→publish as one sequence) — the single doc genre a newcomer or agent
still has to reconstruct from source.

## 5. Correctness risks — C+ (6.0/10 · weight 11%)

Nothing data-corrupting was confirmed, and the worst accusation died at the gate (prod-6). What
remains is first-touch breakage and demo-grade defects in copyable code: the YARP gateway routes
**3 of 4 services wrong** — submission's cluster points at ArticleHub's *live* port (a silent
cross-route, not a clean failure), production and articlehub point at dead ports (glb-5);
`Submission.sln` fails to build on a missing project (glb-1); the repo's single most complex
function silently swallows retry exhaustion — its designed `Failed`-state/throw is dead code
(bb-1); a nested CPM file shadows central versions (rev-4); SendGrid's error logs can never
interpolate (mod-1); and a public-identifier typo family (`GetArticleResonse` across 9 files in two
services, `DependecyInjection.cs` ×3, `seperator`, the dead `ArticlerReviewedEvent` contract —
sub-1/2, rev-3, bb-2, bb-14). Latent, correctly deprioritized by the gate: the state-machine
cache-miss NRE is unreachable as wired (sub-10 corrected), and the redundant refresh-token
predicate compiles clean with unchanged behavior (auth-1, settled by a scratch build).

## 6. Security & identity posture — B+ (8.0/10 · weight 9% · medium confidence — security lens not run)

**What holds (verified):** a closed, range-coded role vocabulary exposed as constants (security-1);
two-layer role + resource gates through a single extension with the read-side exception recorded
(security-2); per-service resource checks answered from local replicated data (security-3); zero
`IsInRole` in business code (endpoints-5); fail-fast validated options repo-wide (infra-4); JWT
issuer + signing-key enforced with the audience skip documented as a single-audience choice
(security-5).

**What doesn't:** one **unmarked half-implementation** — `UserRole.StartDate/ExpiringDate` are
modeled and validated at creation, then never consulted at read time: expired roles keep granting
access through `GetRolesAsync` on Login/Refresh/Me (auth-8). This survives the owner premise
because it is not a missing feature: it is a modeled invariant the *implemented* code silently
doesn't enforce — a copier inherits columns that look load-bearing and aren't. Enforce it, drop the
columns, or mark it like the others. **By-design reference boundaries, not graded (owner premise):**
no refresh rotation (auth-4), configured-but-inert lockout (auth-5), demo secret + 99-year tokens
(auth-7), the `SigningCredentials` todo seam (bb-13) — every one carries an in-code marker, which is
exactly how a reference should state what it deliberately doesn't implement. The grade stops short
of A− for auth-8 plus the unrun security lens.

## 7. Testing & safety nets — F (1.5/10 · graded, excluded from averages — owner decision 2026-07-11)

Zero automated tests exist. Triple-verified: no `*Tests*.csproj` anywhere (filesystem sweep), no
xUnit/NUnit/MSTest reference (`architecture-reference.md` §12.1), re-confirmed at the verify gate
(every test-coverage row's Glob). The single test directory is a **husk** — `Review.Domain.Tests/`
holds `bin/`, `obj/`, and `StrykerOutput/` with no csproj and no sln entry (glb-4): a mutation-tested
suite existed and was deleted, leaving only the residue. Ten registry rows enumerate exactly what
runs unguarded: three data-driven state machines (rev-10, prod-11, sub-11's transitions), invitation
status+expiry (rev-9), the typesetter invariant (prod-12), the asset ceiling (sub-12), token
composition (auth-10/11), shared guards (bb-11), and `Entity.GetHashCode`'s persist-time hash
mutation (bb-10). The only nets are manual: a Postman collection and dev seed data (§12.2–12.3).
The F grade stands as a fact; the category is excluded from the averages because the owner's
standing route is **on-demand mining**: when a class needs a net — before an accepted refactor, or
when a registry row warrants it — `mine-verify-cover` runs M1 (mine rules → verify → generate a
mutation-gated suite) against that class, rather than the repo maintaining a standing suite. The
ten owner-ratified deferred rows in the registry are the ready-made target list for that route.

## 8. Error-handling discipline — B+ (8.0/10 · weight 7%)

The strongest conventional category: one typed self-describing exception hierarchy (errors-1), a
domain exception that doesn't know HTTP exists (errors-2), exactly one exception→status translation
point wired in 6/6 services — with `DomainException → 400` verified in the switch
(`GlobalExceptionMiddleware.cs:19-23`, session grep) — declarative guards (errors-4), try/catch
confined to 18 true I/O boundary sites (errors-5), and a verified zero-error-monad decision
(appcqrs-3). Deductions: `RpcException` has no mapping, so a down dependency surfaces as a raw 500
(sub-8); the dead tenant repository throws the raw base `HttpException` from persistence (bb-5,
corrected: the deviation is raw-base-vs-typed-guard); and two log statements that never interpolate
(mod-1).

## 9. API surface & contracts — B− (7.0/10 · weight 7%)

**What holds:** one code-first gRPC contracts package organized by owning service (boundaries-4),
flat primitive-only integration DTOs (boundaries-1), the REST-resource + `:verb` RPC-suffix route
convention applied identically across all three endpoint frameworks (namingx-4), and camelCase JSON
kept at framework default everywhere (namingx-3). **What doesn't:** the composed public surface —
the gateway — misroutes 3 of 4 services (glb-5); a misspelled type sits in a public response
contract (`GetArticleResonse`, sub-2) and a dead misspelled record sits in the integration
contracts package (bb-14); `Submission.sln` still declares a vanished client project (glb-1).

## 10. CI/CD & runability — D (3.0/10 · graded, excluded from averages — owner decision 2026-07-11)

No CI has ever run: `.github/workflows/` exists and is **empty** (verified — the scaffold is there,
nothing in it), so nothing builds, much less tests, on push. The docker-compose topology is genuinely
good teaching infrastructure — six backing services with healthchecks and volumes
(`architecture-reference.md` §13.1) — but `production-api` is commented out of it (prod-7), the
gateway misroutes what does run (glb-5), the course's focused solution fails to build (glb-1), and
all 49 projects target `net9.0`, an STS release out of support since 2026-05-12 (session grep;
Microsoft lifecycle). Startup migrations and seed data are real positives. The distance to C is
small and config-shaped — see the three moves.

## 11. Dependency & platform hygiene — B+ (8.0/10 · weight 5%)

Central Package Management with zero inline `Version=` attributes across 49 projects
(conventions-1), a current mainstream package set (EF Core 9.0.8, MediatR 13.0.0, FastEndpoints
7.0.1, Carter 9.0.0 — session grep of `Directory.Packages.props`), and uniform
`Nullable`+`ImplicitUsings` enable. **The one live defect is the nested
`Review.API/Directory.Packages.props`** (rev-4): it breaks the single-source guarantee CPM exists to
demonstrate, in the repo teaching CPM. Deleting that one file (folding its two Roslyn pins into the
root) is essentially the whole fix and takes this category to A−; the only residue after it is the
untested MinIO prototype registered in the solution (glb-3, one low row). **Not graded (owner
premise):** the `net9.0` target is past STS end-of-life (2026-05-12) but is treated as the
curriculum's pinned version, not negligence — if that pin is *not* deliberate, this line becomes a
deduction again.

## 12. Complexity concentration — A (9.5/10 · weight 4%)

The metric layer is unambiguous (`metrics.md`): maximum CCN **10** repo-wide — exactly one function
at or above 10 — **zero** functions at CCN ≥ 15, and the largest area (buildingblocks) totals 1,566
NLOC across 81 function-bearing files. A teaching repo must be readable above all; this one
verifiably is. The one footnote writes itself: the single most complex function in the repository is
also the one whose failure path is dead code (bb-1) — complexity and defects concentrate in the
same place even at CCN 10.

---

## The three moves that change the grades

1. **Close the invariant-bypass holes the course itself teaches against.** Private setters on
   `Article.Stage` (sub-5), `ReviewInvitation.Status` (rev-5), `Asset.File` (sub-6); move the Asset
   transition guard into the domain beside its Article sibling (prod-5). Four small diffs; Domain
   modeling B→A− at 15% weight — the single largest scored delta on the board — and the flagship
   lesson stops contradicting itself. *(Confidence: high — the guards already exist; the fix is
   closing the side door.)*
2. **Fix the run path — config-only, an afternoon.** Correct the three gateway cluster ports
   (glb-5), delete the dead `Submission.sln` entry (glb-1), the `Review.Domain.Tests` husk (glb-4),
   and the nested `Review.API/Directory.Packages.props` (rev-4), and restore `production-api` to
   docker-compose or document its exclusion (prod-7). Correctness C+→B− (11%) and dependency
   hygiene B+→A− (5%); the CI/runability improvement is unscored but is the first thing every
   student who clones the repo hits. *(Confidence: high — every item is a verified config/solution
   defect with a one-line fix.)*
3. **Run the owner's mining route against the ratified target list.** The ten owner-ratified
   deferred rows are a ready-made queue for `mine-verify-cover`: M1 (mine → verify → mutation-gated
   suite) on the highest-value classes first — the three state machines (sub-11/rev-10/prod-11),
   `ReviewInvitation` (rev-9), and `Entity` equality (bb-10) — and M2 across any accepted refactor
   from the registry's 44 accepted rows. This turns the excluded F from a standing liability into
   the demonstrated on-demand workflow the estate is built around; the Stryker residue (glb-4)
   shows the toolchain already ran here once. *(Confidence: high on the route — it is the owner's
   stated decision; medium on effort per class.)*

---

*Provenance: td-run-1 — metric layer 2026-07-10 (24 commits, 0 bot-filtered, lizard 1.23.0, Code
Maat absent → git-only fallback, strict hotspot filter did not fire), mine + skeptic verify +
registry 2026-07-11 at commit `96dde60` (66 findings → 59 CONFIRMED / 6 IMPRECISE-corrected / 1
WRONG, 0 verdicts dropped; `docs/tech-debt/run-report.md`) · self reference model runs 1–3 @
`cd4d0b1`, verified 2026-07-05 (75 rows; `docs/reference-model.md`) · architecture inventory +
cross-check 2026-07-05 · session greps 2026-07-11: `GlobalExceptionMiddleware` status switch, outbox
absence (`grep -rni outbox src` → 0), TFM/nullable sweep, skills + CLAUDE.md inventory, package
versions. `docs/reference-scorecard.md` deliberately not read (commissioning instruction) —
reconciliation deferred · owner decision 2026-07-11 (Laurentiu): testing & CI/CD graded but excluded
from both averages (reference project; nets mined on demand via `mine-verify-cover`) — averages
re-baselined to the 10 scored categories, prior both-in values retained in the scorecard footnote
· owner premise refinement 2026-07-11 (Laurentiu): complete-in-implemented-features — messaging,
security, and dependency hygiene regraded under it (7.6→7.9); correctness deliberately **not**
regraded (its rows are defects in implemented code).
Grades are architect judgment over verified evidence; advisory. Regenerate after any of the three
moves land or on material drift.*
