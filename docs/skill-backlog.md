# Skill Backlog

## Refresh Queue

### mine-reference-model refresh
- **Status:** Owed
- **Type:** Refresh
- **Source:** adhoc-MineFamilyCore P0 record hygiene (nexus); pilot ran 2026-07-05 (self-reference, runs 1-3), `last_verified: 2026-07-05`
- **Description:** Re-run `mine-reference-model` (self-reference mode) to refresh `docs/reference-model.md` against this repo's git delta since `last_verified: 2026-07-05`. Refresh owner: operator.
- **Date:** 2026-07-10

## Skills Created

### Error Handling
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows errors-1..5 + appcqrs-3 (docs/reference-model.md); clean-room build
- **Description:** Project-local `.claude/skills/error-handling/SKILL.md` — which exception to throw where (HttpException subclasses in app/endpoint code, DomainException in domain code, FluentValidation thrown by the pipeline), the binding prohibitions (no error-monad; one translation switch; guards not inline null-checks; try/catch only at I/O boundaries), the guard utilities, and the add-a-new-exception-type recipe. Lint exit 0.
- **Date:** 2026-07-05

### gRPC Communication
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows boundaries-4 + datasharing-1/-3/-4/-5 (docs/reference-model.md); clean-room build
- **Description:** Project-local `.claude/skills/grpc-communication/SKILL.md` — leads with the decision rule for when to use synchronous gRPC (write-path lazy get-or-create hydration; authoritative gate at a state transition) versus event-replicated local data (reads) versus never (a pure read model must have every field in the event payload). Then the four phases — contract (`Articles.Grpc.Contracts`, code-first), server (owning service, `MapGrpcService`), client registration (shared `AddCodeFirstGrpcClient<T>`, never a hand-rolled channel), handler usage — with template code lifted from JournalContracts / JournalGrpcService / GrpcClientRegistrationExtensions / CreateArticle + ApproveArticle handlers. Lint exit 0.
- **Date:** 2026-07-05

### create-aggregate
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows domain-1/-2/-4/-5 + persistence-4 (docs/reference-model.md); owner action-parameter-placement feedback; clean-room build
- **Description:** Project-local `.claude/skills/create-aggregate/SKILL.md` — leads with the variant decision (EF-backed `AggregateRoot` when the aggregate enforces atomic write-side invariants; Redis.OM `Entity` for CRUD-shaped reference data), then the phases per variant: state file → behavior partial → value objects → domain events → EF configuration ladder (or Redis attributes). Binding rules cite domain-1 (events raised only via `AddDomainEvent` from inside the aggregate), domain-2 (partial split + state machine as sanctioned second enforcement site), domain-4 (behavior metadata as `EnumEntity`, not attributes+reflection), domain-5 (VO private ctor + static `Create` + `Guard.ThrowIf*`), errors-2 (`DomainException`, no HTTP types in domain), and the owner rule that `action` follows the domain arguments. Seven templates lifted from live Submission + Journals source; a Verify section with grep post-conditions. Deliberately overlaps shipped `nexus-dotnet:create-aggregate` — authored clean-room as a registry-verified project-local variant per owner instruction. Lint exit 0.
- **Date:** 2026-07-05

### article-state-machine
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows domain-3 (primary) + domain-2 note (docs/reference-model.md); architecture-reference §3.8; clean-room build
- **Description:** Project-local `.claude/skills/article-state-machine/SKILL.md` — data-driven stage-transition validation for a service. Leads with the binding rule (legal transitions are cached DATA, never a hardcoded switch; validate before mutate) then the four phases: (1) `ArticleStageTransition` metadata entity + composite-key EF config + `Data/Master/*.json` seed, (2) `IArticleStateMachine` + `ArticleStateMachineFactory` delegate + `ValidateStageTransition` guard (Domain) and the Stateless wrapper (Application), (3) the scoped factory DI registration in two proven variants — `IMemoryCache`-backed (Submission/Review) and `ProductionDbContext.GetAllCached`-backed (Production), (4) the `ValidateStageTransition` call as the first line of `SetStage` in the aggregate behavior. Cites domain-3 (data not switch; validate-before-mutate), domain-2 (guard throw is the sanctioned second invariant-enforcement site outside Behaviors), persistence-5 (ICacheable whole-table cache + startup prewarm). Includes an add-a-new-transition recipe and the recorded `//todo` future direction (drop Stateless for direct DB-backed checks). Template code lifted from live Submission + Production source. Lint exit 0. Judgment gate (evaluate-skill rubric Layers 1-4): ACCEPT, no Critical/High/Medium.
- **Date:** 2026-07-05

