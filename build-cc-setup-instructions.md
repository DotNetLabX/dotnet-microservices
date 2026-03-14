# Instructions: Build the Claude Code Agent + Skills Setup

You are building the complete Claude Code configuration for the DotNetLabX Articles microservices project. This includes agent role files, skills, CLAUDE.md files, and folder structure.

Read these instructions fully before starting. Then execute each section in order.

## Context

This project uses a two-terminal setup:
- **Terminal 1 — Architect** (`claude --model opus`): discusses architecture, plans features, writes plan files. Never writes code.
- **Terminal 2 — Developer** (`claude --model opusplan`): implements features following plans. Doesn't debate architecture.

Skills are used instead of convention files. Skills load on-demand (progressive disclosure), saving context tokens compared to always-loaded convention files.

The project has 6 microservices (Submission, Review, Production, Auth, Journals, ArticleHub) with different endpoint frameworks per service. The architecture uses DDD, vertical slice, CQRS, gRPC for sync calls, MassTransit integration events for async communication.

## Reference files

Before starting, read these files to understand the codebase:
1. The existing `CLAUDE.md` at the repo root (the large architecture reference — this is the source of truth for all patterns and code examples)
2. `cc-workflow-decisions.md` in the project files (for terminal setup, session management, cost efficiency decisions)

## What to build

### 1. Root CLAUDE.md (create/update at repo root)

Keep this **under 150 lines**. It loads on every single message in both terminals, so only universal, always-needed content goes here.

