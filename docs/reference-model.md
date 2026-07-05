# Reference Model — dotnet-microservices

**Species:** reference-model registry (`mine-reference-model` skill) — the "what to copy" registry, sibling to `docs/business-rules/` and `docs/tech-debt/`.
**Mode:** self-reference — reference repo = consuming repo (`D:\src\dotnet-microservices`). The repo's own confirmed pattern choices, formalized as its self-model.
**Provenance:** commit `cd4d0b1`; run-1 rows `run1-20260705`, run-2 rows `run2-20260705`, run-3 rows `run3-20260705`; all verified `2026-07-05`.
**Portability:** collapses to `portable` for every row (same stack — self-reference mode). The translation dictionary section is omitted in this mode.
**Gate:** every row below was re-executed by a fresh skeptic against the source; a verdict without re-execution output was dropped by the orchestrator (none were). Rows are never deleted — verdicts flip, the record stays. Changes append to the changelog.

---

## Dimension: Layering & BuildingBlocks

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| layering-1 | Domain projects never reference Persistence/Application or another service — dependency points inward only. Every Persistence/Application/API project depends on Domain, never the reverse. | `grep -rn "ProjectReference" src/Services --include=*Domain.csproj \| grep -iE "Persistence\|Application"` → zero hits | CONFIRMED |
| layering-2 | BuildingBlocks form a shallow single-concern dependency graph, not a monolithic "Common" lib: `Blocks.Exceptions` and `Blocks.Domain` are leaves (zero refs), `Blocks.Core` depends only on `Blocks.Exceptions`, higher packages pull only the lower blocks they need. Rule: cross-cutting concerns split by concern into small composable packages with minimal footprints. | `grep -n "ProjectReference" src/BuildingBlocks/*/*.csproj` | CONFIRMED |
| layering-3 | `Articles.Abstractions` (ArticleStage, IArticleAction, ArticleCommandBase) is referenced from the **Domain** project of 5 of 6 services — lifecycle vocabulary is domain-level shared language. Journals.Domain is the sole deliberate exception (journals are not lifecycle-bearing articles): shared kernel applied by domain fit, not blanket. | `grep -rln "Articles.Abstractions" src/Services --include=*Domain.csproj` | CONFIRMED |
| layering-4 | No service takes a ProjectReference on another service's Domain/Application/Persistence/API — the microservice boundary is enforced at the build-graph level, not by convention. Cross-service contact only through BuildingBlocks contract packages (`Articles.Grpc.Contracts`, `Articles.Integration.Contracts`). | `for f in src/Services/*/*/*.csproj; do svc=$(echo "$f" \| cut -d/ -f3); grep -o 'Include="[^"]*\.csproj"' "$f" \| sed 's/Include="//;s/"$//' \| tr '\\\\' '/' \| while read -r ref; do target=$(basename "$ref" .csproj); case "$target" in Blocks.*\|Articles.*\|EmailService*\|FileService*\|FileStorage*\|ArticleTimeline*\|${svc}.*) ;; *) echo "CROSS-SERVICE: $f -> $ref";; esac; done; done` → zero CROSS-SERVICE lines across 22 csproj | CONFIRMED |
| layering-5 | The Application layer is optional per service (ArticleHub, Journals skip it); where absent the API references Domain/Persistence directly — a controlled layer-skip. Where Application exists, 3 of 4 APIs (Submission, Review, Production) reference **only** Application; Auth.API is the sole outlier referencing Application + Domain + Persistence. | `grep -n "ProjectReference" src/Services/*/[A-Z]*.API/*.csproj` | CONFIRMED |

**Evidence excerpts**
- layering-1: `(no matches — exit 1)` across all 6 `*Domain.csproj`.
- layering-2: `Blocks.Exceptions.csproj` / `Blocks.Domain.csproj` — no `<ProjectReference>`; `Blocks.Core.csproj → Blocks.Exceptions` only; `Blocks.EntityFrameworkCore → Domain, Exceptions, Core`; `Blocks.MediatR → Core, Domain`; `Blocks.FastEndpoints → AspNetCore, Domain`.
- layering-3: hits = ArticleHub, Auth, Production, Review, Submission Domain csproj; Journals.Domain absent (refs: Grpc.Contracts, Blocks.Domain, Blocks.Redis).
- layering-4: `DONE (nothing above = zero cross-service refs)`. **Audit note:** the extractors' original grep (`Services[\\/]\K…`) was VACUOUS — sibling-service relative paths (`..\..\Other\…`) never contain the literal `Services`, so it returns zero even if a violation existed. The skeptic killed it and the claim was re-proven with the name-prefix check recorded here. *(Merged from: layering P4 + boundaries P3.)*
- layering-5: `Submission.API → Submission.Application` (only); `Review.API → Review.Application` (only); `Production.API → Production.Application` (only); `Auth.API → Application + Persistence + Domain`; `Journals.API → Domain + Persistence`; `ArticleHub.API → Persistence`.

## Dimension: Domain Modeling (DDD)

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| domain-1 | Every aggregate builds on `AggregateRoot<TPrimaryKey>` owning a private `List<IDomainEvent>` exposed read-only, with `AddDomainEvent`/`ClearDomainEvents` as the only mutation points. Rule: domain events are an aggregate-owned side channel — observers can read, never inject. | `rg -n "AddDomainEvent\|ClearDomainEvents\|_domainEvents" src/BuildingBlocks/Blocks.Domain` | CONFIRMED |
| domain-2 | Aggregates use a partial-class split — a state file and a `Behaviors/{Name}.cs` file with the same class name (Submission, Auth, Review, Production). **Corrected scope:** state and behavior files do not always share a folder (Submission separates `Entities/` from `Behaviors/`), and invariant-enforcing `DomainException` throws are NOT fully confined to Behaviors files — the state-machine guard (`IArticleStateMachine.cs`) and one Review consumer also throw. Rule: what an aggregate has is separated from what it does, with the state machine as a sanctioned second enforcement site. | `rg -n "throw new DomainException" src/Services` + partial-class pairing scan | IMPRECISE (corrected) |
| domain-3 | Stage transitions run through a Stateless-backed `IArticleStateMachine` built per-service from an `ArticleStateMachineFactory` delegate, configured at runtime from a cached transition table — legal actions per stage are **data**, not a hardcoded switch; `ValidateStageTransition` runs before any mutation. | `rg -n "Permit\|ArticleStateMachineFactory\|ValidateStageTransition" src/Services src/BuildingBlocks` | CONFIRMED |
| domain-4 | Enums needing metadata are modeled as `EnumEntity<TEnum> : Entity<TEnum>` — a real seeded entity keyed by the enum value — rather than `[Description]` attributes + reflection. Rule: enum metadata that drives behavior (MaxAssetCount, AllowsMultipleAssets) is queryable/joinable domain data. | `rg -n ": EnumEntity<" src` | CONFIRMED |
| domain-5 | Primitive-wrapping value objects (`EmailAddress`, `FileExtension`, `AssetNumber`, `HonorificTitle`) derive from `StringValueObject`/`SingleValueObject<T>`, hide the constructor as `private`, and expose only a static `Create(...)` that runs `Guard.ThrowIf*` first. Rule: an invalid value object is structurally impossible to construct. | `rg -n "private EmailAddress\|Guard\.ThrowIf\|static .*Create" src/Services/Submission/Submission.Domain/ValueObjects/EmailAddress.cs` | CONFIRMED |

**Evidence excerpts**
- domain-1: `AggregateRoot.cs:31-34` — private list, `IReadOnlyList` expose, add/clear only.
- domain-2: `Submission.Domain/IArticleStateMachine.cs:19` and `Review.Domain/IArticleStateMachine.cs:20` throw `DomainException` outside Behaviors; pair confirmed: `Submission.Domain/Entities/Article.cs` (partial, state) + `Submission.Domain/Behaviors/Article.cs` (partial, behavior).
- domain-3: `ArticleStateMachine.cs:19-23` `Configure(...).Permit/PermitReentry` from transition table; `IArticleStateMachine.cs:10` factory delegate; registered in Submission/Review/Production `DependencyInjection.cs`.
- domain-4: `Stage : EnumEntity<ArticleStage>` and `AssetTypeDefinition : EnumEntity<AssetType>, ICacheable` in Submission, Review, Production; base `EnumEntityConfiguration` in Blocks.EntityFrameworkCore.
- domain-5: `EmailAddress.cs:10` private ctor; `:12-15` static `Create` + `Guard.ThrowIfNullOrWhiteSpace` + `Guard.ThrowIfFalse(IsValidEmail…)`.