### add-integration-event
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows boundaries-1/-2/-5 + eventpub-5 (docs/reference-model.md); clean-room build
- **Description:** Project-local `.claude/skills/add-integration-event/SKILL.md` — propagating a business event across service boundaries via MassTransit. Three ordered phases: contract record + flat DTOs in `Articles.IntegrationEvents.Contracts` (boundaries-1, never a domain entity), a `PublishIntegrationEventOn{DomainEvent}Handler` that re-fetches the aggregate, `Adapt`s to the DTO, and `IPublishEndpoint.Publish`es — the only sanctioned domain-to-integration handoff (boundaries-2), named by the domain event and colocated in its feature folder (eventpub-5) — and auto-discovered consumers in the receiving services (boundaries-5, no manual registration). Publisher variant table for MediatR (`INotificationHandler`/`Handle`, `.Application`) vs FastEndpoints (`IEventHandler`/`HandleAsync`, `.API`); rich + simple consumer templates showing pub/sub fan-out. Documents the `Articles.Integration.Contracts` folder vs `Articles.IntegrationEvents.Contracts` namespace gotcha and the "place the consumer in the project that calls `AddMassTransitWithRabbitMQ(Assembly.GetExecutingAssembly())`" placement rule (verified across all 5 messaging services). Templates lifted from live Submission/Journals publishers, Review/Journals consumers, and `Blocks.Messaging`. Verify section with grep post-conditions. Deliberately overlaps shipped `nexus-dotnet:add-integration-event` — authored clean-room as a registry-verified project-local variant per team-lead instruction. Lint exit 0; judgment layer applied inline (evaluate-skill not spawned, to avoid its no-overlap layer reading the forbidden sibling).
- **Date:** 2026-07-05

### service-infra-conventions
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows infra-1/-2/-3/-4/-5 + appcqrs-2 + namingx-3 + conventions-1/-2/-5 (docs/reference-model.md); clean-room build
- **Description:** Project-local `.claude/skills/service-infra-conventions/SKILL.md` — cross-cutting infrastructure wiring and repo-wide conventions. Leads with the two through-lines (inner layers depend on a narrow capability not the transport; required config fails at startup) then a cheat-sheet table, then ten sections: the segregated `IClaimsProvider`/`IRouteProvider` bridge with `HttpContextProvider` as the sole `HttpContext` toucher (infra-1); the scoped `RequestContext` POCO populated once by middleware with the header/`Activity`/`TraceIdentifier` correlation-id chain (infra-2); options binding only via `AddAndValidateOptions`/`GetSectionByTypeName` with fail-fast `ValidateOnStart` (infra-4); default-interface-method derivations on `IAuditableAction` (infra-3); the fixed MediatR behavior order AssignUserId→Validation→Logging (appcqrs-2); HTTP JSON casing left at the camelCase default with only `PropertyNameCaseInsensitive` set (namingx-3); CPM with zero inline `Version=` and the nested-props smell called out as debt (conventions-1); per-project `GlobalUsings.cs` with category headers (conventions-2); `_camelCase` fields — picks the declared-field convention and recommends underscore ctor captures, noting the current inconsistency (conventions-5); and framework-split validator vocabulary, no shared cross-framework helpers (infra-5). Template code lifted from live `HttpContextProvider`, `RequestContext`/`RequestContextMiddleware`, `ConfigurationExtensions`, `IAuditableAction`, Submission/Review Application DI, the JsonOptions sites, `Directory.Packages.props`, `GlobalUsings.cs`, and Production `_Shared/Validators.cs`. Every cited file:line verified by read. Lint exit 0; judgment layer (proven-patterns AP1-AP7 + Quality Gate) applied inline — evaluate-skill not spawned, to keep the clean-room constraint (its no-overlap layer reads sibling plugin skills) and per the team-lead method scoping the done-condition to the skill-lint gate.
- **Date:** 2026-07-05

