# Plugin feedback — nexus-dotnet 1.3.1

Portable feedback for the `nexus-dotnet` plugin (v1.3.1) skills, generated in a consuming
project (`dotnet-microservices`) where the plugin cache is read-only (ADR-1). Send to the
plugin owner to apply in the plugin source repo; on the owner's machine the same entries apply
there directly.

**Shared provenance (all entries):** found during clean-room skill regeneration + comparative
evaluation, 2026-07-05, repo `dotnet-microservices` @ `cd4d0b1`, verified against live source.
In every entry, "canonical" = the clean-room-regenerated skill that won the evaluation;
"shipped" = the nexus-dotnet 1.3.1 skill under review. Severities are the evaluation's own.

---

## add-integration-event — namespace trap, consumer-placement heuristic, over-strict `sealed`

- **Suggested target:** nexus-dotnet:add-integration-event → SKILL.md (consumer placement + `sealed`), workflows/EventContract.md and workflows/Consumer.md (namespace)
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #1). Live proofs: the contracts project namespace `Articles.IntegrationEvents.Contracts` ≠ its folder name `Articles.Integration.Contracts` (a commented-out wrong-namespace line in source is the trap made concrete); Production hosts a rich consumer in `.API`, not `.Application`; `sealed` appears on only 2 of 9 live consumers.
- **Lesson:**
  - **[High]** Add an explicit namespace-vs-folder warning. The integration-contracts namespace is `Articles.IntegrationEvents.Contracts` but the folder is `Articles.Integration.Contracts`; the shipped templates are namespace-silent and lead straight into the mismatch. State the correct namespace in the template and call out the divergence.
  - **[High]** Fix the consumer-placement rule. The shipped "rich consumer → `.Application`, simple consumer → `.API`" heuristic is wrong — Production's rich consumer lives in `.API`. Placement must key off the project that calls `AddMassTransitWithRabbitMQ`; the consumer goes wherever that registration lives, regardless of consumer complexity.
  - **[Low]** Soften the `sealed` mandate. `sealed` on the consumer class is a minority pattern (2 of 9), not a rule — present it as optional, not required.

---

## error-handling — missing prohibitions, broken extension recipe, wrong UnauthorizedException site

- **Suggested target:** nexus-dotnet:error-handling → SKILL.md (add prohibitions + fix the Extension-Points recipe and the UnauthorizedException "Where" row)
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #2). Live proof: `UnauthorizedException` is thrown from the Auth RefreshToken endpoint code, not from authentication middleware; the single translation point is the error middleware's `MapStatusCode` switch.
- **Lesson:**
  - **[add]** State two load-bearing prohibitions the shipped skill omits entirely: (1) no `Result<T>` — errors flow as thrown exceptions, not result objects; (2) `try/catch` only at genuine I/O boundaries, never as domain control flow.
  - **[High]** Fix the Extension-Points recipe: adding a new exception type MUST add its arm to the `MapStatusCode` switch. The shipped recipe omits this step, and a forgotten arm silently falls through to a 500 — say so explicitly as the failure mode.
  - **[Medium]** Correct the `UnauthorizedException` "Where" claim. Shipped text says "Authentication middleware"; it is actually thrown from endpoint code (the Auth RefreshToken endpoint). Middleware only translates exceptions to responses; it does not raise this one.

---

## create-aggregate — understated action-order, weakened VO ctor, nonexistent IAction type

- **Suggested target:** nexus-dotnet:create-aggregate → SKILL.md (action-parameter rule), workflows/ValueObject.md (ctor), workflows/DomainEvent.md (IAction)
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #3). Live proof: aggregate behavior methods take the action/owner parameter last, after the factory arguments; value-object constructors are `private` with `[JsonConstructor]`; no `IAction` type exists in the domain.
- **Lesson:**
  - **[High]** Sharpen the action-parameter rule (F1). Shipped text says the action is "preferred last" with no factory-relative ordering. The real rule is owner-binding: the action/owner is **always last, after the factory arguments**. Add the Submission drift callout — Submission diverges from this and is the known exception, not the template.
  - **[Medium]** Fix the value-object constructor (F2). Shipped templates use an `internal` ctor; the live pattern is a `private` ctor plus `[JsonConstructor]`. `internal` weakens the invariant by widening who can bypass the factory.
  - **[Low]** Remove the nonexistent `IAction` type from DomainEvent.md (F5). No such type exists in the codebase; the example won't compile and misleads the reader about the action abstraction.

