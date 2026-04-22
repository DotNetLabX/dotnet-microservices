# Articles Application — DotNetLabX

Article lifecycle system: draft → submission → review → production → publish. ArticleHub aggregates latest state across services via integration events. Built for a Udemy course on DDD + Vertical Slice + CQRS, evolving from modular monolith to microservices.

## Repo structure

```
src/
├── BuildingBlocks/
│   ├── Articles.Abstractions        — Shared article interfaces & enums (ArticleStage, IArticleAction, ArticleCommandBase)
│   ├── Articles.Grpc.Contracts      — gRPC code-first contracts ([ServiceContract]/[ProtoContract])
│   ├── Articles.Integration.Contracts — MassTransit integration event records
│   ├── Articles.Security            — JWT config, role constants, authorization handlers
│   ├── Blocks.Core                  — Guards, IClaimsProvider, caching, extensions, RequestContext
│   ├── Blocks.Domain                — Entity<T>, AggregateRoot<T>, ValueObject, IDomainEvent
│   ├── Blocks.EntityFrameworkCore   — ApplicationDbContext, TenantDbContext, design-time factory
│   ├── Blocks.Exceptions            — HttpException hierarchy (BadRequest/NotFound/Unauthorized)
│   ├── Blocks.AspNetCore            — AssignUserIdFilter, HttpContextProvider
│   ├── Blocks.FastEndpoints         — DomainEventPublisher, pre-processors, custom config
│   ├── Blocks.MediatR               — MediatR-based DomainEventPublisher
│   ├── Blocks.Messaging             — RabbitMQ/MassTransit config (RabbitMqOptions)
│   ├── Blocks.Redis                 — Redis.OM Repository<T>, Entity base
│   └── Blocks.Hasura                — Hasura GraphQL metadata management
├── Modules/
│   ├── ArticleTimeline              — Tracks article stage transitions via domain event handlers
│   ├── EmailService                 — Pluggable: Empty (dev) / SendGrid / SMTP
│   └── FileService                  — Pluggable: AzureBlob / MongoGridFS / MinIO
├── Services/
│   ├── Auth                         — Users, JWT, Person gRPC server (port 4401)
│   ├── Journals                     — Journal/section CRUD, Journal gRPC server (port 4402)
│   ├── ArticleHub                   — Read-only aggregate view, event consumers (port 4403)
│   ├── Submission                   — Article creation & submission workflow (port 4404)
│   ├── Review                       — Peer review process (port 4405)
│   └── Production                   — Post-acceptance typesetting & assets (port 4406)
├── ApiGateway/                      — YARP reverse proxy
└── docker-compose
```

## Core principles

**Architecture style:** Clean Architecture layering (`.API`, `.Application`, `.Domain`, `.Persistence`) with Vertical Slice feature organization inside `.Application/Features/`. DDD lives in `.Domain`. CQRS via MediatR. These are not mutually exclusive — the layering is structural, the slicing is organizational. Skills have the details.

## Naming conventions

- Private fields: `_camelCase`
- Public members/types: `PascalCase`
- Locals/parameters: descriptive `camelCase`
- No abbreviated names: never `req`, `cmd`, `res`, `ops`, `_q`, `_m`

## Architecture guardrails

- No service layer classes (`ArticleService`). Use: domain methods, handlers, repositories, gRPC clients, infra helpers
- No interfaces without multiple implementations for repositories
- No god folders (`Services/`, `Helpers/`, `Utils/`)
- No inventing patterns — use what exists in the codebase
- No bypassing domain rules via EF configs or endpoints
- Domain events = within service boundary. Integration events = cross-service
- Default to module. Microservice only when deployment/scaling/ownership demands it

## Commands

```bash
dotnet ef migrations add Name -p Services/{Svc}/{Svc}.Persistence -s Services/{Svc}/{Svc}.API
dotnet ef database update -p Services/{Svc}/{Svc}.Persistence -s Services/{Svc}/{Svc}.API
```

## Port conventions

Services use 4400–4499. HTTP/HTTPS pairs: (4401/4451), (4402/4452), etc.

## Feature plans

Implementation plans live in `plans/{ServiceOrModule}/{FeatureName}.md`, grouped by the service or module that owns the work. Created by Architect, consumed by Developer.

## Compaction rules

When compacting, preserve: domain model decisions, current file changes, which files were already read.

## File re-read rule

After reading a file, trust your context. Do not re-read files unless told they changed or you explicitly modified them. After editing a file, do not read it back to verify.

## .claudeignore

Exists at repo root — excludes bin/, obj/, Migrations/, .vs/, *.lock, docker-compose.override.yml.

## Agent auto-loading
If the CC_AGENT environment variable is set, immediately be agents/{CC_AGENT}.md 
and follow that role for the entire session. Do this before responding to any prompt.

## Agent loading

"Load/be {agent}" → read `ai-coding/agents/{Agent}.md`, follow its instructions silently. Never show file contents unless explicitly asked.

## Skills

Available in `.claude/skills/`. Check them when working on features — they contain patterns, workflows, and templates for this codebase.