### authorization-security
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows security-1..security-5 (docs/reference-model.md); clean-room build
- **Description:** Project-local `.claude/skills/authorization-security/SKILL.md` — the article-lifecycle authorization model in five phases. Role vocabulary: closed `UserRoleType : int` enum range-partitioned by domain (1-9 cross / 11-19 submission / 21-29 review / 31-39 production / 91-99 auth), surfaced as `Role` string constants via `nameof`; add-a-role recipe = enum member in-range + constant + gate usage (security-1). Endpoint gate: the two-layer `RequireRoleAuthorization` extension (role gate + `ArticleRoleRequirement`), with the sanctioned single-layer `.RequireAuthorization()` for ArticleHub read-only aggregate views (security-2). Resource check: per-service `IArticleAccessChecker.HasAccessAsync` against local `ArticleActors` (never a synchronous cross-service query), same decision tree with a service-specific admin bypass (Submission `Overlaps([EOF, REVED])` / Review `Contains(EOF)` / Production `Contains(POF)`), wired via `ArticleAccessAuthorizationHandler` (security-3). Identity stamping: framework variant table + code for MediatR `AssignUserIdBehavior`, Minimal-API `AssignUserIdFilter`, FastEndpoints `AssignUserIdPreProcessor`, all hooking shared `IClaimsProvider`, with the honest presence-guard difference (`TryGetUserId` skip vs `GetUserId` throw) noted (security-4). JWT: `AddJwtAuthentication` enforcing issuer + signing key and deliberately skipping audience, `RoleClaimType = ClaimTypes.Role` (security-5). All code lifted from live source; row ids cited inline; house style matches sibling pattern skills (inline code, no templates/ folder). Clean-room per team-lead — did not read the shipped `nexus-dotnet:authorization-patterns` skill. Lint exit 0 (no warnings); judgment layer (proven-patterns AP1-AP7 + Quality Gate) applied inline — evaluate-skill not spawned, to keep the clean-room constraint and per the team-lead method scoping the done-condition to the skill-lint gate.
- **Date:** 2026-07-05

### Domain Event Wiring
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows eventpub-1, eventpub-2, eventpub-4, eventpub-5, persistence-3 (and the corrected/retired eventpub-3) (docs/reference-model.md); clean-room build
- **Description:** Project-local `.claude/skills/domain-event-wiring/SKILL.md` — wiring in-process domain-event dispatch for a service and writing the handlers that react to those events. Leads with the dual-typed `IDomainEvent : INotification, IEvent` model (eventpub-2, dispatch library is a DI choice not a modeling one) and a per-service resolution table, then three decisions: (1) publisher implementation — MediatR (`Blocks.MediatR.DomainEventPublisher`, Submission/Review/Production) vs FastEndpoints (`Blocks.FastEndpoints.DomainEventPublisher`, Auth/Journals), picked by the service's event bus, with the eventpub-4 rule that a FastEndpoints service may register MediatR purely as the bus (Production); (2) dispatch trigger — EF `SaveChangesInterceptor` harvest for write-side EF services vs manual endpoint `PublishAsync` for Redis-backed Journals and Auth's Identity flow, encoding the corrected eventpub-3 reality that manual publish is legitimate (the killed absolute was "never dispatched manually"); (3) interceptor flavor — post-save `DispatchDomainEventsInterceptor` (Submission/Review) vs transactional `TransactionalDispatchDomainEventsInterceptor` (Production, with its `TransactionProvider` + `TransactionOptions` extra deps) per persistence-3. Handler section: `{Effect}On{Event}Handler`, feature-folder colocated, assembly-scanned, `INotificationHandler`/`Handle` vs `IEventHandler`/`HandleAsync` by publisher, with the ArticleTimeline generic-base exception (eventpub-5). Scope fence points cross-service propagation to the integration-event skill (boundaries-2). Template code lifted from live `IDomainEvent`, both `DomainEventPublisher`s, both interceptors, `DbContextExtensions.DomainEvents`, five DI registrations, the Journals/Auth manual-publish endpoints, and the Production/Auth effect handlers. Every cited claim verified by read (incl. Production referencing both Blocks.MediatR + Blocks.FastEndpoints). Lint exit 0 (no warnings); judgment gate (evaluate-skill rubric Layers 0-4) run in-context — verdict ACCEPT, no Critical/High findings. Clean-room: no domain-event plugin skill read.
- **Date:** 2026-07-05

