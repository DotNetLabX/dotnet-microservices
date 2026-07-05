---
name: create-service
description: Scaffold a new microservice skeleton under src/Services/ — folder tree, csproj references, GlobalUsings, per-layer DependencyInjection, Program.cs, and port/launchSettings — with correct Clean Architecture layering and BuildingBlocks composition. Use when standing up a brand-new service, before writing any aggregate, feature, or endpoint. Not for adding a feature to an existing service.
user-invocable: true
---

# Create Service

Scaffold an **empty, buildable** service skeleton. This produces structure and wiring only — no aggregates, value objects, features, endpoints, DbContext body, or entity configurations. Those come after, once the skeleton compiles.

Full copy-paste template sets (every csproj, GlobalUsings, DependencyInjection, Program.cs, launchSettings — by variant) live in `references/templates.md`. This file is the recipe and the decision rules; pull the matching block from there at each phase.

## Phase 0 — Fix the shape

Every service is defined by three per-service choices plus two structural variant axes. Nail all five before writing a file.

Each existing service declares the three choices in its own `CLAUDE.md` — endpoint framework, database, port *(endpoints-1)*. Write `src/Services/{Service}/CLAUDE.md` first (framework, db, port, one-line domain summary); it is the source of truth the rest of this recipe reads back.

**Fork A — Application layer? (layering-5)**
Create a `{Service}.Application` project **only** when the service has cross-feature shared state: a MediatR request pipeline, shared mappings, a state-machine factory, or cross-cutting handlers. If every feature is self-contained (handler logic sits in the endpoint, nothing shared across features), skip it — the API references Domain and Persistence directly. Precedent: with Application = Submission, Review, Auth; without = Journals, ArticleHub, Production.

**Fork B — Persistence shape? (persistence-4)**
- **EF Core** — SQL Server for a write-side service, Npgsql/PostgreSQL for a read/projection side. Full machinery: a `{Service}DbContext` over the EF base, entity configurations, interceptors, `Repository<>` unit-of-work.
- **Redis.OM** — a document store with **none** of the EF machinery: no DbContext base, no interceptors, no repository-UoW. Just a thin `Repository<>` over `IRedisCollection` and a `{Service}DbContext` wrapping `RedisConnectionProvider`. Precedent: Journals.

Resulting shape drives which template blocks you pull:

| Choice | Options | Sets the |
|---|---|---|
| Endpoint framework | FastEndpoints / Minimal+MediatR / Carter | API csproj framework block, Program.cs middleware tail |
| Database | SQL Server / Postgres / Redis | Persistence csproj + DI, DbContext base |
| Application layer | present / absent | csproj graph, DI composition |
| Port | next free 44xx pair | launchSettings |

## Phase 1 — Folder tree

Create under `src/Services/{Service}/`. Projects depend **inward only**: `API → Application → Domain`, `API/Application → Persistence → Domain`; every layer additionally references the BuildingBlocks it needs. See `references/templates.md` §1 for the trees (with-Application, without-Application, and the read-model/projection variant). A **read-model service** (ArticleHub-style, populated only by integration-event consumers) has no Application layer and organizes the API around `{Aggregate}/Consumers/` plus query-only endpoints; its Domain holds plain data-bag entities (public get/set, no behavior — consumers-5), not aggregates.

## Phase 2 — csproj files with references

One csproj per project. Pull the exact block from `references/templates.md` §2. The binding rules:

- **Domain references nothing outward** *(layering-1)*. It takes only inward-free BuildingBlocks: `Blocks.Domain` (EF services) or `Blocks.Redis` (Redis services), plus the shared kernel `Articles.Abstractions`, and `Articles.Grpc.Contracts` only if the domain speaks a foreign contract. Never Persistence, Application, or API.
- **Never reference another service's projects** *(layering-4)*. Cross-service contact happens only through the BuildingBlocks contract packages `Articles.Grpc.Contracts` (sync) and `Articles.Integration.Contracts` (async). If a template line points at `..\..\{OtherService}\...`, it is wrong.
- **BuildingBlocks freely, Modules selectively** *(modules-5)*. BuildingBlocks (`Blocks.*`, `Articles.*`) are technical primitives — reference whichever you need. Modules (`EmailService.*`, `FileService.*`, `ArticleTimeline`) are business capabilities — add one **only** when the service actually uses it. A service that sends no mail references no `EmailService`.
- **Reference only the specific blocks you need** *(layering-2)*. The BuildingBlocks graph is shallow and single-concern; do not pull a block "just in case," and there is no monolithic common lib to lean on.
- **Central Package Management — zero inline versions** *(conventions-1)*. Every `PackageReference` is bare (`Include="..."` with no `Version=`). Versions live in `src/Directory.Packages.props`; if you introduce a package that isn't pinned there yet, add a `PackageVersion` entry to that file in the same pass.

Each csproj block in the templates separates **core references (always)** from **optional, add per capability** — the second list is where modules-5 is applied.

## Phase 3 — GlobalUsings

Each layered project ships its own `GlobalUsings.cs` with curated `global using` lines, grouped by a header comment (`// Third-party libraries` / `// Internal libraries` / `// Domain`) — the house habit *(conventions-2)*. Curate per project; do not copy one project's usings wholesale into another. Templates in `references/templates.md` §3.

## Phase 4 — DI wiring

Each layer exposes a `DependencyInjection.cs` with one `Add{Layer}Services(this IServiceCollection, IConfiguration)` extension; Program.cs composes them. Blocks in `references/templates.md` §4. The binding rules:

