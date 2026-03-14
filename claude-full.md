# claude.md — DotNetLabX Articles Application (Guidelines for AIAgent Work)

You are helping build an Articles Application in .NET. The codebase is used to create a Udemy-style course on DDD + Vertical Slice + CQRS, evolving from a modular monolith into microservices where it makes sense.

## Quick Reference (Common Tasks)

 Task  Steps 
-------------
 Add new endpoint  1. Identify service → 2. Create vertical slice folder → 3. Add Endpoint + RequestResponse → 4. Add Validator → 5. Add CommandQuery + Handler → 6. Add Mapster config 
 Add gRPC method  1. Update `.proto` in `Articles.Grpc.Contracts` → 2. Rebuild → 3. Implement server → 4. Update client in `ClientServices` 
 Add migration  `dotnet ef migrations add Name -p Service.Persistence -s Service.API` 
 Cross-service call  Always use gRPC (via client wrappers in `ClientServices`) 
 Cross-service event  Use MassTransit integration events (not domain events) 

## What this system is
A suite that supports an end-to-end article lifecycle (draftsubmission → review → production → publishpublic pages later). There is also an ArticleHub that aggregates the latest state of each article across services via events.

## Non-negotiable architecture rules
- DDD + Vertical Slice over layered services.
- CQRS Commands mutate, queries read (keep handlers focused and small).
- DRY + YAGNI don't add abstractionsinterfaces unless there are truly multiple implementations.
- No Service layer classes like `ArticleService`. Use
  - domain logic inside aggregatesentitiesvalue objects
  - repositories for persistence
  - clientsproxies for calling other services (gRPC clients)
  - infra helpers (cache, messaging, http, etc.) as needed
- Prefer domain events inside a boundary; use integration events for cross-service communication.
- All inter-service communication is gRPC (HTTP APIs are for external clients  gateway).

## Tech stack (common)
- .NET  ASP.NET Core
- FastEndpoints (at least in SubmissionAuth services)
- MediatR (CQRS handlers; in FastEndpoints services the application layer can be thin)
- FluentValidation
- EF Core (SQL Server andor Postgres depending on service)
- Mapster (mappings)
- MassTransit (messaging when needed)
- Redis (caching where needed)
- API Gateway YARP (for the course)

## Repository structure (high-level)
```
Articles
├── BuildingBlocks
│   ├── Articles.Abstractions
│   ├── Articles.Grpc.Contracts
│   ├── Articles.Security
│   └── Blocks. (Core, Domain, EFCore, MediatR, Messaging, Redis, FastEndpoints, Exceptions)
├── ClientServices
│   └── ServiceNameGrpcClient (e.g., UserServiceGrpcClient)
├── Modules
│   ├── ArticleTimeline
│   ├── EmailService
│   ├── FileStorage
│   └── CacheStorage
├── Services
│   ├── Submission
│   ├── Review
│   ├── Production
│   ├── Journals
│   ├── Auth
│   └── ArticleHub
├── ApiGateway
└── docker-compose
```

## Port conventions
- Services use the 4400–4499 range.
- Typically pairs like (4403, 4453) for HTTPHTTPS.

## Naming conventions (strict)
- Private fields `_camelCase`
- Localsparameters descriptive `camelCase` (no `req`, `ops`, `_q`, `_m`)
- Public memberstypes `PascalCase`
- Prefer explicit names over cleverness.

## Vertical slice conventions (typical)

Example structure for a feature in a service
```
ServicesSubmissionFeaturesSubmitArticle
├── SubmitArticleEndpoint.cs          (FastEndpoints endpoint)
├── SubmitArticleRequest.cs           (DTO)
├── SubmitArticleResponse.cs          (DTO)
├── SubmitArticleValidator.cs         (FluentValidation)
├── SubmitArticleCommand.cs           (MediatR command)
├── SubmitArticleHandler.cs           (MediatR handler)
├── SubmitArticleMappings.cs          (Mapster config)
└── SubmitArticleTests.cs             (if tests are co-located)
```

When adding a feature, keep everything close to the feature
- Endpoint
- RequestResponse DTOs
- Validator
- CommandQuery + Handler
- Mapping (Mapster)
- Persistence changes (EF Core)
- Tests (if present in the service)

Avoid creating shared god folders like `Services`, `Helpers`, `Utils` unless unavoidable.

## EF Core + domain events
- Domain events are raised in aggregatesentities.
- They are dispatched via an EF Core SaveChangesInterceptor pattern.
- Keep event handlers idempotent and mindful of transaction boundaries.

## gRPC rules
- gRPC is the contract for service-to-service calls.
- Contracts live in `Articles.Grpc.Contracts` (or equivalent).
- Prefer explicit requestresponse messages (versionable).
- When adding a new method
  1. Update protocontracts in `Articles.Grpc.Contracts`
  2. Regeneratebuild
  3. Implement server endpoint in target service
  4. Update client wrapper in `ClientServicesServiceNameGrpcClient`
  5. Update callers

## Messaging rules (MassTransit)
- Use integration events when a change must be observed by other services (e.g., ArticleHub sync).
- Keep event contracts stable; avoid leaking EFdomain internals.
- Consumers must be idempotent.

## Modules vs Microservices

Use a Module when
- Feature is reusable but doesn't need independent deployment
- Lowno cross-team ownership concerns
- Shares lifecycle with parent service
- Example `EmailService`, `FileStorage`

Use a Microservice when
- Needs independent scalingdeployment
- Different team ownership
- Different data sovereignty requirements
- Example `Submission`, `Review`, `Production` services

Default to module first unless there's a strong reason for independent deploymentscalingownership.

## How to work on this repo (agent workflow)
When you implement something, follow this routine
1. Restate the change in 1–2 lines.
2. Identify the bounded contextservice impacted.
3. List the files you will touch before editing.
4. Implement minimal change.
5. Ensure
   - build passes
   - existing patterns are respected (FastEndpointsMediatRValidationMapster)
   - no new abstractions unless justified

## Questions you MUST ask before making assumptions
If any of these are unclear, ask first (don't guess)
- Which service owns the feature
- Is this command or query
- Does it require cross-service communication (gRPC) or integration events
- Which DB is used by that service (SQL Server vs Postgres)
- Do we need idempotency  retries (messaging)
- Any existing naminglocation conventions for that service's slices

## Guardrails (common mistakes to avoid)
- Don't introduce a generic service layer.
- Don't add interfaces just in case.
- Don't centralize feature code into shared buckets.
- Don't bypass domain rules by pushing logic into EF configurations or endpoints.
- Don't invent new patterns if the repo already has one that works.
- Don't use abbreviated variable names (`req`, `cmd`, `res`, `ops`).

## Useful commands (typical)
```bash
# Build
dotnet build

# Test
dotnet test

# Run compose
docker compose up -d

# EF migrations (example)
dotnet ef migrations add Name -p ServicesServiceService.Persistence -s ServicesServiceService.API
dotnet ef database update -p ServicesServiceService.Persistence -s ServicesServiceService.API
```

---
If you need to refactor keep it small, vertical-slice-friendly, and explain why (DDD rule, coupling, boundary, testability), not just cleanup.