### consumer-patterns
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows consumers-1..5 + datasharing-2 (docs/reference-model.md); real consumer source (Review, Production, ArticleHub, Submission Journal pair); clean-room build
- **Description:** Project-local `.claude/skills/consumer-patterns/SKILL.md` — writing MassTransit integration-event consumers. Leads with the five-phase shape: consumer class + concrete primary-ctor persistence deps (concrete `DbContext`/`Repository<T>`/`{X}Repository`, interfaces only for non-persistence collaborators — consumers-1) → idempotency → local reference-data hydration (`GetOrCreate{X}`, Adapt or hand-construct — consumers-3; independent-refresh data via the `JournalCreated`/`JournalUpdated` shadow-row pair — datasharing-2) → domain mutation (aggregate factory) or projection write (plain data bag, zero behavior — consumers-5; cross-stage file bytes physically migrated — consumers-4) → single save. Centerpiece is the three-variant idempotency decision table with an explicit rule: silent skip-if-exists (reference-data upserts), throw-if-exists (`EnsureNotExistsOrThrowAsync`/`AnyAsync`→throw, article creation where a dup is a bug), ExistsAsync→silent return (Production) — plus the load-or-throw invert for update-existing projections (consumers-2). Three `references/` templates lifted from live source: write-side-consumer, projection-consumer (create-new + update-existing), reference-data-consumer (skip + upsert pair). Clean-room per team-lead — did not read the shipped `nexus-dotnet:add-integration-event` or any plugin skill. Lint exit 0 (no warnings); judgment gate (evaluate-skill rubric Layers 0-4) run inline: no Critical/High, F1 (AP3 recap duplication) and F2 (AP5 forward-reference to not-yet-existent file-storage-patterns) folded as a consolidating pass, F3 (lessons-capture line) waived for a light pattern-reference skill.
- **Date:** 2026-07-05

### file-storage-patterns
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows filestorage-1/-2/-3/-4 + modules-3 + consumers-4 (docs/reference-model.md); clean-room build
- **Description:** Project-local `.claude/skills/file-storage-patterns/SKILL.md` — composing the pluggable FileService module in a service. Leads with the one rule (the generic options-marker type `IFileService<TFileStorageOptions>` is the DI key, never keyed DI — filestorage-1), then five phases: (1) choose provider per service, registered as the non-generic `IFileService` singleton (each service owns its storage — consumers-4); (2) subclass the provider options into an empty marker per extra store, resolved as the closed generic (modules-3); (3) register singleton-default + `AddMongoFileStorageAsScoped<TOptions>` per extra; (4) upload with the compensating `TryDeleteAsync` in try/catch — file write is outside the SQL transaction (filestorage-3); (5) cross-stage byte migration in consumers — `DownloadAsync` from the previous stage's store, `UploadAsync` into the own (consumers-4). Variant decision table (filestorage-2): enum-keyed `FileServiceFactory` when the store is chosen at runtime (Review) vs direct multi-param injection when both stores are always needed (Production). Binding rules cite filestorage-1/-4 + consumers-4. `references/templates.md` carries the lifted code (contract, options subclass, Mongo/Azure registration, both upload handlers, factory + registration, direct-injection consumer). Verify section with grep post-conditions. Templates lifted from live `FileService.Contracts/IFileService.cs`, Review `FileStorage/*` + `FileServiceFactory` + `FileServiceFactoryRegistration`, Submission/Review `_Shared/UploadFileCommandHandler`, Production `_Shared/UploadFileEndpoint` + `ArticleAcceptedForProductionConsumer`. One deliberate deviation from verbatim lift: the factory's own-store arm is keyed to the non-generic `IFileService` (resolvable) rather than the shipped `IFileService<MongoGridFsFileStorageOptions>` (not registered in Review), flagged inline in SKILL + templates. Lint exit 0; judgment layer (proven-patterns AP1-AP7 + Quality Gate) applied inline — evaluate-skill not spawned, to keep the clean-room constraint (its no-overlap layer reads sibling plugin skills) and per the team-lead method scoping the done-condition to the skill-lint gate.
- **Date:** 2026-07-05