- **Options fail-fast** *(infra-4)*. Bind every options type through `AddAndValidateOptions<T>()` (section name equals the type name; DataAnnotations validated at startup) inside a `ConfigureApiOptions` method. Resolve values you need at registration time (e.g. gRPC service URLs) with `GetSectionByTypeName<T>()`. Never hand-roll `services.Configure<TOptions>` for required config — required config must fail at startup, not first use.
- **Wire the request context** *(infra-2)*. Register `RequestContext` as scoped and `HttpContextProvider` behind both `IClaimsProvider` and `IRouteProvider`. The scoped `RequestContext` POCO is populated once by the middleware (Phase 5) and read downstream via DI — never re-derived from `HttpContext`.
- **Persistence EF** registers the interceptor(s), the `DbContext` with its provider and interceptors, `Repository<>` (plus `AddDerivedTypesOf(typeof(Repository<>))` for the per-aggregate repos), and any cached repositories or hosted seed loaders. The `{Service}DbContext` class itself is a **stub** at scaffold time — the correct base (`ApplicationDbContext<{Service}DbContext>(options, cache)`) and `OnModelCreating` wiring, with an empty DbSet region; the DbSets and configurations fill in with the aggregates (template §4e). Auth is the exception — it derives from `IdentityDbContext<User, Role, int>`, not the base.
- **Persistence Redis** registers `RedisConnectionProvider`, `IConnectionMultiplexer`, the `{Service}DbContext`, and `Repository<>` — and nothing EF.
- **Application** (when present) registers MediatR with the three open behaviors **in order** — `AssignUserIdBehavior` then `ValidationBehavior` then `LoggingBehavior` — plus validators, Mapster, MassTransit, and `IDomainEventPublisher`. When there is no Application project, fold these into the API's `AddApiServices` (the without-App services do exactly this).

## Phase 5 — Program.cs

Compose the layers, then run the shared middleware pipeline in this order: Swagger → routing → `GlobalExceptionMiddleware` → `RequestContextMiddleware` → `RequestDiagnosticsMiddleware` → authentication → authorization, then the framework tail (`MapAllEndpoints()` for Minimal APIs, `UseCustomFastEndpoints()` for FastEndpoints, `MapCarter()` for Carter). `RequestContextMiddleware` is what populates the scoped `RequestContext` from Phase 4 *(infra-2)* — it must be in the pipeline. Finish with data init: `app.Migrate<{Service}DbContext>()` for EF (or `UseRedis()` for Redis) and a dev-only seed. Templates in `references/templates.md` §5.

## Phase 6 — Ports and launchSettings

Assign the service a port pair from the 44xx block: service number N gets HTTPS `445N` / HTTP `440N`, set as `applicationUrl` in `Properties/launchSettings.json`; the Docker profile maps container 8080/8081. The next free slot after Production (4406) is **4407 / 4457**. Template in `references/templates.md` §6.

## Phase 7 — Register and containerize

The skeleton isn't usable until the solution and the container tooling know about it. Four steps, templates in `references/templates.md` §7:

1. **Add the projects to the solution.** `dotnet sln src/Articles.sln add` for each of the (three or four) projects — Domain, Persistence, [Application,] API.
2. **Add a Dockerfile** at `{Service}.API/Dockerfile`. The repo's Dockerfiles are `ARG`-parameterized by `APP_NAME`/`PROJECT_PATH`; the build context is `src/` and the runtime image exposes 8080/8081. (Verified note: the live Dockerfiles pin the SDK image at `sdk:8.0` while targeting `net9.0` — use `sdk:9.0` in a new service so restore/publish match the target framework.)
3. **Add a docker-compose service block** to `src/docker-compose.yml` with `depends_on` listing only the infra this service actually uses (SQL Server / Postgres / Redis / RabbitMQ / Mongo). Match the existing form: a plain list for most; the long `condition: service_healthy` form only where the dependency has a healthcheck (Postgres).
4. **Register in `src/docker-compose.dcproj`.** Add a `ProjectReference` to the API csproj in its `ItemGroup` (the file carries an `Add more if needed` marker).

Wiring the service **behind the YARP gateway** (`src/ApiGateway`) — its route + cluster and the port-drift reconciliation — is a separate step at the gateway boundary, not part of the skeleton.

## Verify the skeleton

After scaffolding, prove the binding rules mechanically (run from `src/`, adjust `{Service}`):

```bash
# layering-1 — Domain references nothing outward (expect zero hits)
grep -iE "Persistence|Application" Services/{Service}/{Service}.Domain/{Service}.Domain.csproj

# layering-4 — no reference to another service (expect zero hits)
grep -oE 'Include="\.\.\\\.\.\\[^"]+"' Services/{Service}/*/*.csproj | grep -viE "{Service}\.|BuildingBlocks|Modules"

# conventions-1 — no inline package versions in the new csproj (expect zero hits)
grep -rn 'Version=' Services/{Service} --include=*.csproj
```

Then confirm the solution builds (`dotnet build`), each layer has both a `GlobalUsings.cs` and a `DependencyInjection.cs`, and the port pair is unique across `Services/*/*/Properties/launchSettings.json`.

## What this skill does NOT do

- **No business classes.** Aggregates, value objects, and domain events come next via **create-aggregate**; features, endpoints, validators, and handlers via **create-feature-slice**. This skill stops at the DbContext **stub** (correct base + `OnModelCreating`); the populated DbSets and entity configurations land with the aggregates.
- **No cross-service contracts.** It references the contract BuildingBlocks the service needs but does not author new gRPC or integration-event contracts.
- **No gateway routing.** Phase 7 containerizes and registers the service, but wiring it behind the **YARP API gateway** (`src/ApiGateway`) — adding its route and cluster and reconciling the known port drift — is a separate step at the gateway boundary.
- **No domain rules.** It never encodes an invariant in a csproj, DI, or Program.cs.