Content to include:
- Project description (2-3 lines): what this system is, article lifecycle overview
- Repo structure (the high-level folder tree from the existing CLAUDE.md)
- BuildingBlocks overview (1-2 lines per block — both agents need this for orientation)
- Core principles — just names: DDD, Vertical Slice, CQRS (not explanations)
- Naming conventions: `_camelCase` private fields, `PascalCase` public, descriptive `camelCase` locals, no abbreviated names (`req`, `cmd`, `res`, `ops`)
- Architecture guardrails (the "don't" list): no service layers, no interfaces without multiple implementations, no god folders, no inventing new patterns
- Build/test commands: `dotnet build`, `dotnet test`, EF migration command template
- Port conventions: 4400–4499 range
- Feature plans location: `plans/{FeatureName}.md`
- Compaction rules: when compacting, preserve domain model decisions and current file changes
- "Don't re-read files" rule: "After reading a file, trust your context. Do not re-read files unless told they changed or you explicitly modified them. After editing a file, do not read it back to verify."
- `.claudeignore` reference (mention it exists, don't inline it)
- Mention that skills are available in `.claude/skills/` — CC should check them when working on features

Content to **NOT** include (these go in skills or service CLAUDE.md):
- Detailed patterns with code examples (those go in skills)
- Service-specific details (those go in service-level CLAUDE.md)
- Endpoint framework details
- gRPC contract patterns
- Integration event patterns
- Domain event dispatch details

### 2. Service-level CLAUDE.md files

Create one for each service. These auto-load when CC works in that service's folder. Each should be **under 50 lines**.

**`Services/Submission/CLAUDE.md`:**
- Endpoint framework: Minimal APIs + MediatR
- DB: SQL Server + MongoDB (GridFS for files)
- Aggregate: Article (partial class split — state in Article.cs, behavior in Behaviors/Article.cs)
- Key entities: Asset, ArticleActor, ArticleAuthor
- Endpoint registration pattern: static class with `Map()` extension method, composed via `EndpointRegistration.MapAllEndpoints()`
- MediatR pipeline behaviors: AssignUserIdBehavior, ValidationBehavior, LoggingBehavior
- File storage: MongoGridFS (singleton)
- Note: Application layer is separate project (`Submission.Application`)

**`Services/Review/CLAUDE.md`:**
- Endpoint framework: Carter + MediatR
- DB: SQL Server + MongoDB (GridFS for files, dual storage with factory)
- Aggregate: Article (same partial class pattern)
- Carter pattern: `ICarterModule` with `AddRoutes(IEndpointRouteBuilder)`
- MediatR pipeline behaviors: same three as Submission
- File storage: MongoGridFS (singleton + scoped with factory)
- Note: Application layer is separate project (`Review.Application`)

**`Services/Production/CLAUDE.md`:**
- Endpoint framework: FastEndpoints (no MediatR)
- DB: SQL Server + Azure Blob Storage (files)
- Aggregate: Article (same partial class pattern)
- FastEndpoints pattern: attribute-based `[HttpPost]`, `HandleAsync` contains logic directly
- Validators extend `BaseValidator<T>` (custom, extends FastEndpoints `Validator<T>`)
- Uses `TransactionalDispatchDomainEventsInterceptor` (wraps save + dispatch in transaction)
- File storage: Azure Blob
- Note: No separate Application layer — features live in API project

**`Services/Auth/CLAUDE.md`:**
- Endpoint framework: FastEndpoints (partial MediatR)
- DB: SQL Server with ASP.NET Identity
- Aggregate: User (extends `IdentityUser<int>`, implements `IAggregateRoot` manually — cannot extend `AggregateRoot<T>`)
- Also has: Person entity, RefreshToken
- JWT configuration in `Articles.Security`
- Role constants in `Articles.Security/Role.cs`
- gRPC server: PersonGrpcService (exposes person data to other services)
- Note: No separate Application layer

**`Services/Journals/CLAUDE.md`:**
- Endpoint framework: FastEndpoints (no MediatR)
- DB: Redis (Redis.OM) — NOT SQL Server
- Entity: Journal (extends `Blocks.Redis.Entity`, NOT `AggregateRoot`)
- Redis.OM decorators: `[Document]`, `[Indexed]`, `[Searchable]`
- Repository: `Repository<T>` from Blocks.Redis (not EF Core)
- ID generation: Redis string increment
- gRPC server: JournalGrpcService
- Handler logic lives directly in endpoint class (combined pattern)

**`Services/ArticleHub/CLAUDE.md`:**
- Endpoint framework: Carter (partial)
- DB: SQL Server
- Read model — aggregates latest state from all services via integration event consumers
- Consumers: one per integration event, update local Article representation
- No domain logic — pure projection/read model

### 3. Agent role files

These are NOT skills, NOT subagents. They are regular markdown files loaded manually at session start with "read agents/Architect.md" or "read agents/Developer.md".

**`agents/Architect.md`:**

Update the existing Architect.md (attached below as reference) with these changes:
- Remove the `## Convention references` section at the bottom (no more `@conventions/` references)
- Update `## What you never do` to say: "Never write C# code, proto files, or configuration files. **Do** write plan files to `plans/{FeatureName}.md`"
- Add to `## Your role`: "Write implementation plans as markdown files to `plans/{FeatureName}.md`"
- Add a `## Plan workflow` section:
  1. I describe the feature
  2. You ask clarifying questions if needed (which service, command or query, cross-service impacts)
  3. You produce the plan following the Implementation plan format
  4. You save the plan to `plans/{FeatureName}.md`
  5. I review and annotate the plan with inline notes
  6. You revise the plan addressing all notes — repeat until I approve
  7. Only after approval does the Developer implement

Keep everything else from the existing Architect.md unchanged. The system knowledge sections (lifecycle, communication patterns, service boundaries, domain model patterns, vertical slice structure, cross-service contracts, guardrails, plan format) are all correct and should stay.

**`agents/Developer.md`:**

Create this new file. The Developer's identity:

- Role: implement features following plans from `plans/`. Don't debate architecture — the Architect already decided.
- First thing to do when given a feature: read the plan file at `plans/{FeatureName}.md`
- Execute plan steps one at a time. Each step = one focused implementation task.
- After each step: verify the build passes (`dotnet build`), then report what was done.
- Check the service's CLAUDE.md for which framework/patterns to use.
- Use `/create-feature`, `/create-aggregate`, `/create-grpc-contract`, `/add-integration-event` skills when they match the current step.
- Run `/simplify` after completing a full feature (all plan steps done).
- Communication style: report what you did and what's next. Don't explain architecture decisions. If a plan step is ambiguous, ask for clarification rather than guessing.
- What you never do: change architecture decisions, refactor beyond the plan scope, add patterns not in the codebase, skip build verification.

### 4. Skills

Create these in `.claude/skills/`. Each skill is a directory with a SKILL.md file and optional supporting files.

#### 4.1 Workflow skills (user-invocable, invoked with `/skill-name`)

**`.claude/skills/plan-feature/SKILL.md`:**
```yaml
---
name: plan-feature
description: Creates an architecture plan for a new feature. Use when planning a feature, endpoint, or capability before implementation. Produces a plan file in plans/ directory.
disable-model-invocation: true
---
```
Instructions: plan a feature using the Implementation plan format from agents/Architect.md. Save to `plans/$ARGUMENTS.md`. The plan must include: summary, affected services, domain model changes, file list with full paths, numbered implementation steps, cross-service impacts, migration notes, open questions.

**`.claude/skills/create-feature/SKILL.md`** (with workflows/ and templates/):
```yaml
---
name: create-feature
description: Creates a complete vertical slice feature including endpoint, command/query, handler, validator, and mappings. Use when implementing a new endpoint or feature slice. Check the service CLAUDE.md for which endpoint framework to use.
---
```
The SKILL.md should:
- Instruct CC to check the current service's CLAUDE.md for the endpoint framework
- Route to the correct workflow file based on framework
- Include workflow files in `workflows/`:
  - `CreateEndpointMinimalApi.md` — static class with `Map()`, registration in `EndpointRegistration`, `ISender` dispatch to MediatR
  - `CreateEndpointCarter.md` — `ICarterModule` with `AddRoutes()`, `ISender` dispatch to MediatR
  - `CreateEndpointFastEndpoints.md` — class extending `Endpoint<TRequest, TResponse>`, attribute-based routing, handler logic in `HandleAsync`
  - `CreateHandler.md` — MediatR `IRequestHandler<TCommand, TResponse>`, pattern: load aggregate → call domain method → save → return response
  - `CreateValidator.md` — `AbstractValidator<T>` for MediatR services, `BaseValidator<T>` for FastEndpoints services, using custom extensions (`.NotEmptyWithMessage()`, `.WithMessageForInvalidId()`)
  - `CreateMappings.md` — Mapster `IRegister` with `Register(TypeAdapterConfig)`, value object unwrapping patterns
- Include templates in `templates/` — one `.template` file per workflow, containing the skeleton code structure (NOT full implementation — just the structure CC fills in)

To build the templates: read the architecture reference file for real code examples of each pattern. Extract the structural skeleton, replace specific names with `{FeatureName}`, `{ServiceName}`, `{AggregateName}` placeholders.

**`.claude/skills/create-aggregate/SKILL.md`** (with workflows/ and templates/):
```yaml
---
name: create-aggregate
description: Creates a DDD aggregate root with behavior split, value objects, and domain events. Use when adding a new aggregate or entity to a service's domain model.
---
```
Workflow files:
- `CreateAggregate.md` — partial class extending `AggregateRoot`, state file + behavior file, backing collections pattern (`private readonly List<T> _items = new(); public IReadOnlyList<T> Items => _items.AsReadOnly()`)
- `CreateValueObject.md` — `StringValueObject` or `ValueObject` with static `Create()` factory and validation
- `CreateDomainEvent.md` — record extending service-specific `DomainEvent` base, handler naming convention: `{Action}On{Event}Handler.cs`

Special case: note that Auth's User aggregate cannot extend `AggregateRoot<T>` due to `IdentityUser` inheritance — must implement `IAggregateRoot` manually.

**`.claude/skills/create-grpc-contract/SKILL.md`** (with workflows/ and templates/):
```yaml
---
name: create-grpc-contract
description: Creates a gRPC code-first contract, server implementation, client registration, and updates callers. Use when adding service-to-service synchronous communication.
---
```
Workflow files:
- `CreateContract.md` — `[ServiceContract]`/`[OperationContract]` interface in `Articles.Grpc.Contracts/`, `[ProtoContract]`/`[ProtoMember]` request/response messages
- `CreateServer.md` — class implementing the contract interface, registered in Program.cs with `app.MapGrpcService<T>()`
- `CreateClient.md` — `services.AddCodeFirstGrpcClient<T>(grpcOptions, "ServiceKey")` registration, usage in handlers/endpoints

**`.claude/skills/add-integration-event/SKILL.md`** (with workflows/ and templates/):
```yaml
---
name: add-integration-event
description: Adds a MassTransit integration event for cross-service communication. Creates the event contract, publisher in domain event handler, and consumer in target service(s). Use when a business event needs to propagate across service boundaries.
---
```
Workflow files:
- `CreateEventContract.md` — record in `Articles.Integration.Contracts/` with DTO payload
- `CreatePublisher.md` — domain event handler that uses `IPublishEndpoint.Publish()`, naming: `Publish{IntegrationEvent}On{DomainEvent}Handler.cs`
- `CreateConsumer.md` — `IConsumer<TEvent>` in target service, idempotent, auto-discovered by MassTransit `config.AddConsumers(assembly)`

#### 4.2 Background knowledge skills (CC auto-loads when relevant)

**`.claude/skills/article-lifecycle/SKILL.md`:**
```yaml
---
name: article-lifecycle
description: Cross-service article lifecycle flow and integration event chain. Loaded when discussing or implementing features that span multiple services, involve article state transitions, or require integration events between Submission, Review, Production, and ArticleHub.
user-invocable: false
---
```
Content: the full lifecycle flow diagram (Submission → Review → Production → ArticleHub), which events trigger each transition, which service publishes and which consumes. Extract this from the Architect.md "Article lifecycle" section but add more detail about specific event names and their DTO payloads.

**`.claude/skills/service-boundaries/SKILL.md`:**
```yaml
---
name: service-boundaries
description: Service ownership, database choices, endpoint frameworks, and module vs microservice decisions. Loaded when determining which service owns a feature, understanding cross-service dependencies, or choosing the correct patterns for a specific service.
user-invocable: false
---
```
Content: the service boundaries table, which service calls which (gRPC and events), modules overview, decision criteria for module vs microservice.

**`.claude/skills/domain-patterns/SKILL.md`:**
```yaml
---
name: domain-patterns
description: DDD patterns used in this codebase — aggregates, entities, value objects, domain events, partial class behavior split, and event dispatch. Loaded when designing or implementing domain models, creating aggregates, or working with domain events.
user-invocable: false
---
```
Content: extract from architecture reference — AggregateRoot base class, Entity base, ValueObject patterns (StringValueObject, SingleValueObject), partial class convention, domain event dispatch via SaveChangesInterceptor. Include real file paths as examples, but keep concise. No full code listings — just patterns and file references.

**`.claude/skills/cqrs-patterns/SKILL.md`:**
```yaml
---
name: cqrs-patterns
description: CQRS command and query patterns including MediatR interfaces, handler structure, pipeline behaviors, and the load-mutate-save handler pattern. Loaded when implementing commands, queries, or handlers.
user-invocable: false
---
```
Content: ICommand/IQuery interfaces, handler pattern (load aggregate → call domain method → save → return), pipeline behaviors (AssignUserIdBehavior, ValidationBehavior, LoggingBehavior), command record conventions (`[JsonIgnore]` for ArticleId/CreatedById), difference between MediatR services and FastEndpoints services.

### 5. .claudeignore (create at repo root)

```
bin/
obj/
Migrations/
.vs/
*.lock
docker-compose.override.yml
```

### 6. Plans directory

Create `plans/` directory at repo root with a `.gitkeep` file. Add a brief `plans/README.md`:
```
# Feature Plans

Implementation plans created by the Architect agent.
Each file follows the standard plan format and is consumed by the Developer agent.

Format: `{FeatureName}.md`
```

## Execution order

1. Create `.claudeignore`
2. Create root `CLAUDE.md`
3. Create all 6 service-level CLAUDE.md files
4. Create `agents/Architect.md` (update from existing)
5. Create `agents/Developer.md` (new)
6. Create `plans/` directory with README
7. Create workflow skills: `plan-feature`, `create-feature`, `create-aggregate`, `create-grpc-contract`, `add-integration-event`
8. Create background knowledge skills: `article-lifecycle`, `service-boundaries`, `domain-patterns`, `cqrs-patterns`
9. Verify: list the complete directory structure of `.claude/`, `agents/`, `plans/`, and all service-level CLAUDE.md files

## Quality checks

After building everything:
- Root CLAUDE.md should be under 150 lines
- Each service CLAUDE.md should be under 50 lines
- Each SKILL.md should be under 500 lines (recommended by Anthropic)
- Workflow skills should have clear routing logic and templates
- Background knowledge skills should have `user-invocable: false` in frontmatter
- No code examples in root CLAUDE.md (those belong in skills)
- No duplicate content between root CLAUDE.md and skills
- Agent role files should NOT be in `.claude/agents/` (they are regular files in `agents/`)
- Skills MUST be in `.claude/skills/` (CC auto-discovers them there)

## Existing Architect.md for reference

The current Architect.md is in the `agents/` folder. Update it as described in section 3 — keep all system knowledge sections, remove convention references, add plan file writing capability and plan workflow section.