### create-feature-slice
- **Status:** Created
- **Type:** Gap
- **Source:** Reference-model rows endpoints-1/-2/-3/-4/-5 + appcqrs-1/-2/-5 + namingx-4 + security-2 (docs/reference-model.md); architecture-reference §4 + §11; clean-room build
- **Description:** Project-local `.claude/skills/create-feature-slice/SKILL.md` — the workhorse skill for adding a complete vertical-slice feature (endpoint, command/query record, co-located validator, handler-or-in-endpoint logic, route, authorization gate). Leads with Step 0 — read the target service's CLAUDE.md `Endpoint framework` line (endpoints-1) — which selects one of three variants, each with a full copy-ready template: (A) FastEndpoints without MediatR, logic in the endpoint `HandleAsync`, no handler file (endpoints-2), everything colocated in `.API/Features/` (endpoints-3); (B) Carter + MediatR, `ICarterModule` shim dispatching via `ISender`, command/validator/handler in `.Application/Features/`; (C) Minimal APIs + MediatR, static `Map` extension via `ISender`, plus the one manual `EndpointRegistration.MapAllEndpoints()` registration line. Six framework-agnostic phases in SKILL.md cite the rules; the three `templates/*.md` carry the code. Binding rules: feature-is-a-folder never a god-folder (endpoints-3); article provenance `[JsonIgnore]`+pipeline-computed via the `ArticleCommand`/`ArticleCommandBase` base, never a wire field (appcqrs-1); `ICommand`/`IQuery` markers with no behavior branching on them (appcqrs-5); the `AssignUserId→Validation→Logging` pipeline already registered per service — note it, don't re-register (appcqrs-2); validator base type follows the framework — `AbstractValidator`/`Validator`/`BaseValidator` (endpoints-4); declarative auth only — two-layer `RequireRoleAuthorization`/`[Authorize(Roles=...)]` for article writes, bare `.RequireAuthorization()`/`[Authorize]` for read-only aggregate endpoints (endpoints-5, security-2); `:verb` RPC suffix for non-CRUD routes (namingx-4). Template code lifted from live Submission (CreateArticle/ApproveArticle/GetArticle), Review (AcceptArticle/GetArticle), Journals (CreateJournal/GetJournal), Production (AssignTypesetter + `_Shared` BaseEndpoint/Validators) source, plus `Articles.Abstractions/ArticleCommandBase.cs` and `Articles.Security/Extensions.cs`. Every cited path/type verified by read; two templates trimmed of unused injected deps and one unverified generic-repository method during the inline judgment pass. Deliberately overlaps shipped `nexus-dotnet:create-feature` — authored clean-room as a registry-verified project-local variant per team-lead instruction. Lint exit 0 (no warnings); judgment layer (proven-patterns AP1-AP7 + Quality Gate) applied inline — evaluate-skill not spawned, matching the sibling clean-room decision (its no-overlap layer would read the forbidden `nexus-dotnet:create-feature`) and the team-lead method scoping the done-condition to the skill-lint gate.
- **Date:** 2026-07-05

## Skills Fixed

## Routed to Plugin

All 13 entries below route to `docs/plugin-feedback/nexus-dotnet-1.3.1-2026-07-05.md` — the portable feedback file for the shipped `nexus-dotnet` 1.3.1 skills. Found during clean-room skill regeneration + comparative evaluation, 2026-07-05, repo dotnet-microservices @ cd4d0b1, verified against live source.

