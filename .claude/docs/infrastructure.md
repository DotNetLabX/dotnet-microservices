# Infrastructure

## gRPC rules

- gRPC is the contract for all service-to-service calls
- Contracts live in `BuildingBlocks/Articles.Grpc.Contracts/` (code-first proto approach)
- Prefer explicit request/response messages (versionable)
- Clients registered per-service using `AddCodeFirstGrpcClient<TService>()` in DI

### gRPC services inventory

| Service contract | Host service | Methods |
|-----------------|--------------|---------|
| `IPersonService` | Auth | GetPersonById, GetPersonByUserId, GetPersonByEmail, GetOrCreatePersonAsync |
| `IJournalService` | Journals | GetJournalById, IsEditorAssignedToJournal |
| `IArticleQueryService` | Submission | GetArticleDetails (placeholder) |

### gRPC client usage by service

| Service | Calls |
|---------|-------|
| Submission | IPersonService, IJournalService |
| Review | IPersonService |
| Journals | IPersonService |
| Production | (none currently) |
| ArticleHub | (none — consumes events only) |
| Auth | (none — server only) |

### Adding a new gRPC method

1. Update proto/contracts in `Articles.Grpc.Contracts`
2. Rebuild
3. Implement server endpoint in target service
4. Register client via `AddCodeFirstGrpcClient<T>()` in consuming service's DI
5. Update callers

## Messaging (MassTransit)

- Use integration events when a change must be observed by other services
- Contracts live in `BuildingBlocks/Articles.Integration.Contracts/`
- Consumers must be idempotent
- Keep event contracts stable — don't leak EF/domain internals

### Integration events

| Event | Triggered by | Consumed by |
|-------|-------------|-------------|
| ArticleApprovedForReviewEvent | Submission | Review, ArticleHub |
| ArticleAcceptedForProductionEvent | Review | Production, ArticleHub |
| ArticleReviewedEvent | Review | ArticleHub |
| ArticlePublishedEvent | Production | ArticleHub |
| JournalCreatedEvent | Journals | ArticleHub |
| JournalUpdatedEvent | Journals | ArticleHub |
| PersonUpdatedEvent | Auth | ArticleHub |

## EF Core patterns

- Domain events raised in aggregates/entities
- Dispatched via EF Core `SaveChangesInterceptor` pattern
- Keep event handlers idempotent and mindful of transaction boundaries
- Base contexts in `Blocks.EntityFrameworkCore`: `ApplicationDbContext<T>` (with caching), `TenantDbContext` (multi-tenancy)

### Migrations

```bash
# Add migration
dotnet ef migrations add MigrationName -p Services/{Service}/{Service}.Persistence -s Services/{Service}/{Service}.API

# Apply migration
dotnet ef database update -p Services/{Service}/{Service}.Persistence -s Services/{Service}/{Service}.API
```

## Modules

### ArticleTimeline
- Tracks article stage transitions via domain event handlers
- `AddTimelineEventHandler<TDomainEvent, TAction>` — generic handler that creates Timeline entries
- Transaction-aware: writes within the same transaction as the triggering event
- Used by all services that have article stage changes

### EmailService
- Interface: `IEmailService.SendEmailAsync(EmailMessage, CancellationToken)`
- Implementations: Empty (dev stub), SendGrid, SMTP
- Each service configures its preferred implementation in DI

### FileService
- Interface: `IFileService` — GenerateId, UploadAsync, DownloadAsync, DeleteAsync, FindByTag
- Implementations: AzureBlob, MongoGridFS, MinIO (prototype)
- Submission & Review use MongoGridFS; Production uses AzureBlob

## Useful commands

```bash
dotnet build                    # Build all
dotnet test                     # Run tests
docker compose up -d            # Start infrastructure (DBs, Redis, RabbitMQ)
```