---

## domain-patterns — same understated action-order and weakened VO ctor as create-aggregate

- **Suggested target:** nexus-dotnet:domain-patterns → the aggregate action-order guidance and the value-object constructor guidance
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #3). domain-patterns carries the same two drifts as create-aggregate F1/F2 (verified against the same live source above).
- **Lesson:**
  - **[High]** Same action-order fix as create-aggregate: action/owner is **always last, after the factory**, with the Submission drift called out as the exception. domain-patterns currently repeats the understated "preferred last".
  - **[Medium]** Same value-object constructor fix: `private` ctor + `[JsonConstructor]`, not `internal`. Keep this consistent with create-aggregate so the two skills don't disagree.

---

## create-feature — legacy Send API, provenance regression, generic Carter/auth guidance

- **Suggested target:** nexus-dotnet:create-feature → SKILL.md (Send API claim, :verb routes, authorization), workflows/Handler.md (CreatedById), workflows/EndpointCarter.md (`with {Id}`)
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #4). Live proof: services call `Send.OkAsync` (not `SendOkAsync`); the acting-user id is stamped server-side from `ArticleCommandBase`, never bound from the request; live Carter handlers mutate-then-assign rather than `record with { Id }`; write routes use the `:verb` convention; writes carry a two-layer role + resource gate.
- **Lesson:**
  - **[Critical]** Replace the legacy `SendOkAsync` with `Send.OkAsync`, and delete the false "used across all services" claim. The old fluent form is not what the services use; asserting universality entrenches the stale call.
  - **[High]** Fix Handler.md's wire-settable `CreatedById`. Binding the creator id from the request is a provenance/security regression — the acting user must be stamped server-side (via `ArticleCommandBase`), never accepted from the caller.
  - **[Medium]** Correct the Carter mutation pattern. Shipped text uses a generic `record with { Id }`; the dominant live pattern mutates the entity and assigns. Show the real pattern.
  - **[Medium]** Add the `:verb` route convention. Write endpoints follow `resource:verb` routing; the shipped skill omits it.
  - **[Medium]** Fix the authorization guidance. Putting `RequireAuthorization` at the group level on write endpoints drops the per-resource gate — writes need both the role layer and the resource layer, not a single group-level auth call.

---

## cqrs-patterns — worked snippet violates the action-last rule

- **Suggested target:** nexus-dotnet:cqrs-patterns → the command/handler example snippet
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #4, upstream note). The snippet's parameter ordering contradicts the verified owner-binding rule (action always last, after the factory) enforced by create-aggregate/domain-patterns.
- **Lesson:**
  - **[Medium]** Reorder the parameters in cqrs-patterns' own command/handler snippet so the action/owner is last, after the factory arguments. As written the example teaches the wrong order and undercuts the aggregate skills that state the rule correctly.

---

## create-service — 10-defect staleness cluster (predates generic DbContext, IMemoryCache, RowVersion, Data/Master)