## Dimension: Boundaries & Messaging

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| boundaries-1 | Domain events (`IDomainEvent`) carry full domain entities and stay in-process; integration events (`Articles.Integration.Contracts`) are flat records carrying only DTOs of primitives/enums, published via MassTransit. Rule: domain objects never leave the process — only their DTO projection does. | `rg -n "record JournalCreated" src/Services/Journals/Journals.Domain` + `rg -n "record ArticleApprovedForReviewEvent" src/BuildingBlocks` | CONFIRMED |
| boundaries-2 | The domain→integration handoff is a named one-purpose handler convention: `PublishIntegrationEventOnXHandler : INotificationHandler<XDomainEvent>` re-fetches the aggregate, maps to DTO via Mapster, publishes via `IPublishEndpoint` — the only place a domain event triggers cross-service traffic. | `rg -n "class PublishIntegrationEventOn" src` + `rg -n "_publishEndpoint.Publish" src/Services` | CONFIRMED |
| boundaries-4 | Synchronous cross-service calls use code-first gRPC (`[ServiceContract]`/`[OperationContract]`/`[ProtoContract]`, protobuf-net.Grpc) defined once in `Articles.Grpc.Contracts`, organized by owning service; used for read-style queries, not state changes. | `rg -n "ServiceContract\|OperationContract\|ProtoContract" src/BuildingBlocks/Articles.Grpc.Contracts/Journals/JournalContracts.cs` | CONFIRMED |
| boundaries-5 | MassTransit/RabbitMQ wiring is centralized in one shared `AddMassTransitWithRabbitMQ` extension every service calls identically; consumers auto-discovered by assembly scan; endpoint names via `SnakeCaseWithServiceSuffixNameFormatter`; one integration event fans out pub/sub to multiple independent consumers. | `rg -n "AddMassTransitWithRabbitMQ\|AddConsumers\|SnakeCaseWithServiceSuffix" src` + `rg -n "IConsumer<ArticleApprovedForReviewEvent>" src` | CONFIRMED |

**Evidence excerpts**
- boundaries-1: `JournalCreated.cs:5: record JournalCreated(Journal Journal) : IDomainEvent;` vs `ArticleApprovedForReviewEvent.cs:6: record ArticleApprovedForReviewEvent(ArticleDto Article);` — `ArticleDto` is primitives/enums/nested DTOs only.
- boundaries-2: 4 handlers (Submission ApproveArticle, Journals Create/Update, Review AcceptArticle); all 4 `_publishEndpoint.Publish` sites sit inside `PublishIntegrationEventOn*` files — no stray publish.
- boundaries-4: `JournalContracts.cs:7 [ServiceContract]`, `:10/:13 [OperationContract]`, `:17/24/31/47/57 [ProtoContract]`.
- boundaries-5: `Blocks.Messaging/DependencyInjection.cs:12,22,26`; `ArticleApprovedForReviewEvent` consumed by Journals, Review, ArticleHub (3-way fan-out).
- *(boundaries-3 was merged into layering-4 — see that row.)*

## Dimension: Endpoint & Feature Slicing

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| endpoints-1 | Endpoint framework is a per-service decision declared in each service's `CLAUDE.md`; FastEndpoints, Carter+MediatR, and Minimal APIs+MediatR coexist. Rule: endpoint framework is an implementation detail of the service, not an architectural constant. | `rg -n "Endpoint framework" src/Services/*/CLAUDE.md` | CONFIRMED |
| endpoints-2 | MediatR presence is the fork for handler shape: FastEndpoints-without-MediatR services (Journals, Production) fold handler logic into the endpoint class (zero `IRequestHandler` files); MediatR services (Review 11, Submission 10) keep the endpoint a thin shim dispatching via `ISender`. | `rg -l "IRequestHandler<" src/Services` (per-service count) | CONFIRMED |
| endpoints-3 | Every API organizes as `Features/{Area}/{Operation}/` folders colocating endpoint + request + validator (+ handler when MediatR) — no `Controllers/` or `Validators/` god-folder exists anywhere. | `find src/Services -type d -name Controllers` → zero hits | CONFIRMED |
| endpoints-4 | Validators are co-located per feature folder; the base type follows the framework — FastEndpoints services extend `Validator<T>` (or project-local `BaseValidator<T>`), MediatR services extend `AbstractValidator<T>`. Rule: validation is a feature-folder concern, never a cross-cutting layer. | `rg -n "class \w+Validator" src` | CONFIRMED |
| endpoints-5 | Authorization is a declarative gate at the endpoint boundary — `[Authorize(Roles=…)]` or `.RequireAuthorization()`/`.RequireRoleAuthorization(…)` — never a role check inside a handler or domain body (zero `IsInRole` in business code). | `rg -n "\[Authorize\|\.RequireAuthorization" src` + `rg -n "IsInRole" src/Services` → zero | CONFIRMED |

**Evidence excerpts**
- endpoints-1: Submission = Minimal APIs + MediatR; Review = Carter + MediatR; Journals/Production = FastEndpoints (no MediatR); Auth = FastEndpoints (partial MediatR); ArticleHub = Carter (partial).
- endpoints-2: `IRequestHandler` files — Review 11, Submission 10, Journals 0, Production 0.
- endpoints-3: e.g. `Production.API/Features/Assets/UploadFiles/UploadDraftFile/` = Command + Endpoint; `Journals.API/Features/Journals/Create/` = Command + Endpoint + PublishIntegrationEvent handler.
- endpoints-4: `UpdateSectionCommandValidator : Validator<…>` (Journals/FE) vs `CreateUserCommandValidator : AbstractValidator<…>` (Auth) vs `BaseValidator<T> : Validator<T>` (Production `_Shared`).
- endpoints-5: 27 declarative gate sites; `IsInRole` in handler/domain bodies: zero.

