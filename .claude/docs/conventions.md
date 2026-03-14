# Conventions

## Quick reference (common tasks)

| Task | Steps |
|------|-------|
| Add new endpoint | 1. Identify service → 2. Create vertical slice folder → 3. Add Endpoint + Request/Response → 4. Add Validator → 5. Add Command/Query + Handler → 6. Add Mapster config |
| Add gRPC method | 1. Update `.proto` in `Articles.Grpc.Contracts` → 2. Rebuild → 3. Implement server → 4. Update client registration in consuming service's DI |
| Add migration | `dotnet ef migrations add Name -p Service.Persistence -s Service.API` |
| Cross-service call | Always gRPC (registered via `AddCodeFirstGrpcClient<T>()` in each service's DI) |
| Cross-service event | MassTransit integration events (contracts in `Articles.Integration.Contracts`) |

## Naming conventions (strict)

- Private fields: `_camelCase`
- Locals/parameters: descriptive `camelCase` (no `req`, `ops`, `_q`, `_m`, `cmd`, `res`)
- Public members/types: `PascalCase`
- Prefer explicit names over cleverness

## Vertical slice conventions

Each feature lives in its own folder with all related files co-located:

```
Services/{Service}/{Service}.API/Features/{FeatureName}/
├── {FeatureName}Endpoint.cs          (FastEndpoints / Carter / MinimalAPI endpoint)
├── {FeatureName}Request.cs           (DTO)
├── {FeatureName}Response.cs          (DTO)
├── {FeatureName}Validator.cs         (FluentValidation)
├── {FeatureName}Command.cs           (MediatR command or query)
├── {FeatureName}Handler.cs           (MediatR handler)
├── {FeatureName}Mappings.cs          (Mapster config)
└── {FeatureName}Tests.cs             (if tests are co-located)
```

When adding a feature, keep everything close:
- Endpoint, Request/Response DTOs, Validator, Command/Query + Handler, Mapping, Persistence changes, Tests

Avoid creating shared god folders like `Services/`, `Helpers/`, `Utils/` unless unavoidable.

## Port conventions

Services use the 4400–4499 range, typically in HTTP/HTTPS pairs:

| Service | HTTP | HTTPS |
|---------|------|-------|
| Auth | 4401 | 4451 |
| Journals | 4402 | 4452 |
| ArticleHub | 4403 | 4453 |
| Submission | 4404 | 4454 |
| Review | 4405 | 4455 |
| Production | 4406 | 4456 |
