# Cross-check: architecture-reference.md vs reference-model.md

**Inputs:** `docs/architecture-reference.md` (descriptive scan, generated 2026-03-22, unverified) vs `docs/reference-model.md` (skeptic-verified registry, 75 rows / 16 dimensions, verified 2026-07-05 @ `cd4d0b1`) + current code.
**Method:** section-by-section; every STALE flag re-verified against the repo with an executed command; CONTRADICTED claims cite registry row ids.

**Headline:** the scan is broadly accurate on structure, messaging, security, and naming. Genuinely broken: two §5.2 code listings (pre-RowVersion, pre-generic-ladder), one wrong factual claim (Auth `UserCreated` "not raised" — it is raised), and the `Behaviours/`/`Behavior/` folder spellings throughout. Several absolutes are demoted by registry corrections.

---

## Section findings

### §1 Repository Structure
- **VERIFIED:** BuildingBlocks inventory, module split, two solution files, nested `Review.API/Directory.Packages.props`, layer directions + Auth/Journals/Production exceptions (layering-1..5, modules-1/5, conventions-1).
- **CONTRADICTED:** §1.6 single-central-CPM-file framing → conventions-1 (two `Directory.Packages.props` files exist: root + Review.API).
- **STALE:** tree shows `Submission.Domain/Behaviours/` — folder is `Behaviors/` (verified by `ls`).
- **UNVERIFIED-BY-REGISTRY:** §1.5 per-project dependency inventory (spot-check passed: Journals.Persistence → Blocks.Redis + JournalDbContext).

### §2 Architecture Patterns
- **VERIFIED:** vertical-slice layout, `ArticleCommandBase` `[JsonIgnore]` audit fields (appcqrs-1), pipeline order (appcqrs-2), ICommand/IQuery markers (appcqrs-5), aggregate event side-channel (domain-1), state/behavior split (domain-2).
- **STALE (factual error):** §2.5 "Auth `UserCreated` defined and not currently raised" — it IS raised: `CreateUserEndpoint.cs:43 await PublishAsync(new UserCreated(user, resetPasswordToken))`, handled by `SendConfirmationEmailOnUserCreatedHandler`. Only the aggregate-level `AddDomainEvent` is commented out (`User.cs:22`). The scan's own §4.1 listing shows the PublishAsync — internal self-contradiction.
- **STALE:** `Behaviours/` paths.

### §3 Domain Models per Service
- **VERIFIED (registry-adjacent):** ArticleHub read-model bag (consumers-5); Journals Redis entity, no AggregateRoot (persistence-4).
- **STALE:** repeats the wrong `UserCreated` claim (§3.1); `Behaviours/` (§3.3) and `Behavior/` singular (§3.5) — actual folders are `Behaviors/`.
- **Spot-checks passed:** `TypesetterAlreadyAssignedException`, `ReviewInvitation` location, `ArticleStage` enum values exact (enum now additionally carries static `HashSet<ArticleStage>` sets — additive, not contradictory).

