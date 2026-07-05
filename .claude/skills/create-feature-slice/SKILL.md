---
name: create-feature-slice
description: Adds a complete vertical-slice feature to a service in this repo — feature folder, command or query record, co-located validator, handler (or in-endpoint logic), route, and a declarative authorization gate. Three variants selected by the service's declared endpoint framework: FastEndpoints without MediatR (handler in the endpoint), Carter + MediatR, and Minimal APIs + MediatR. Use when adding a new feature to Submission, Review, Journals, Production, Auth, or ArticleHub — read the service's CLAUDE.md for its framework first.
user-invocable: true
---

# Create Feature Slice

Adds one vertical-slice feature to a service: a feature *folder* holding the command or query, its co-located validator, and the handler — plus a thin endpoint and a declarative authorization gate. A feature is always a folder; there is never a shared `Controllers/` or `Validators/` god-folder (endpoints-3).

The endpoint framework is a per-service decision (endpoints-1). Read the target service's `CLAUDE.md` **Endpoint framework** line first, then follow the matching variant — do not carry one service's shape into another.

## Step 0 — Pick the variant

Read `src/Services/{Service}/CLAUDE.md`. Its **Endpoint framework** line selects one of three variants; each has a copy-ready template with full code for every phase below. Then skim one existing feature in that same service — the closest sibling operation — for its exact current idioms (folder nesting, base types, response shape) before writing: the template gives the shape, the neighbour confirms this service's live form.

| Declared framework | Variant | Services | Handler shape | Template |
|---|---|---|---|---|
| FastEndpoints (no MediatR) | A | Journals, Production, Auth | logic in the endpoint's `HandleAsync` — **no handler file** (endpoints-2) | `templates/fastendpoints.md` |
| Carter + MediatR | B | Review (ArticleHub is read-only) | `ICarterModule` shim dispatches via `ISender` to an `IRequestHandler` | `templates/carter-mediatr.md` |
| Minimal APIs + MediatR | C | Submission | static `Map` extension dispatches via `ISender` to an `IRequestHandler` | `templates/minimal-api-mediatr.md` |

The framework inventory is architecture-reference.md §4; the rules are endpoints-1 and endpoints-2.

## Where the feature folder lives

- **Variant A (FastEndpoints):** everything is colocated in the API project — `{Service}.API/Features/{Area}/{Operation}/` holds the endpoint + command + validator together (endpoints-3).
- **Variants B and C (MediatR):** the command + validator + handler live in `{Service}.Application/Features/{Area}/{Operation}/`; the thin endpoint shim lives in `{Service}.API/Endpoints/` (endpoints-2, endpoints-3). Submission uses a flat `Features/{Operation}/`; Review nests under an `{Area}/` (for example `Articles/`).

## Phases (every variant)

Each phase states the binding rule and its registry row; the concrete per-variant code is in the template from Step 0.

1. **Feature folder** — create `Features/{Area}/{Operation}/` in the project named above. One folder per feature; never a god-folder (endpoints-3).

2. **Command / query record**
   - *Article-scoped write* — inherit the service's `ArticleCommand` base (its own `Features/.../_Shared/ArticleCommand.cs`, which is `ArticleCommandBase<ArticleActionType>` plus the CQRS marker). The base self-computes audit and provenance: `ArticleId`, `ActionType`, `Action`, `CreatedOn`, and `CreatedById` are all `[JsonIgnore]` and pipeline-set — only `Comment` and your own payload fields are wire-settable. Override the abstract `ActionType` with this feature's enum value. Never add a settable `CreatedById` or `CreatedOn` to the wire contract; provenance is pipeline-computed, never client-trusted (appcqrs-1).
   - *Non-article command/query* (for example CreateJournal, GetArticle) — a plain record. MediatR variants mark it `ICommand<TResponse>` or `IQuery<TResponse>`; FastEndpoints request records carry no marker.
   - The `ICommand<T>` / `IQuery<T>` markers express read/write intent in the type system only — no pipeline behavior branches on them (appcqrs-5).

3. **Validator (co-located)** — in the same file as the record. The base type follows the framework (endpoints-4): MediatR uses `AbstractValidator<T>`; FastEndpoints uses `Validator<T>` (or the project-local `BaseValidator<T>` in Production). Article-scoped commands extend the service's `ArticleCommandValidator<T>` base, which supplies the `ArticleId` greater-than-zero rule. Validators are auto-discovered — MediatR via `AddValidatorsFromAssemblyContaining` (already wired in `Application/DependencyInjection.cs`), FastEndpoints via assembly scan — so do not register the validator by hand.

4. **Handler / in-endpoint logic** — same body shape everywhere: load the aggregate, call its existing domain behavior method, `SaveChangesAsync`, return the response.
   - *Variant A* — the logic goes in the endpoint's `HandleAsync`; there is no handler file (endpoints-2).
   - *Variants B and C* — write an `IRequestHandler<TCommand, TResponse>` in the feature folder; the endpoint only dispatches via `ISender`. Handlers are MediatR-assembly-scanned — no DI edit.
   - No role checks and no `IsInRole` in the handler or domain body — authorization is the endpoint's job (endpoints-5).