- **Suggested target:** nexus-dotnet:create-service → workflows/ScaffoldPersistenceProject.md, ScaffoldInfrastructure.md, ScaffoldApiProject.md, ScaffoldCsprojFiles.md, ScaffoldFolders.md (per defect below)
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #5). Each correction below was checked against a live service.
- **Lesson:**
  - **[High]** DbContext base is non-compiling. Real base is `ApplicationDbContext<T>(options, cache)` (constructor takes the options and the cache); the shipped stub does not compile against the current base.
  - **[High]** Wrong domain namespaces. Shipped uses `Blocks.Domain`; the real namespace is `Blocks.Entities`. As written, entities won't resolve.
  - **[High]** Fictional middleware/registration methods (AP5). There are no `Use{Name}Middleware` / `Add{Name}Services` methods. Real wiring is uniform `AddApiServices` (etc.) plus an explicit `UseMiddleware` chain — name the real methods, not per-service invented ones.
  - **[High]** MediatR registration omits the three open behaviors. The real registration wires the AssignUserId, Validation, and Logging pipeline behaviors; the shipped stub leaves them out.
  - **[High]** Omits the `DispatchDomainEventsInterceptor` registration. A scaffolded service without it does not dispatch domain events.
  - **[Medium]** Wrong mapper. Shipped references AutoMapper/Mapperly; the repo uses **Mapster**.
  - **[Medium]** Wrong integration-contracts csproj filename. The project file is `Articles.IntegrationEvents.Contracts.csproj` inside the folder `Articles.Integration.Contracts` — mirror the same name/folder divergence flagged in add-integration-event.
  - **[Medium]** EF provider registered at the API level. The provider belongs in `.Persistence`, not `.API`.
  - **[Low]** Wrong launchSettings profiles. Real profiles are `https` + `Container`; the shipped set differs.
  - **[Low]** Wrong seed-data folders. Real layout is `Data/Master` and `Data/Test`, not `MasterData/`.

---

## create-domain-event-handler — variant keyed off the wrong axis

- **Suggested target:** nexus-dotnet:create-domain-event-handler → SKILL.md variant-selection step and workflows/Handler.md
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #6). Live counterexample: Production is a FastEndpoints service whose domain-event bus is MediatR (eventpub-4) — endpoint framework and event-publisher are independent there.
- **Lesson:**
  - **[High]** Select the handler variant from the registered `IDomainEventPublisher`, not from the service's endpoint framework. The shipped rule ("pick the variant from the CLAUDE.md endpoint framework") is a latent trap: Production uses FastEndpoints for endpoints but MediatR for the event bus, so the framework-keyed rule picks the wrong handler shape. The skill needs the concept that the event bus is a separate axis from the request framework.

---

## authorization-patterns — fictional identifiers, dropped role gate, mischaracterized single-layer case, dead tenant variant

- **Suggested target:** nexus-dotnet:authorization-patterns → SKILL.md (role vocabulary, policy registration, single-layer exception, tenant variant)
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #7). Live proof: `UserRoleType.Editor/Admin` and `Role.Admin` do not exist (compile error); real policy registration keeps a `RequireRole` layer; the one legitimately single-layer case is authentication-only read models, not role-only admin; the tenant variant is unused in this stack.
- **Lesson:**
  - **[High]** Remove the fictional identifiers. `UserRoleType.Editor`, `UserRoleType.Admin`, and `Role.Admin` don't exist — examples using them won't compile. Use the real closed role vocabulary.
  - **[High]** Restore the role gate in policy registration. The shipped registration drops the `RequireRole` layer, defeating the two-layer model. Registration must keep both the role gate and the resource gate.
  - **[Medium]** Fix the single-layer characterization. The real single-layer exception is an **authentication-only read model**, not a role-only admin endpoint. The shipped description inverts which case legitimately skips the resource layer.
  - **[Low]** Drop or clearly mark the tenant variant as not-applicable. It is dead code in this stack; presenting it as live guidance misleads.

---

## create-grpc-contract — false error-mapping claim, non-universal repo method, missing decision layer, misleading port note