## Dimension: Persistence

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| persistence-1 | The repository IS the unit of work — `SaveChangesAsync`, `ClearTracking`, range ops all live on `RepositoryBase`; no `IUnitOfWork` abstraction exists anywhere. `DeleteByIdAsync` deliberately drops to raw `ExecuteSqlInterpolatedAsync` (EF can't materialize a delete-only entity with required properties). Rule: one seam per aggregate; don't multiply abstractions. | `rg -n "SaveChangesAsync\|ExecuteSqlInterpolatedAsync\|ClearTracking" src/BuildingBlocks/Blocks.EntityFrameworkCore/Repositories/Repository.cs` + `rg -n "IUnitOfWork" src` → zero | CONFIRMED |
| persistence-2 | Entity configuration is an inheritance ladder: `EntityConfiguration<T>` (key + generated-id toggle + JSON-file seeding baked into every `Configure()`) → `AuditedEntityConfiguration<T>` (audit columns mandatory, constrained to `IAggregateRoot`; opt-in default-OFF `RowVersion` concurrency token — zero opt-ins to date). Rule: audit is mandatory for aggregates, concurrency is opt-in, seeding is free. | `rg -n "HasConcurrencyToken" src/BuildingBlocks src/Services` | CONFIRMED |
| persistence-3 | Domain-event dispatch is a `SaveChangesInterceptor` in two variants selected per service: post-save `DispatchDomainEventsInterceptor` (Submission, Review) vs `TransactionalDispatchDomainEventsInterceptor` (Production — handlers write in the same transaction). Rule: default cheap post-save dispatch; pay for the transaction only where event handlers must write atomically with the trigger. | `rg -n "AddScoped<ISaveChangesInterceptor" src/Services` | CONFIRMED |
| persistence-4 | Storage engine follows service role: PostgreSQL for ArticleHub (read-side projection), SQL Server for write-side services (Auth, Submission, Review, Production), Redis.OM for Journals — which gets no EF machinery at all (no DbContext, no interceptors, no repository-UoW; a thin `IRedisCollection<T>` wrapper). Rule: EF's full machinery is reserved for real write-side invariants; CRUD-shaped aggregates get a document store without the ceremony. | `rg -n "Npgsql.EntityFrameworkCore.PostgreSQL\|Microsoft.EntityFrameworkCore.SqlServer" src/Services -g "*.csproj"` | CONFIRMED |
| persistence-5 | Caching is reserved for small, rarely-changing reference tables — marker interface `ICacheable`, loaded via `IMemoryCache` keyed only by CLR type FullName (whole-table, process-wide, no eviction), exposed via `CachedRepository<>` and `GetAllCached<T>()`; Submission prewarms at startup via `DatabaseCacheLoader` (IHostedService). Rule: the type-only cache key excludes by construction anything per-request or per-instance. | `rg -n "GetOrCreateByType" src/BuildingBlocks/Blocks.Core/Cache/MemoryCacheExtensions.cs` | CONFIRMED |

**Evidence excerpts**
- persistence-1: `Repository.cs:68` raw SQL delete; `:80-81` SaveChangesAsync; `:83` ClearTracking; `IUnitOfWork` → zero hits.
- persistence-2: `AuditedEntityConfiguration.cs:23` `HasConcurrencyToken => false`; `:34-35` conditional `RowVersion`; `EntityConfiguration.cs:24-25` `HasKey` + `SeedFromJsonFile()` in base `Configure`; zero overrides enable the token (flag added in `793056c`, unused).
- persistence-3: Submission/Review → `DispatchDomainEventsInterceptor`; Production → `TransactionalDispatchDomainEventsInterceptor`.
- persistence-4: ArticleHub → Npgsql; Auth/Submission/Review/Production → SqlServer; Journals.Persistence has no EF provider — `JournalDbContext` wraps `RedisConnectionProvider`.
- persistence-5: `MemoryCacheExtensions.cs:8` `GetOrCreate(typeof(T).FullName!, factory)`; users: `AssetTypeRepository`, `DatabaseCacheLoader` (`ArticleStageTransition`, `AssetTypeDefinition`).

## Dimension: Error Handling

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| errors-1 | `Blocks.Exceptions` defines one `HttpException` base carrying `HttpStatusCode`, with `BadRequest`/`NotFound`/`Unauthorized` as thin status-picking subclasses. Rule: exceptions self-describe their transport status — no separate exception→status lookup table. | `rg -n "class \w+Exception" src/BuildingBlocks/Blocks.Exceptions` | CONFIRMED |
| errors-2 | `DomainException` lives in `Blocks.Domain` and is a plain `Exception` with no HTTP status. Rule: the domain layer does not know HTTP exists; translating a rule violation to a status code is an infrastructure decision made later. | `rg -n "class DomainException" src/BuildingBlocks/Blocks.Domain -A1` | CONFIRMED |
| errors-3 | A single `GlobalExceptionMiddleware`, registered in all 6 services' `Program.cs`, is the only place mapping exception types to HTTP status (one switch expression) and the only try/catch around the request pipeline. Rule: exception→response translation is centralized at one boundary. | `rg -n "UseMiddleware<GlobalExceptionMiddleware>" src` (6 sites = 6 services) | CONFIRMED |
| errors-4 | `Guard`/`GuardExtensions` in Blocks.Core centralize null/not-found throwing checks (`AgainstNull`, `NotFound<T>`, `OrThrowNotFound<T>`) as declarative calls, not inline `if (x == null) throw`. (Source keeps two competing styles side by side deliberately, per an in-code insight comment.) | `rg -n "OrThrowNotFound\|NotFound<T>\|AgainstNull" src/BuildingBlocks/Blocks.Core` | CONFIRMED |
| errors-5 | Real try/catch exists only at true I/O boundaries: 18 catch sites in 11 files — the global middleware, file-storage adapters (MinIO/GridFS/AzureBlob), SMTP, Hasura client, EF seeding, and the 3 upload-file wrappers calling those adapters. Business, domain, and endpoint code just throws. | `rg -n "catch\s*\(" src` (18 hits, all boundary files) | CONFIRMED |

**Evidence excerpts**
- errors-1: `HttpException : Exception`; `BadRequestException/NotFoundException/UnauthorizedException : HttpException`.
- errors-2: `DomainException.cs:3-4` — primary-constructor plain Exception.
- errors-3: `GlobalExceptionMiddleware.cs:15` `MapStatusCode(...) => ex switch`; wired in Review, Auth, Journals, ArticleHub, Production, Submission (6 of 6 — an extractor's "7 services" note was wrong; corrected here).
- errors-4: `GuardExtensions.cs:8`, `Guard.cs:25,28`; `GuardExtensions.cs:7` insight comment on the two styles.
- errors-5: middleware(3), Hasura(3), file adapters(6), SMTP(1), EF seed(2), upload wrappers(3) = 18.

## Dimension: Application / CQRS Pipeline *(run 2)*

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| appcqrs-1 | Every article command inherits `ArticleCommandBase<TActionType>`, which self-computes `CreatedOn => DateTime.UtcNow`, derives `Action` from an abstract `ActionType`, and `[JsonIgnore]`s every audit/provenance field (ArticleId, ActionType, Action, CreatedOn, CreatedById) — only `Comment` is wire-settable. Rule: provenance is pipeline-computed, never client-trusted. | `rg -n "ArticleCommandBase" src` + read `Articles.Abstractions/ArticleCommandBase.cs` | CONFIRMED |
| appcqrs-2 | MediatR services register open behaviors in a fixed order — `AssignUserIdBehavior` → `ValidationBehavior` → `LoggingBehavior` — identically in Submission and Review. Rule: identity is stamped before validation runs; logging wraps only the innermost span. | `rg -n "AddOpenBehavior" src` | CONFIRMED |
| appcqrs-3 | Failures never flow as a `Result<T>`/OneOf/ErrorOr/FluentResults return type — no error-monad library is referenced and no handler returns one; failures are thrown and translated once at the middleware. (Partial overlap with errors-1/errors-3 for the mapping half; this row's new claim is the verified zero-monad absence.) | `rg -n "OneOf\|FluentResults\|ErrorOr\|LanguageExt" --glob '*.csproj'` + `rg -n "Task<Result<\|: Result<\|=> Result<" src/Services` → zero hits | CONFIRMED |
| appcqrs-5 | Commands and queries are distinguished by marker interfaces (`ICommand<T>`/`IQuery<T>`, thin wrappers over `IRequest<T>`), but no pipeline behavior branches on the distinction. Rule: CQRS intent is expressed in the type system for readability, not (yet) used to vary pipeline behavior between reads and writes. | `rg -n "interface ICommand\|interface IQuery" src/BuildingBlocks/Blocks.MediatR` + behavior constraint grep | CONFIRMED |

**Evidence excerpts**
- appcqrs-1: `ArticleCommandBase.cs` — `[JsonIgnore]` on ArticleId/ActionType/Action/CreatedOn/CreatedById; `CreatedOn => DateTime.UtcNow` computed.
- appcqrs-2: `Submission.Application/DependencyInjection.cs:26-28` and `Review.Application/DependencyInjection.cs:26-28` — same three, same order.
- appcqrs-3: csproj monad libs → zero hits; Result-typed handler returns → zero hits; the only `Result<` hits are `ArticleGraphQLReadStore.cs` `QueryResult<ArticleDto>`/`SingleResult<ArticleDto>` (Hasura response envelopes, not error monads). Non-vacuous: handlers return `IdResponse`, `InviteReviewerResponse`, etc.
- appcqrs-5: `ValidationBehavior: where TRequest : IRequest<TResponse>`; `LoggingBehavior: where TRequest : notnull, IRequest<TResponse>`; `AssignUserIdBehavior: where TRequest : IAuditableAction` — none constrain on ICommand/IQuery.
- *(appcqrs-4 was merged into security-4 — id retired, never reused.)*

## Dimension: Module Composition *(run 2)*

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| modules-1 | EmailService/FileService split into a `.Contracts` project plus sibling implementation projects; each implementation references Contracts and (in 5 of 6 cases) the shared `Blocks.Core` — but never a sibling implementation. Rule: swapping providers never touches consumer or peer-provider code. *(Corrected: original "each depends only on Contracts" was wrong — Blocks.Core is also referenced.)* | `grep -rn "ProjectReference" src/Modules/EmailService/*/*.csproj src/Modules/FileService/*/*.csproj` | IMPRECISE (corrected) |
| modules-2 | Implementation selection is compile-time and explicit — every consuming service activates exactly one `AddEmptyEmailService(...)` call, never a config/reflection switch; Submission/Auth/Review keep the unused alternative as a commented-out sibling line (Production has only the active line). Rule: provider choice is a visible code decision. *(Corrected: commented-sibling decoration is 3 of 4, not universal.)* | `rg -n "Add(Empty\|Smtp\|SendGrid)EmailService" src/Services` | IMPRECISE (corrected) |
| modules-3 | `IFileService<TFileStorageOptions>` uses a generic options-marker type so one service registers the same implementation multiple times for different purposes — each purpose subclasses `MongoGridFsFileStorageOptions`, resolved by closed generic (`GetRequiredService<IFileService<SubmissionFileStorageOptions>>`), not named/keyed lookup. Rule: multi-instance module composition via generics + DI typing. | `rg -n "IFileService<\|FileStorageOptions\|AddMongoFileStorage" src/Services/Review/Review.API` | CONFIRMED |
| modules-4 | ArticleTimeline is a different module archetype: Domain+Persistence+Application, no Contracts project, no alternate implementation, exactly one consumer (Production), sharing the host's `DbConnection` with only its migrations-history table namespaced. Rule: Contracts + swappable providers only when multiple implementations/consumers exist; a single-consumer domain slice embeds into its owner's database. | `find src/Modules/ArticleTimeline -name "*.csproj"` + `grep -rln "ArticleTimeline" src/Services` + DI grep | CONFIRMED |
| modules-5 | BuildingBlocks are technical primitives available to everything (`Blocks.Core` referenced by 17 projects across the tree); Modules are named business capabilities pulled in selectively — Journals and ArticleHub reference no module at all. | `grep -rLE "EmailService\|FileService\|FileStorage" --include="*.csproj" src/Services/Journals src/Services/ArticleHub` | CONFIRMED |

**Evidence excerpts**
- modules-1: `EmailService.SendGrid → Contracts` only; `Empty`/`Smtp`/`AzureBlob`/`MongoGridFS`/`MinIO` → Contracts + Blocks.Core; no impl→impl reference anywhere.
- modules-2: `Submission:57-58`, `Auth:63-64`, `Review:62-63` — active `AddEmptyEmailService` + commented `//AddSmtpEmailService`; `Production:64` active only.
- modules-3: `ReviewFileStorageOptions : MongoGridFsFileStorageOptions`; `SubmissionFileStorageOptions : MongoGridFsFileStorageOptions`; DI: `AddMongoFileStorageAsSingletone` + `AddMongoFileStorageAsScoped<SubmissionFileStorageOptions>`; factory switches on `FileStorageType`.
- modules-4: csproj set = Application/Domain/Persistence (no Contracts); sole consumer `Production.Application.csproj`; `DependencyInjection.cs:15` `GetRequiredService<DbConnection>()`, `:19` `MigrationsHistoryTable("__EFMigrationsHistory","ArticleTimeline")`.
- modules-5: all 6 Journals/ArticleHub csproj carry zero module refs; `Blocks.Core.csproj` in 17 csproj.

## Dimension: Consumer / Projection *(run 2)*

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| consumers-1 | MassTransit consumers take persistence collaborators as concrete types — the concrete DbContext and `Repository<T>`/`{X}Repository`, never a repository interface (the "no interfaces without multiple implementations" guardrail applied). Non-persistence collaborators (IFileService, factories) ARE injected as interfaces. *(Corrected: original "never an interface abstraction" was too broad.)* | read consumer constructors: `rg -n "class \w+Consumer\(" src -A5` | IMPRECISE (corrected) |
| consumers-2 | Consumers use three deliberately different duplicate-delivery strategies: silent skip-if-exists (Journal consumers), throw-if-exists (`EnsureNotExistsOrThrowAsync` in Review; `AnyAsync`→throw in ArticleHub), and `ExistsAsync`→silent return (Production). No house-wide idempotency policy — the Review consumer's `//insight - inbox pattern vs simple business rules` comment marks the variance as intentional/pedagogical. | `rg -n "Idempotency\|EnsureNotExistsOrThrowAsync\|ExistsAsync\|inbox pattern" src` | CONFIRMED |
| consumers-3 | Every article-creation consumer re-derives foreign-owned reference data (Journal, Person) locally: query the local DB first, materialize from the event DTO only if missing (`GetOrCreate*` helpers). Materialization varies — ArticleHub/Review use Mapster `Adapt<T>()`, Production hand-constructs. Rule: duplicate-and-reconcile per event, not query-on-demand. *(Corrected: "via Mapster" is not universal.)* | `rg -n "GetOrCreate" src/Services --glob "*Consumer.cs"` | IMPRECISE (corrected) |
| consumers-4 | When an article changes stage, the receiving consumer physically migrates each asset's bytes — `DownloadAsync` from the sending stage's store, `UploadAsync` into its own — rather than sharing storage or passing a pointer. Rule: each service owns its file-storage technology, at the cost of duplicating bytes per stage. | `rg -n "DownloadAsync\|UploadAsync" src/Services/Production/.../ArticleAcceptedForProductionConsumer.cs src/Services/Review/.../ArticleApprovedForReviewConsumer.cs` | CONFIRMED |
| consumers-5 | ArticleHub's `Article` is a plain data bag — public get/set, `List<T>` collections, extends only `Entity`, zero behavior methods — a genuine read-model projection, in deliberate contrast to the write-side DDD aggregates. | `cat src/Services/ArticleHub/ArticleHub.Domain/Entities/Article.cs` | CONFIRMED |

**Evidence excerpts**
- consumers-1: Production consumer ctor = `ProductionDbContext, ArticleRepository, Repository<Person>, Repository<Journal>, AssetTypeRepository, IFileService<ReviewFileStorageOptions>, IFileService`; Review consumer similar + `FileServiceFactory, ArticleStateMachineFactory`.
- consumers-2: `JournalCreatedConsumer` "// Idempotency: skip if already exists"; ArticleHub `:20 AnyAsync → throw`; Review `:28-29` insight comment + `EnsureNotExistsOrThrowAsync`; Production `:30 ExistsAsync → return`.
- consumers-3: ArticleHub `:43 articleDto.Journal.Adapt<Journal>()`; Review `:57 Adapt<Journal>()`; Production `:52 new Journal { Id=…, Name=… }` — no Adapt.
- consumers-4: Review `:75→:77` submission-store → review-store; Production `:106→:108` review-store → Azure Blob.
- consumers-5: `Article : Entity` — properties and collections only, no methods in file.

## Dimension: Conventions & Naming *(run 2)*

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| conventions-1 | Central Package Management with zero inline `Version=` attributes on any csproj (49 scanned). Centralization is split, not single-file: root `src/Directory.Packages.props` plus a nested `Review.API/Directory.Packages.props` declaring its own PackageVersion entries. Rule: no per-project version literals — but the nested props file is a CPM-correctness smell (routed to debt). *(Corrected: "one central file" was wrong.)* | `grep -rn "Version=" --include="*.csproj" src/` → zero + `find src -iname Directory.Packages.props` | IMPRECISE (corrected) |
| conventions-2 | Layered projects ship their own `GlobalUsings.cs` (17 files). The category header grouping (`// Third-party libraries` / `// Internal libraries` / `// Domain`) is a majority habit — 11 of 17 files — not a guarantee. Rule: per-project curated usings; header taxonomy aspirational. *(Corrected: grouping is 11/17, not universal.)* | `find src -iname "GlobalUsings.cs"` + header-presence check | IMPRECISE (corrected) |
| conventions-3 | Service DbContexts mirror the owner's name (AuthDbContext, ReviewDbContext, ProductionDbContext, SubmissionDbContext, ArticleHubDbContext; Journals uses singular `JournalDbContext`); no generic per-service `AppDbContext` exists. BuildingBlocks provides generic bases (`ApplicationDbContext<TDbContext>`, `TenantDbContext`). Rule: context identity mirrors ownership. *(Corrected: "zero exceptions" refuted by the Journals singular + generic bases.)* | `grep -rn "class.*DbContext" --include="*.cs" src/ \| grep -v Snapshot` | IMPRECISE (corrected) |
| conventions-4 | String column lengths come from the shared `Blocks.Core.MaxLength` power-of-two constants (C0…C2048) — 85 active `HasMaxLength(MaxLength.*)` sites; the only numeric-literal `HasMaxLength(500)` calls are commented-out dead lines. Rule: column lengths are a shared vocabulary, not per-config literals. | `rg -n "HasMaxLength\(MaxLength\." src \| wc -l` + `rg -n "HasMaxLength\([0-9]" src` | CONFIRMED |
| conventions-5 | Explicitly declared private instance fields are consistently `_camelCase`. Primary-constructor captures are NOT standardized — `dbContext` and `_dbContext` coexist even between sibling repositories of the same service (Production `ArticleRepository(… dbContext)` vs `AssetRepository(… _dbContext)`). Rule: the underscore convention governs declared fields; capture naming is unsettled. *(Corrected: "mechanical" capture claim refuted.)* | `rg -n "class \w+Repository\(\w+DbContext (dbContext\|_dbContext)" src/Services` | IMPRECISE (corrected) |

**Evidence excerpts**
- conventions-1: inline `Version=` → zero hits; nested props body pins Carter 9.0.0, Microsoft.CodeAnalysis 4.14.0, OpenApi/EFCore.Design 9.0.8.
- conventions-2: no-header files = Blocks.EntityFrameworkCore, Auth.Domain, Production.API, Production.Domain, Review.API, Review.Domain.
- conventions-3: 5 exact `{Service}DbContext` + `JournalDbContext` (singular); generic bases in BuildingBlocks only.
- conventions-4: 85 `MaxLength.*` sites; `Production/ArticleEntityConfiguration.cs:59-63` literals are all commented out.
- conventions-5: no-underscore captures: ArticleRepository ×4, UserRepository, PersonRepository, AssetTypeRepository; underscore captures: AssetRepository ×3, FileRepository.

## Dimension: Security & Identity Wiring *(run 2)*

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| security-1 | Roles are a closed `UserRoleType : int` enum, numeric-range-coded per domain (1-9 cross-domain, 11-19 submission, 21-29 review, 31-39 production, 91-99 auth); `Articles.Security.Role` exposes them only as string constants via `nameof()`. Rule: one shared role vocabulary across all services; ranges reserve namespace. | `rg -n "= [0-9]" src/BuildingBlocks/Articles.Abstractions/Enums/UserRoleType.cs` + read `Role.cs` | CONFIRMED |
| security-2 | Role-gated article endpoints stack two layers via the single `RequireRoleAuthorization` extension — `policy.RequireRole(roles)` + `policy.Requirements.Add(new ArticleRoleRequirement(roles))`; bare `RequireRole` never appears outside the extension. ArticleHub's read-only endpoints are the sanctioned exception: single-layer `.RequireAuthorization()` (any authenticated user, no resource check). Rule: writes get role + resource gates; aggregate reads get authentication only. *(Corrected: "always two layers" refuted for the read side.)* | read `Articles.Security/Extensions.cs` + `rg -n "\.RequireAuthorization\(\|\.RequireRole\(" src/Services` | IMPRECISE (corrected) |
| security-3 | `IArticleAccessChecker.HasAccessAsync` is implemented per service against that service's own `ArticleActors` table (local, integration-event-replicated data — never a synchronous cross-service query), with a per-service admin bypass: Submission `Overlaps([EOF, REVED])`, Review `Contains(EOF)`, Production `Contains(POF)`. Rule: resource access is answered from local data; the bypass role set is service-specific. *(Corrected: bypass set is not a uniform EOF/REVED.)* | `rg -n "HasAccessAsync\|Overlaps\|Contains\|ArticleActors" src/Services/**/ArticleAccessChecker.cs` | IMPRECISE (corrected) |
| security-4 | Identity stamping (`CreatedById` on `IAuditableAction`) is reimplemented once per endpoint framework: `AssignUserIdPreProcessor` (FastEndpoints), `AssignUserIdFilter` (Minimal APIs), `AssignUserIdBehavior` (MediatR) — each hooking the one shared `IClaimsProvider`. The guard is NOT identical: the MediatR behavior and Minimal-API filter use `TryGetUserId()` and skip when absent; the FastEndpoints preprocessor calls `GetUserId()`, which throws on a missing claim. Rule: identity injection is a framework-boundary concern; the contract stays shared, the missing-claim policy does not. *(Merged from appcqrs P4 + security P4. Corrected 2026-07-05: the original "identical only-if-present guard" clause was refuted during skill-building — see changelog.)* | `rg -n "TryGetUserId\|GetUserId" src/BuildingBlocks --glob "**/AssignUserId*.cs"` | IMPRECISE (corrected) |
| security-5 | JWT validation deliberately skips audience (`ValidateAudience = false`) while enforcing issuer + issuer-signing-key, mapping roles via `ClaimTypes.Role`. Rule: single-audience internal system — issuer and signature are the boundary, audience is not. | `rg -n "ValidateAudience\|ValidateIssuer\|RoleClaimType" src/BuildingBlocks/Articles.Security/ConfigureAuthentication.cs` | CONFIRMED |

**Evidence excerpts**
- security-1: `EOF=1, AUT=11, CORAUT=12, REVED=21, REV=22, POF=31, TSOF=32, USERADMIN=91`; `Role.Author = nameof(UserRoleType.AUT)` etc.
- security-2: `Extensions.cs` both overloads pair RequireRole + ArticleRoleRequirement; ArticleHub `SearchArticles/GetTimeline/GetArticle` use `.RequireAuthorization() // allows all authenticated users`.
- security-3: Submission `:16 Overlaps([EOF, REVED])`; Review `:16 Contains(EOF)`; Production `:17 Contains(POF)`; all fall through to `ArticleActors.AnyAsync(...)`.
- security-4: `Blocks.MediatR/Behaviors/AssignUserIdBehavior.cs:18`; `Blocks.FastEndpoints/AssignUserIdPreProcessor.cs:14`; `Blocks.AspNetCore/Filters/AssignUserIdFilter.cs:17`.
- security-5: `:26 ValidateIssuer=true`, `:28 ValidateIssuerSigningKey=true`, `:30 ValidateAudience=false`, `:32 RoleClaimType=ClaimTypes.Role`.

## Dimension: Domain-Event Publication Plumbing *(run 3)*

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| eventpub-1 | One producer seam — `IDomainEventPublisher.PublishAsync` in Blocks.Domain — with two interchangeable implementations: `Blocks.MediatR.DomainEventPublisher` (wraps `IMediator.Publish`; Production, Submission, Review) and `Blocks.FastEndpoints.DomainEventPublisher` (wraps `@event.PublishAsync(Mode.WaitForAll)`; Auth, Journals). 5 of 6 services register one (ArticleHub, pure read model, registers none). Rule: services depend on the abstraction; the dispatch library is a DI choice. | `rg -n "AddScoped<IDomainEventPublisher" src` | CONFIRMED |
| eventpub-2 | The domain event contract is dual-typed — `IDomainEvent : INotification, IEvent` — so one event class satisfies both MediatR's and FastEndpoints' contracts simultaneously. Rule: dispatch-library choice is DI-time, never a modeling decision. | `rg -n "interface IDomainEvent" src/BuildingBlocks/Blocks.Domain` | CONFIRMED |
| eventpub-4 | Production declares "FastEndpoints (no MediatR)" yet registers `AddMediatR` + the MediatR publisher — MediatR wired purely as the domain-event bus (one `INotificationHandler`, zero `ISender`/`IRequestHandler`/`IRequest<`). Rule: request-dispatch framework and event-dispatch library are independent decisions. | `rg -n "ISender\|IRequestHandler\|IRequest<" src/Services/Production` → zero hits | CONFIRMED |
| eventpub-5 | Domain-event handlers are assembly-scan discovered, colocated in the feature folder they serve, and named by effect: `<Effect>On<Event>Handler`. The sanctioned exception is ArticleTimeline, which reacts generically to many aggregates' events via an abstract generic base (`AddTimelineEventHandler<TDomainEvent,TAction>`) in a dedicated `EventHandlers/` folder — it isn't owned by a single feature. | `rg -n "class \w+On\w+Handler" src` | CONFIRMED |

**Evidence excerpts**
- eventpub-1: Production DI:53 → `Blocks.MediatR.DomainEventPublisher`; Submission/Review Application DI:32 → MediatR impl; Auth DI:52, Journals DI:45 → FastEndpoints impl.
- eventpub-2: `Blocks.Domain/IDomainEvent.cs:6: public interface IDomainEvent : INotification, IEvent;`
- eventpub-4: zero request-side hits; `NotifyProductionOfficeOnArticleAcceptedHandler.cs:14 : INotificationHandler<ArticleAcceptedForProduction>`; `Production.API/DependencyInjection.cs:52 AddMediatR`.
- eventpub-5: SendConfirmationEmailOnUserCreated, NotifyProductionOfficeOnArticleAccepted, PublishIntegrationEventOn* — all in feature folders; ArticleTimeline `AddTimelineWhenArticleStageChangedEventHandler : AddTimelineEventHandler<ArticleStageChanged, IArticleAction>`.
- *(eventpub-3 was killed as WRONG — see the run-3 report. Id retired, never reused.)*

## Dimension: Cross-Service Data Access *(run 3)*

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| datasharing-1 | gRPC is used for lazy get-or-create hydration on the write path — when a command references a foreign entity (Journal, Person) missing locally, the handler fetches it synchronously and persists a local copy before proceeding (two sub-variants: local-null-guard; server-side get-or-create). Rule: gRPC bridges gaps in local data at the moment a write needs it — not a general read path. | `rg -n "_journalClient\.\|_personClient\." src/Services/Submission` | CONFIRMED |
| datasharing-2 | Foreign Journal reference data is kept fresh via dedicated `JournalCreated`/`JournalUpdated` consumers that upsert a local shadow row in each consuming write service (Submission, Review). Rule: upstream changes replicate eagerly so downstream reads never need a synchronous call. | `rg -n "class Journal(Created\|Updated)Consumer" src` | CONFIRMED |
| datasharing-3 | gRPC is reserved for checks that must be authoritative at the instant of the decision — `IsEditorAssignedToJournalAsync` gating the ApproveArticle state transition (throws BadRequest on failure). Rule: authorization/business gates on state transitions go over gRPC live; only descriptive data gets replicated. (n=1 instance of the category.) | `rg -n "IsEditorAssignedToJournalAsync" src` | CONFIRMED |
| datasharing-4 | ArticleHub — the cross-service aggregate read view — has zero gRPC client registrations; it is populated exclusively from integration-event DTOs carrying denormalized snapshots. Rule: a pure read model never reaches back over gRPC; every field it needs must already be in the event payload. | `rg -n "IPersonService\|IJournalService\|AddCodeFirstGrpcClient" src/Services/ArticleHub` → zero hits (non-vacuous) | CONFIRMED |
| datasharing-5 | Every gRPC client registers through one shared helper — `AddCodeFirstGrpcClient<TClient>(grpcOptions, serviceKey)` — resolving URLs from config-bound `GrpcServicesOptions`; both `GrpcChannel.ForAddress` sites live inside the shared extensions file. Rule: no service hand-rolls channel creation; cert handling and address resolution are centralized. | `rg -n "AddCodeFirstGrpcClient<I" src` + `rg -n "GrpcChannel.ForAddress" src` | CONFIRMED |

**Evidence excerpts**
- datasharing-1: `CreateArticleCommandHandler.cs:33` (only when local lookup null); `ApproveArticleCommandHandler.cs:40-46` (`if person is null` → fetch → `Person.Create` → Add); `CreateAuthorCommandHandler.cs:29` `GetOrCreatePersonAsync`.
- datasharing-2: Submission + Review each have `JournalCreatedConsumer` and `JournalUpdatedConsumer` (distinct from consumers-3's article-event-embedded hydration).
- datasharing-3: `ApproveArticleCommandHandler.cs:15-16` `if(!await IsEditorAssignedToJournal(...)) throw BadRequestException`; contract `JournalContracts.cs:14`; server `JournalGrpcService.cs:19`.
- datasharing-4: zero hits; the same grep hits Submission/Journals/Review (non-vacuous).
- datasharing-5: Submission DI:63-64, Journals DI:56, Review DI:67; `GrpcChannel.ForAddress` only at `GrpcClientRegistrationExtensions.cs:58,75`.

## Dimension: File-Storage Abstraction *(run 3)*

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| filestorage-1 | Multi-storage disambiguation uses the generic options-marker interface (`IFileService<TFileStorageOptions>`), never .NET keyed DI — zero `AddKeyed` anywhere in the repo. Rule: the type system is the DI key. (Mechanism detail lives in modules-3; this row proves the keyed-DI absence.) | `rg -n "AddKeyed" src` → zero hits | CONFIRMED |
| filestorage-2 | Two resolution strategies coexist deliberately: Review uses an enum-keyed delegate factory (`FileServiceFactory: FileStorageType → IFileService`) because the choice is dynamic at runtime; Production injects `IFileService<TOptions>` + `IFileService` directly because both stores are always needed together. Rule: factory ceremony only when the choice is dynamic. | `rg -n "delegate IFileService FileServiceFactory\|IFileService<" src` | CONFIRMED |
| filestorage-3 | Every upload handler (one shared base per write-side service ×3) wraps the domain mutation in try/catch and compensates with `TryDeleteAsync` on failure. Rule: file-storage writes sit outside the SQL transaction — a compensating delete substitutes for cross-store atomicity. | `rg -n "TryDeleteAsync" src` | CONFIRMED |
| filestorage-4 | `FileUploadRequest`/`FileMetadata` are plain immutable records defined once in FileStorage.Contracts and passed verbatim across provider and service boundaries; no provider-native type (BsonDocument, BlobHttpHeaders, Tagging) ever crosses the `IFileService` boundary. | `rg -n "public record File" FileService.Contracts/IFileService.cs` + `rg -n "BsonDocument\|BlobHttpHeaders\|Tagging" src/Modules/FileService` | CONFIRMED |

**Evidence excerpts**
- filestorage-1: zero `AddKeyed` hits; `IFileService.cs:7 IFileService<TFileStorageOptions> : IFileService`.
- filestorage-2: `Review FileServiceFactory.cs:11` delegate; `FileServiceFactoryRegistration.cs:16-17` enum→closed-generic switch; Production consumer ctor `:21-22` direct dual injection.
- filestorage-3: Submission `_Shared/UploadFileCommandHandler.cs:23-33` try/catch + `TryDeleteAsync`; Review `:31`; Production `UploadFileEndpoint.cs:44` — same compensating delete in all three.
- filestorage-4: `FileUploadRequest(...)`/`FileMetadata(...)` records at `IFileService.cs:26,28`; `BlobHttpHeaders` only inside AzureBlob's own `BlobClientExtensions.cs:12`.
- *(filestorage source-P2, physical byte migration, was excluded as a duplicate of consumers-4.)*

## Dimension: Cross-Stack Naming Transforms *(run 3)*

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| namingx-1 | The snake_case table rewriter is passed by exactly one context — ArticleHub (PostgreSQL). Three SQL Server write contexts (Submission, Review, Production) call `UseEntityTypeNamesAsTables()` with no rewriter; Auth omits the call entirely and lands on PascalCase via plain EF defaults. Rule: match the target database's own naming convention — snake_case is Postgres-only, not a repo-wide choice. *(Corrected: Auth doesn't call the helper; no-rewriter callers are 3, not 4.)* | `rg -n "UseEntityTypeNamesAsTables\|SnakeCaseNameRewriter" src -g "!**/Migrations/**"` | IMPRECISE (corrected) |
| namingx-2 | `UseEntityTypeNamesAsTables` exists to normalize TPH root-type naming (walk to the root base type, name the table after it), with the case rewriter as an optional parameter — not to pick a case style; Auth's PascalCase tables come from pure EF defaults with no helper call at all. | read `ModelBuilderExtensions.cs:19-32` + Auth absence from the namingx-1 grep | CONFIRMED |
| namingx-3 | HTTP JSON casing is never overridden in any service — every JsonOptions call sets only `PropertyNameCaseInsensitive = true` (+ string-enum converter), keeping ASP.NET's camelCase web default for the Vue frontend; only Blocks.Hasura sets explicit naming policies (STJ CamelCase + Newtonsoft SnakeCase) because it talks to Hasura/Postgres. Rule: don't fight the framework default for the frontend wire format. | `rg -n "PropertyNamingPolicy\|PropertyNameCaseInsensitive" src` | CONFIRMED |
| namingx-4 | Routes are REST-resource-first with a `:verb` RPC suffix for non-CRUD actions (`/articles/{id}:submit`, `…/review-reports:upload`), applied identically across Minimal APIs, FastEndpoints, and Carter — the rule rides on the URL template string, framework-agnostic by construction. | `rg -n ":submit\|:approve\|:reject\|:upload\|:download\|:accept\|:decline" src/Services` | CONFIRMED |

**Evidence excerpts**
- namingx-1: `ArticleHubDbContext.cs:29` with `SnakeCaseNameRewriter`; Submission `:26`/Review `:30`/Production `:56` bare; Auth absent from results.
- namingx-2: `ModelBuilderExtensions.cs:19-32` — TPH root walk, `tableName = rootType.ClrType.Name`, rewriter optional; Auth snapshot tables `Persons`, `RefreshTokens` (EF defaults).
- namingx-3: all six API DI files set only `PropertyNameCaseInsensitive = true`; `HasuraRegistration.cs:27` (STJ CamelCase) + `:65` (Newtonsoft SnakeCase).
- namingx-4: Submission (Minimal API) `:submit/:reject/:approve/…:upload/:download`; Production (FastEndpoints) `…files:upload`, `…:request`, `…:approve`; Review (Carter) `…:accept/:decline`, `…review-reports:upload`.
- *(namingx source-P3, the MassTransit snake_case formatter, was excluded as a duplicate of boundaries-5.)*

## Dimension: Cross-Cutting Infrastructure *(run 3)*

| Id | Pattern (choice + rule it reveals) | Evidence command | Verdict |
|----|-----------------------------------|------------------|---------|
| infra-1 | `HttpContextProvider` implements two segregated interfaces — `IClaimsProvider` (Blocks.Core) and `IRouteProvider` (Blocks.AspNetCore) — instead of one fat "current request" service; all three AssignUserId mechanisms inject only `IClaimsProvider`. Rule: inner layers depend on the narrow capability, never on HttpContext. (In-code "interface segregation" insight comment marks it deliberate.) | `rg -n "IClaimsProvider\|IRouteProvider" src/BuildingBlocks` | CONFIRMED |
| infra-2 | `RequestContext` is a plain scoped POCO populated once by `RequestContextMiddleware` and read downstream via DI (LoggingBehavior, RequestDiagnosticsMiddleware) — never re-derived from `HttpContext.Items`. Correlation-id priority: incoming header → `Activity.Current` trace id → Kestrel `TraceIdentifier`. | `rg -n "RequestContext\|CorrelationId" src/BuildingBlocks` | CONFIRMED |
| infra-3 | `IAuditableAction<TActionType>` uses a C# default interface method to derive the `Action` string from the generic enum — audit-string behavior for free with no base class ("default implementation in interfaces" insight comment marks it intentional). | `rg -n "default implementation in interfaces\|IAuditableAction" src/BuildingBlocks/Blocks.Domain` | CONFIRMED |
| infra-4 | Options bind through two shared Blocks.Core helpers — `AddAndValidateOptions<T>()` (section-name = type-name, DataAnnotations + ValidateOnStart, fail-fast at startup) and `GetSectionByTypeName<T>()` — 27 call sites; zero ad-hoc `services.Configure<JwtOptions/RabbitMqOptions/TenantOptions/EmailOptions>` calls anywhere. Rule: required config fails at startup, not first use. | `rg -c "AddAndValidateOptions<\|GetSectionByTypeName<" src` + zero-hit ad-hoc Configure grep | CONFIRMED |
| infra-5 | Validator bases are framework-specific (the endpoints-4 split) — and so is the **message vocabulary**: `Blocks.Core.FluentValidation.Extensions` (NotEmptyWithMessage, WithMessageForInvalidId) is used only by the MediatR services; Production's FastEndpoints validators use a separate local `ValidatorsMessagesConstants`. `BaseValidator<T>` adds a real not-null request guard; its failure-logging hook is a commented-out stub. Rule: validation infrastructure splits along the framework line all the way down — there is no shared cross-framework vocabulary. *(Corrected: original "both lean on shared extensions" refuted.)* | read `Production/_Shared/Validators.cs` + `rg -n "NotEmptyWithMessage\|WithMessageForInvalidId\|ValidatorsMessagesConstants" src/Services/Production` | IMPRECISE (corrected) |

**Evidence excerpts**
- infra-1: `HttpContextProvider.cs:16-18` insight comment + dual interface; `AssignUserIdBehavior.cs:8`, `AssignUserIdFilter.cs:7`, `AssignUserIdPreProcessor.cs:13` all inject `IClaimsProvider` only.
- infra-2: `RequestContext.cs:3` (no HttpContext ref); `RequestContextMiddleware.cs:17` populate; `ResolveCorrelationId :41 header → :45 Activity → :50 TraceIdentifier`; readers `RequestDiagnosticsMiddleware.cs:13`, `LoggingBehavior.cs:15`.
- infra-3: `IAuditableAction.cs:17-18` — default interface method `Action => ActionType.ToString()`.
- infra-4: 27 call sites; ad-hoc `services.Configure<…Options>` → zero hits (non-vacuous).
- infra-5: `BaseValidator.cs:20-24` not-null guard via `ValidatorsMessagesConstants.NotNull`; `:9-16` commented `//Log.Error` stub; Production hits for the shared extensions → zero.

---

## Translation Dictionary

Omitted — self-reference mode (reference stack = consuming stack). Built only for external-reference runs.

## Files-Read Evidence List

Key files grounding the rows above (repo-relative): `src/BuildingBlocks/Blocks.Domain/Entities/AggregateRoot.cs`, `Blocks.Domain/DomainException.cs`, `Blocks.Exceptions/*.cs`, `Blocks.Core/Guard.cs`, `Blocks.Core/GuardExtensions.cs`, `Blocks.Core/Cache/MemoryCacheExtensions.cs`, `Blocks.EntityFrameworkCore/Repositories/Repository.cs`, `Blocks.EntityFrameworkCore/EntityConfigurations/EntityConfiguration.cs`, `…/AuditedEntityConfiguration.cs`, `Blocks.AspNetCore/Middlewares/GlobalExceptionMiddleware.cs`, `Blocks.Messaging/MassTransit/DependencyInjection.cs`, `Articles.Grpc.Contracts/Journals/JournalContracts.cs`, `Articles.Integration.Contracts/Articles/ArticleApprovedForReviewEvent.cs`, all `src/Services/*/*.csproj` (22), all `src/Services/*/CLAUDE.md` (6), `src/Services/*/[Svc].Persistence/DependencyInjection.cs`, `Submission.Domain/{Entities,Behaviors}/Article.cs`, `Submission.Domain/IArticleStateMachine.cs`, `Submission.Domain/ValueObjects/EmailAddress.cs`, `Journals.Persistence/JournalDbContext.cs`.

Run 2 additions: `Articles.Abstractions/ArticleCommandBase.cs`, `Articles.Abstractions/Enums/UserRoleType.cs`, `Articles.Security/{Role,Extensions,ConfigureAuthentication}.cs`, `Blocks.MediatR/Behaviors/*.cs`, `Blocks.MediatR/Abstractions/{ICommand,IQuery}.cs`, `Blocks.FastEndpoints/AssignUserIdPreProcessor.cs`, `Blocks.AspNetCore/Filters/AssignUserIdFilter.cs`, `Blocks.Core/MaxLength.cs`, all `src/Modules/*/*/*.csproj`, `Modules/ArticleTimeline/ArticleTimeline.Persistence/DependencyInjection.cs`, the four `ArticleAccessChecker.cs` / consumer files in Review, Production, ArticleHub, Journals, `src/Directory.Packages.props` + `src/Services/Review/Review.API/Directory.Packages.props`, all 17 `GlobalUsings.cs`, `Review.API/FileStorage/*.cs`.

## Run Report — run1-20260705

- **Mode:** self-reference (portability collapsed to `portable`; translation dictionary omitted).
- **Dimensions (6, user-tailored):** Layering & BuildingBlocks, Domain Modeling, Boundaries & Messaging, Endpoint & Feature Slicing, Persistence, Error Handling.
- **Stats:** 30 patterns mined → 1 merged (cross-dimension duplicate) → 29 consolidated → **28 CONFIRMED, 1 IMPRECISE (corrected), 0 WRONG (invented virtues)**. Survival rate 29/29 (with one scope correction).
- **Gate notes:** every verdict carries re-execution output (none dropped). The skeptic invalidated one vacuous evidence grep (layering-4) and re-proved the claim with a stronger check — recorded in that row's audit note.
- **Self-reference cross-check:** no `docs/tech-debt/` registry exists in this repo — the confirmed-virtue vs confirmed-debt contradiction check could not run. If a debt mine (`mine-verify-repo`) runs later, re-check domain-2 and layering-5 (both carry known inconsistencies) against its findings.
- **Candidate dimensions for run 2** (from extractor gap candidates, deduped):
  1. **Application/CQRS pipeline** — MediatR behaviors (AssignUserId, Validation, Logging), command/handler shape, throw-vs-Result error model.
  2. **Module composition** — pluggable Contracts+implementations modules (EmailService, FileService, ArticleTimeline); Production's asymmetric module use.
  3. **Consumer/projection patterns** — DbContext-in-consumer, inline `Adapt<T>()` DTO→entity, inbox-lite idempotency (`EnsureNotExistsOrThrowAsync`); the Submission↔Review file-transfer inside a consumer (possible boundary smell — verify, don't assume).
  4. **Conventions & naming** — snake_case Postgres rewriter, GlobalUsings, `_camelCase`, MaxLength constants.
  5. **Security & identity wiring** — `Articles.Security` policy extensions, JWT config, `RequireRoleAuthorization`, `AssignUserId` plumbing across three frameworks.
- **Debt-shaped findings (route to `mine-verify-repo`, not this registry):** Auth.API references Domain+Persistence despite having an Application layer; unused multitenancy stack (TenantDbContext et al.) with `TenantRepositoryBase.GetById` throwing raw `HttpException` from persistence; commented-out `AddInterceptors` block in Production.Persistence DI; zero RowVersion opt-ins despite the concurrency-token seam.

## Run Report — run2-20260705

- **Mode:** self-reference (unchanged). Same commit as run 1 (`cd4d0b1`) — run-1 row refresh is a no-op re-stamp.
- **Dimensions (5, from run-1 gap candidates):** Application/CQRS Pipeline, Module Composition, Consumer/Projection, Conventions & Naming, Security & Identity Wiring.
- **Stats:** 25 patterns mined → 1 merged (appcqrs P4 + security P4 → security-4) → 24 consolidated → **14 CONFIRMED, 10 IMPRECISE (corrected), 0 WRONG**. Full duplicates vs run 1: 0 (appcqrs-3 partially overlaps errors-1/errors-3 but carries a new, now-proven zero-monad absence claim).
- **Gate notes:** every verdict carries re-execution output (none dropped). The high IMPRECISE rate (10/24 vs run 1's 1/29) traces to extractors leaning on "always/never/every" wording that counterexample checks broke — the corrected statements are in the rows; the patterns themselves are real.
- **Candidate dimensions for run 3** (consolidated gap candidates): producer side of the domain→integration handoff (`PublishIntegrationEventOn*` + `AddTimelineEventHandler` + `DomainEventPublisher` variants + Production's dispatch-only MediatR); sync-gRPC vs async-event data sharing (`IPersonService`/`IJournalService` from command handlers); file-storage abstraction (`IFileService<TOptions>` + `FileServiceFactory` pluggability); cross-service naming consistency (snake_case split: ArticleHub EF rewriter vs MassTransit formatter vs Hasura JSON); cross-cutting infra (`HttpContextProvider` dual-interface segregation, `IAuditableAction.IsAuthenticated` default interface method, validation-across-frameworks `BaseValidator<T>`).
- **Debt-shaped findings (route to `mine-verify-repo`):** nested `Review.API/Directory.Packages.props` (nearest-wins CPM override — likely correctness bug); `FileStorage.MinIO` referenced by no service (dead implementation); `SendGridEmailService.cs` malformed interpolated log strings; `JwtOptions` commented `//todo introduce SigningCredentials`; Production access-checker comment says "editorial admin" over a POF check; no `CLAUDE.md` under `src/Modules/*`.

## Run Report — run3-20260705

- **Mode:** self-reference (unchanged). Same commit as runs 1-2 (`cd4d0b1`) — prior rows re-stamped without re-verification (zero git delta).
- **Dimensions (5, from run-2 gap candidates):** Domain-Event Publication Plumbing, Cross-Service Data Access, File-Storage Abstraction, Cross-Stack Naming Transforms, Cross-Cutting Infrastructure.
- **Stats:** 25 patterns mined → 2 excluded as full duplicates of existing rows (filestorage byte-migration → consumers-4; MassTransit snake_case formatter → boundaries-5) → 23 judged → **20 CONFIRMED, 2 IMPRECISE (corrected), 1 WRONG (invented virtue — the registry's first)**.
- **The WRONG kill (eventpub-3):** claim — "domain events are never dispatched manually from application code; dispatch is exclusively via the EF SaveChangesInterceptor." Refuting output: `Journals/CreateJournalEndpoint.cs:30`, `UpdateJournalEndpoint.cs:24`, `UpdateSectionEndpoint.cs:24`, and `Auth/CreateUserEndpoint.cs:43` all publish domain events directly from endpoints via FastEndpoints `PublishAsync`; interceptors are registered only in Production/Submission/Review. The true (weaker) rule: EF write-side services dispatch via interceptor; Redis-backed Journals and Auth's Identity flow publish manually. The genuine change-tracker mechanic already lives in persistence-3.
- **Gate notes:** every verdict carries re-execution output (none dropped). All three failures (1 WRONG, 2 IMPRECISE) again trace to "always/never/exclusively/both" wording — consistent with run 2's finding.
- **Coverage judgment:** the remaining gap candidates are debt-shaped or micro-conventions; no further virtue-mining run is recommended. The registry is complete at 75 rows / 16 dimensions for this commit.
- **Debt-shaped findings (route to `mine-verify-repo`; cumulative with runs 1-2):** two dead gRPC helper siblings (`AddConfiguredGrpcClient1` with abandoned Polly retry, `AddGrpcServiceSingleton2` builds-and-discards a channel); `FileStorage.MinIO` prototype (`// todo - not tested`), unregistered; ArticleHub's commented-out `UseSnakeCaseNamingConvention()` with in-code incompatibility note; Production `BaseValidator` failure-logging hook is a commented-out stub.

## Changelog

- `2026-07-05` — run1-20260705 (commit `cd4d0b1`): initial registry created. 6 dimensions, 29 rows (28 CONFIRMED, 1 IMPRECISE-corrected, 0 WRONG). Self-reference mode. boundaries-3 merged into layering-4 at consolidation (id retired, never reused).
- `2026-07-05` — run2-20260705 (commit `cd4d0b1`, same as run 1): 5 new dimensions appended, 24 rows (14 CONFIRMED, 10 IMPRECISE-corrected, 0 WRONG). appcqrs-4 merged into security-4 at consolidation (id retired, never reused). Run-1 rows re-stamped without re-verification (zero git delta). Registry total: 53 rows across 11 dimensions.
- `2026-07-05` — run3-20260705 (commit `cd4d0b1`, same as runs 1-2): 5 new dimensions appended, 22 rows (20 CONFIRMED, 2 IMPRECISE-corrected). 1 pattern killed as WRONG at the gate (eventpub-3 — see run-3 report; id retired, never reused). 2 excluded as duplicates (→ consumers-4, → boundaries-5). Prior rows re-stamped without re-verification. Registry total: **75 rows across 16 dimensions**. Virtue mining for this commit is complete.
- `2026-07-05` — security-4 verdict flip CONFIRMED → IMPRECISE (corrected): the "identical only-if-present guard" clause was refuted during the skill-building pass — `AssignUserIdBehavior` (MediatR) and `AssignUserIdFilter` (Minimal APIs) use `TryGetUserId()` and skip when absent; `AssignUserIdPreProcessor` (FastEndpoints) calls `GetUserId()`, which throws. Re-verified against source at `Blocks.MediatR/Behaviors/AssignUserIdBehavior.cs:16`, `Blocks.AspNetCore/Filters/AssignUserIdFilter.cs:15`, `Blocks.FastEndpoints/AssignUserIdPreProcessor.cs:14`. Statement corrected in place; row retained per registry invariants.