5. **Endpoint + route**
   - REST-resource-first URL; append a `:verb` RPC suffix for non-CRUD actions (`/articles/{articleId:int}:approve`), applied identically across all three frameworks. Plain CRUD stays verb-less (`POST /articles`, `GET /articles/{id}`) (namingx-4).
   - Route-id binding: bind `{articleId}` from the route into the command before dispatch — Minimal API uses `command with { ArticleId = articleId }`, Carter sets `command.ArticleId = articleId`.
   - Registration: FastEndpoints endpoints are attribute-routed and auto-discovered by `UseCustomFastEndpoints()`, and Carter modules are auto-discovered by `MapCarter()` — both need zero manual wiring. Minimal API is the exception: add one line to `EndpointRegistration.MapAllEndpoints()`.

6. **Authorization gate** — declarative, at the endpoint boundary only (endpoints-5):
   - *Role-gated article write* — the two-layer `RequireRoleAuthorization(Role.X, ...)` (Minimal API / Carter) or `[Authorize(Roles = Role.X)]` (FastEndpoints). The `RequireRoleAuthorization` extension stacks `RequireRole` plus an `ArticleRoleRequirement` resource check; never call bare `RequireRole` (security-2).
   - *Read-only aggregate endpoint* (ArticleHub, or any plain authenticated read) — single-layer `.RequireAuthorization()` or `[Authorize]`: authentication only, no resource gate (security-2).

## Response mapping (Mapster)

Handlers and endpoints move data between the wire, the domain, and the response with Mapster's `.Adapt<T>()` — the same tool inbound and outbound:

- **Inbound** (wire record to entity), when the shapes line up: `var journal = command.Adapt<Journal>();` (Journals CreateJournal).
- **Outbound** (entity to DTO): `return new GetArticleResponse(article.Adapt<ArticleDto>());` (Submission GetArticle); `journal.Adapt<JournalDto>()` (Journals GetJournal).

**Eligibility test — bare `.Adapt<T>()` or a config?** Use a bare `.Adapt<T>()` only when member names and nesting line up and Mapster's flattening convention (a nested `Journal.Name` resolves to a flat `JournalName`) does the rest. The moment a member name differs, a value object needs a factory, inheritance is involved, or a property must be ignored, register the rule in an `IRegister` config in the service's `Mappings/` folder (assembly-scanned by `AddMapsterConfigsFromAssemblyContaining`) — never hand-map field-by-field in the handler. Real rules from `Submission.Application/Mappings/RestEndpointMappings.cs`:

```csharp
config.NewConfig<ArticleActor, ActorDto>().Include<ArticleAuthor, ActorDto>();   // inheritance
config.NewConfig<IArticleAction<ArticleActionType>, ArticleAction>()
      .Map(dest => dest.TypeId, src => src.ActionType);                          // renamed member
config.ForType<string, EmailAddress>().MapWith(src => EmailAddress.Create(src)); // value-object factory
```

**Flattening caveat:** the convention is silent — a nested member that does not resolve to a matching flat name is left at its default, with no error. When an outbound `ArticleDto`/`JournalDto` comes back with an unexpectedly empty field, suspect a flattening miss and add an explicit `.Map(...)` rather than trusting the convention.

## Binding rules

1. A feature is a folder; never a `Controllers/` or `Validators/` god-folder (endpoints-3).
2. The endpoint framework comes from the service `CLAUDE.md`, not from preference (endpoints-1).
3. Article provenance (`ArticleId`, `CreatedById`, `CreatedOn`, `Action`) is `[JsonIgnore]` and pipeline-computed — never a wire field (appcqrs-1).
4. Commands and queries carry `ICommand<T>` / `IQuery<T>` markers in MediatR services; nothing branches on them (appcqrs-5).
5. The MediatR request pipeline is already registered once per service in `Application/DependencyInjection.cs` — do not re-register behaviors; rely on it to stamp `CreatedById` before validation runs (appcqrs-2). For the pipeline itself — the behaviors, their order, and how to add one — see the `cqrs-patterns` skill.
6. Authorization is declarative at the endpoint; zero role logic in a handler or domain body (endpoints-5).
7. Non-CRUD routes take a `:verb` suffix (namingx-4).

## What this skill does NOT do

- Create or modify the domain aggregate or its behavior method. The handler *calls* an existing behavior method; if it is missing, add it with the aggregate/domain skill first.
- Wire integration events, consumers, gRPC contracts, or file storage — separate concerns with their own skills.
- Register pipeline behaviors, validators, or handlers by hand — all are already auto-wired (appcqrs-2).
- Add or reorder MediatR pipeline behaviors, or go deeper on command/query dispatch — that is the `cqrs-patterns` skill; this one only slots a feature into the existing pipeline.
- Split a FastEndpoints endpoint that has outgrown its file into endpoint + sibling request/response types — that is the `extract-endpoint-types` skill, the natural follow-up once a Variant A slice grows.

## Lessons

When this skill misfires — a service's real feature shape has drifted from a template, a cited row is now wrong, or you hit a case a variant does not cover — capture it in `docs/skill-backlog.md` (or the running feature's `lessons.md`) so the skill gets a consolidating fix, not a silent one-off workaround.