- **Suggested target:** nexus-dotnet:create-grpc-contract → workflows/Server.md (error-mapping + repo method), SKILL.md (add when-to-use decision layer, fix port note)
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #8). Live proof: no gRPC error-mapping interceptor exists — see the `//todo` at `JournalGrpcService.cs:11`; `FindByIdOrThrowAsync` is EF-only, Redis-backed Journals use `GetByIdOrThrowAsync`; gRPC client URLs target internal Docker ports, not the 44xx host ports.
- **Lesson:**
  - **[High]** Remove the claim that middleware maps gRPC status codes. No interceptor exists — the `//todo` at `JournalGrpcService.cs:11` is the standing gap. Server.md currently asserts a mapping that isn't there.
  - **[Medium]** `FindByIdOrThrowAsync` is not universal. It's EF-only; Redis-backed services (e.g. Journals) use `GetByIdOrThrowAsync`. A server method template that hard-codes `FindByIdOrThrowAsync` won't compile for a Redis service — fork the method by persistence engine.
  - **[add]** Add a when-to-use decision layer. The skill jumps to mechanics without stating when a synchronous gRPC call is the right choice versus event-replicated local data or the event payload. Add that gate up front.
  - **[Medium]** Fix the port-conventions note. gRPC client URLs use **internal Docker ports**, not the 44xx host ports; the shipped note points readers at the wrong port set.

---

## service-registration — misplaced messaging, wrong behavior order, foreign examples, missing transactional variant

- **Suggested target:** nexus-dotnet:service-registration → SKILL.md (layer placement, behavior order, examples, interceptor variant)
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #9, B1). Live proof: for Application-less services MassTransit/MediatR register in the API layer; the live pipeline order is AssignUserId → Validation → Logging; examples in this repo are the Articles services, not eShop.
- **Lesson:**
  - **[High]** Fix the MediatR pipeline behavior order. The load-bearing order is **AssignUserId → Validation → Logging**; the shipped skill lists Validation, Logging, AssignUserId. AssignUserId must run first so downstream behaviors see the acting user.
  - **[Medium]** Correct the layer placement. For services without an `.Application` project, MassTransit and MediatR register in the **API** layer. The shipped guidance assumes an Application layer that isn't always present.
  - **[Medium]** Add the `TransactionalDispatchDomainEventsInterceptor` variant. The skill covers only the non-transactional dispatch interceptor and omits the transactional variant some services need.
  - **[Low]** Replace the foreign eShop examples with the repo's own Articles services so the guidance is grounded in this stack.

---

## central-package-management — grep can't catch nested props override; soften "one file" claim

- **Suggested target:** nexus-dotnet:central-package-management → SKILL.md (add nested-props check, soften one-file claim)
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #9, B2). Live proof: a stray `Directory.Packages.props` nested under `Review.API` overrides the root file (nearest-wins), which the shipped three-form verification grep structurally cannot detect.
- **Lesson:**
  - **[Medium]** Add a check for stray nested `Directory.Packages.props` files. MSBuild uses nearest-wins, so a nested props file silently overrides the root one — and the shipped three-form grep can't catch it (live example: `Review.API`). Add a "find all `Directory.Packages.props`, expect exactly one at `src/` root" step.
  - **[Low]** Soften the "one file, not per-project" statement. It reads as an invariant but isn't enforced and is violated in practice — frame it as the intended state plus the nested-file check that verifies it.

---

## persistence-patterns — misstated config ladder, missing spine rules, ungrounded examples

- **Suggested target:** nexus-dotnet:persistence-patterns → SKILL.md (entity-configuration ladder, repository + caching rules, examples)
- **Action:** update
- **Evidence:** clean-room regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source (verdict #10). Live proof: the real config ladder is `AuditedEntityConfiguration<T> : AuditedEntityConfiguration<T,int> : EntityConfiguration<T,TKey>`; repositories are the unit-of-work; reference data is cached type-keyed.
- **Lesson:**
  - **[Medium]** Fix the entity-configuration ladder. The real chain is `AuditedEntityConfiguration<T> : AuditedEntityConfiguration<T,int> : EntityConfiguration<T,TKey>`. The shipped skill flattens the generics and names the base wrong.
  - **[add]** Add the two missing spine rules: the **repository-as-unit-of-work** base, and the **type-keyed reference-data cache**. These are core to the repo's persistence layer and absent from the shipped skill.
  - **[Low]** Ground the examples. The SQLite and other foreign examples aren't from this stack (SQL Server / PostgreSQL / Redis.OM) — replace them with repo-grounded ones.

---