### §4 Endpoint Patterns
- **VERIFIED:** per-service framework choice (endpoints-1), handler-shape fork on MediatR (endpoints-2), no god-folders (endpoints-3), co-located validators (endpoints-4), declarative auth + zero IsInRole (endpoints-5), two-layer `RequireRoleAuthorization` (security-2 — ArticleHub's bare `.RequireAuthorization()` is the recorded read-side exception).
- **Current:** §4.1 listings already use post-migration `Send.OkAsync` (zero legacy `Send*Async` in src/Services). No staleness.

### §5 Persistence Patterns
- **VERIFIED:** GetAllCached + UseEntityTypeNamesAsTables (persistence-5, namingx-2), interceptor split (persistence-3), DB-per-role (persistence-4), repository-is-UoW / no IUnitOfWork (persistence-1).
- **STALE (load-bearing):** §5.2 `AuditedEntityConfiguration<T>` listing predates commit `793056c`. Actual: two-level generic ladder (`AuditedEntityConfiguration<T> : AuditedEntityConfiguration<T,int>` → `: EntityConfiguration<T,TKey>`), `protected virtual string DefaultDateSql`, and the opt-in `HasConcurrencyToken` → `RowVersion` block (lines 34-35). Copying the scan's listing yields the pre-RowVersion shape. Registry row: persistence-2.
- **STALE (load-bearing):** §5.2 `EntityConfiguration<TEntity>` listing — actual is the two-level `EntityConfiguration<T>` → `EntityConfiguration<T,TKey>` ladder.
- **OMISSION:** Journals persistence narrative never mentions `JournalDbContext` (exists since Initial Commit; wraps `RedisConnectionProvider`, exposes `IRedisCollection<Journal>/<Editor>`) — conventions-3, persistence-4.

### §6 gRPC
- **VERIFIED:** code-first contracts (boundaries-4), centralized `AddCodeFirstGrpcClient<T>` (datasharing-5), lazy hydration + authoritative gate (datasharing-1/3).
- **Spot-checks passed:** 3 `.proto` files exist (incl. `Submission/ArticleRequest.proto`, absent from the scan tree); `Submission.sln` dead reference to `UserServiceGrpcClient.csproj` confirmed.

### §7 Messaging — **VERIFIED throughout** (boundaries-1/2/5, consumers-2, eventpub-1). No staleness.

### §8 BuildingBlocks — **VERIFIED** (layering-2, errors-1/2/3). MapStatusCode switch accurate. No staleness.

### §9 Mapping — **UNVERIFIED-BY-REGISTRY.** Consistent with registry-incidental `Adapt<T>()` evidence; no contradiction found.

### §10 Naming Conventions
- **CONTRADICTED:** §10.3 primary-ctor captures as flat `_camelCase` rule → conventions-5 (captures unsettled: `dbContext` vs `_dbContext` between sibling repos of one service).
- **CONTRADICTED:** §10.1 universal `{Service}DbContext` → conventions-3 (`JournalDbContext` singular; generic BuildingBlocks bases).

### §11 Service-Specific Patterns
- **VERIFIED / spot-checked:** Auth ports 4401/4451; Production commented out in docker-compose; Production no-Application + transactional interceptor + ArticleTimeline (modules-4, persistence-3, eventpub-4); Journals Redis (persistence-4).
- **STALE:** `Behavior/` singular paths.
- **UNVERIFIED-BY-REGISTRY:** full port table, per-service prose (spot-checks passed).

### §12 Testing — **spot-checked TRUE:** zero test projects; `docs/gherkin/Review/AssignEditor.feature` is 0 bytes.

### §13 Configuration & Infrastructure
- **VERIFIED:** §13.3 JWT settings exactly match `ConfigureAuthentication.cs` (security-5).
- **UNVERIFIED-BY-REGISTRY:** docker topology (§13.1) and the §13.2 gateway-route/port-drift claim — flag for a targeted check if the gateway matters.

---

## Recommendation

**Retire the scan as a standalone reference; registry + per-service CLAUDE.md become the source of truth.**

1. **Regenerate / correct before any reuse:** §5.2 both entity-configuration listings (stale post-`793056c`); §2.5/§3.1 `UserCreated` claim (wrong); global replace `Behaviours/` and `Behavior/` → `Behaviors/`; add `JournalDbContext` to the Journals persistence narrative.
2. **Keep (verified current):** §6 gRPC, §7 messaging, §8 BuildingBlocks, §4 endpoints/auth, §13.3 JWT, §12 testing, §3.8 enums, spot-checked port/docker facts.
3. **Drop the demoted absolutes:** §10.3 ctor-capture rule, §10.1 universal DbContext naming, §1.6 single-CPM-file framing — adopt the registry's corrected wording (conventions-1/3/5).
4. **Target state:** fold the still-accurate broad inventories (domain models, ports, docker, mappings) into per-service `CLAUDE.md` files maintained beside the code; `reference-model.md` remains the verified "what to copy" registry. Maintaining the 2400-line scan in parallel is a net liability now that the registry exists.

---
*Produced 2026-07-05 by the mine-reference-model cross-check stage (commit `cd4d0b1`). Companion to `docs/reference-model.md`.*