### add-integration-event
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #1)
- **Description:** Routed 3 defects — missing namespace-vs-folder warning (`Articles.IntegrationEvents.Contracts` ≠ folder `Articles.Integration.Contracts`); wrong consumer-placement heuristic (rich→.Application/simple→.API refuted by Production's rich consumer in .API — must key off the `AddMassTransitWithRabbitMQ` call-site); over-strict `sealed` mandate (2-of-9 minority, make optional).
- **Date:** 2026-07-05

### error-handling
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #2)
- **Description:** Routed 3 defects — missing prohibitions (no `Result<T>`; try/catch only at I/O boundaries); broken Extension-Points recipe (must require the `MapStatusCode` switch arm — forgotten arm silently falls through to 500); wrong `UnauthorizedException` "Where" (thrown from Auth RefreshToken endpoint code, not authentication middleware).
- **Date:** 2026-07-05

### create-aggregate
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #3)
- **Description:** Routed 3 defects — understated action-order (F1: action/owner is always last, after the factory, with the Submission drift callout); weakened value-object ctor (F2: `private` + `[JsonConstructor]`, not `internal`); nonexistent `IAction` type in DomainEvent.md (F5).
- **Date:** 2026-07-05

### domain-patterns
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #3)
- **Description:** Routed 2 defects — carries the same F1 (understated action-order) and F2 (value-object ctor `internal` vs `private` + `[JsonConstructor]`) drifts as create-aggregate; kept a separate entry since it is a distinct skill file upstream.
- **Date:** 2026-07-05

### create-feature
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #4)
- **Description:** Routed 5 defects — legacy `SendOkAsync` + false "used across all services" claim (real: `Send.OkAsync`); wire-settable `CreatedById` in Handler.md (provenance/security regression — stamp server-side via `ArticleCommandBase`); generic Carter `record with { Id }` vs real mutate-then-assign; missing `:verb` route convention; group-level `RequireAuthorization` on writes drops the per-resource gate.
- **Date:** 2026-07-05

### cqrs-patterns
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #4)
- **Description:** Routed 1 defect — the skill's own command/handler snippet violates the action-last rule (parameter order contradicts the owner-binding rule the aggregate skills state). Separate entry, distinct skill file.
- **Date:** 2026-07-05

### create-service
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #5)
- **Description:** Routed the 10-defect staleness cluster — non-compiling DbContext base (real: `ApplicationDbContext<T>(options, cache)`); wrong domain namespaces (`Blocks.Domain` → `Blocks.Entities`); fictional `Use{Name}Middleware`/`Add{Name}Services` (real: uniform `AddApiServices` + explicit `UseMiddleware` chain); MediatR reg omits the 3 open behaviors; AutoMapper/Mapperly vs real Mapster; wrong launchSettings profiles (real: https + Container); `MasterData/` vs `Data/Master` + `Data/Test`; omits `DispatchDomainEventsInterceptor` registration; wrong integration-contracts csproj filename; EF provider belongs in `.Persistence`, not `.API`.
- **Date:** 2026-07-05

### create-domain-event-handler
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #6)
- **Description:** Routed 1 defect (latent trap) — variant must key off the registered `IDomainEventPublisher`, not the service's endpoint framework. Counterexample: Production is a FastEndpoints service whose event bus is MediatR, so the framework-keyed rule picks the wrong handler shape.
- **Date:** 2026-07-05

### authorization-patterns
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #7)
- **Description:** Routed 4 defects — fictional identifiers (`UserRoleType.Editor/Admin`, `Role.Admin` — compile error); policy registration drops the `RequireRole` layer; mischaracterized single-layer case (real exception is authentication-only read models, not role-only admin); dead tenant variant presented as live.
- **Date:** 2026-07-05

### create-grpc-contract
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #8)
- **Description:** Routed 4 defects — false Server.md error-mapping claim (no interceptor exists — `//todo` at JournalGrpcService.cs:11); non-universal `FindByIdOrThrowAsync` (EF-only; Redis uses `GetByIdOrThrowAsync`); missing when-to-use decision layer; misleading port-conventions note (gRPC client URLs use internal Docker ports, not 44xx host ports).
- **Date:** 2026-07-05

### service-registration
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #9)
- **Description:** Routed 4 defects — wrong pipeline behavior order (real load-bearing order: AssignUserId → Validation → Logging); misplaced MassTransit/MediatR (register in the API layer for Application-less services); omits the `TransactionalDispatchDomainEventsInterceptor` variant; foreign eShop examples (replace with repo's Articles services).
- **Date:** 2026-07-05

### central-package-management
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #9)
- **Description:** Routed 2 defects — the three-form verification grep can't catch a stray nested `Directory.Packages.props` (nearest-wins override; live example Review.API) — add a nested-props check; soften the "one file, not per-project" claim (intended state, not an enforced invariant).
- **Date:** 2026-07-05

### persistence-patterns
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Clean-room regeneration + comparative evaluation (verdict #10)
- **Description:** Routed 3 defects — misstated entity-configuration ladder (real: `AuditedEntityConfiguration<T> : AuditedEntityConfiguration<T,int> : EntityConfiguration<T,TKey>`); missing spine rules (repository-as-unit-of-work, type-keyed reference-data cache); ungrounded SQLite/foreign examples.
- **Date:** 2026-07-05

### improve-skills (nexus core — lint + recipe)
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Utility-skill audit vs research canon (docs/skill-evals/2026-07-06-nexus-utility-skills.md; docs/kb/research/dotnet-agent-skill-estates.md)
- **Description:** Routed 5 findings to `docs/plugin-feedback/nexus-1.23.1-2026-07-06.md` — extend lint E6 to `scripts/`/`assets/` citations (F1); add W3 body-size warning >500 lines (F2); sanction `scripts/` in the recipe element menu + scaffold step as the P1 home (F4); add the degrees-of-freedom axis to the recipe (F5); add W4 nested-reference warning (F6). All WARN-level or additive-menu — no behavior break.
- **Date:** 2026-07-06

### evaluate-skill (nexus core — rubric)
- **Status:** Routed-to-plugin
- **Type:** Fix
- **Source:** Utility-skill audit vs research canon (docs/skill-evals/2026-07-06-nexus-utility-skills.md)
- **Description:** Routed 1 finding to `docs/plugin-feedback/nexus-1.23.1-2026-07-06.md` — rubric Layer 0 item 6 (skills-index check) is labeled scripted but skill-lint.mjs has no such check (AP1) and it duplicates Layer 4.3 (AP3); delete from Layer 0. Plus the F5 pointing clause in Layer 2.2 (degrees of freedom).
- **Date:** 2026-07-06

## Deferred Gaps

Coverage dimensions surfaced by the 2026-07-06 external research (`docs/kb/research/dotnet-agent-skill-estates.md`) — recorded, not built; each becomes a skill only when the need first materializes (Quality Gate: repeatable, concrete, no existing coverage).

### upgrade-playbook
- **Status:** Deferred
- **Type:** Gap
- **Source:** Research finding — awesome-copilot ships dotnet-upgrade + MAUI 9→10 playbooks as first-class instruction files
- **Description:** A .NET/package version-migration playbook skill (TFM bumps, breaking-API tiers, CPM version sweeps, validation sequence). Defer until the repo's next framework upgrade provides the real material — a playbook without a lived upgrade is theory.
- **Date:** 2026-07-06

### verification-scripts
- **Status:** Deferred
- **Type:** Gap
- **Source:** Research finding — morphir-dotnet's executable pre-commit gates; Anthropic scripts/ anatomy
- **Description:** Promote the skills' inline verification greps into bundled executable `scripts/` where a grep is insufficient (multi-file invariants, count regressions). Depends on the improve-skills F1/F4 fixes landing first so lint covers script citations.
- **Date:** 2026-07-06

### testing-strategy
- **Status:** Deferred
- **Type:** Gap
- **Source:** Research finding — morphir's four-tool layer mapping with coverage floors; repo has zero test projects (architecture-reference §12.1)
- **Description:** A testing-strategy skill (tool-per-layer, coverage expectations, where tests live per service). Blocked on the owner decision to introduce test projects at all — the course repo currently ships none by design; revisit when mine-verify-cover-dotnet is first used.
- **Date:** 2026-07-06

### observability
- **Status:** Deferred
- **Type:** Gap
- **Source:** Research finding — observability discipline present in mature public estates, absent here beyond the correlation-id chain (infra-2)
- **Description:** Logging/telemetry discipline skill (structured logging conventions, what gets logged where, OpenTelemetry if adopted). Defer until the repo adopts a telemetry stack — currently only Serilog-style console logging + correlation-id middleware exists to encode.
- **Date:** 2026-07-06

### build-release
- **Status:** Deferred
- **Type:** Gap
- **Source:** Research finding — dotnet-cursor-rules' CI/CD, packaging, and BenchmarkDotNet dimensions
- **Description:** Build/release surfaces skill (Dockerfile discipline — incl. the known sdk:8.0 vs net9.0 debt, compose registration, CI pipeline when one exists). Partial overlap with create-service Phase 7 (containerize); defer the rest until a CI pipeline exists.
- **Date:** 2026-07-06
