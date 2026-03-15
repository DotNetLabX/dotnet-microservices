# Consume ArticleAcceptedForProduction

## Summary

Add a MassTransit consumer in Production that handles `ArticleAcceptedForProductionEvent` from Review. Creates the local Article aggregate with journal, authors, and assets (including file transfer from Review's MongoGridFS to Production's Azure Blob). Notifies all seeded POF users via email so they know to assign a typesetter.

## Affected services

- **Production** — new consumer, domain entities, domain event, event handler, seed data, MassTransit + EmailService + MongoGridFS registration

## Domain model changes

- New `ArticleActionType`: `AcceptForProduction`
- New TPH subclass: `ProductionOfficer : Person` (not article-scoped — POFs manage all articles)
- New domain event: `ArticleAcceptedForProduction(Article, IArticleAction)` — triggers POF email notification
- New factory method: `Article.FromReview(...)` — creates aggregate from integration event DTO
- No new association entity for POF — they are not linked to articles via `ArticleContributor`

## File list

### Step 1 — Register MassTransit in Production API
- `src/Services/Production/Production.API/DependencyInjection.cs` — add `AddMassTransitWithRabbitMQ`
- `src/Services/Production/Production.Application/DependencyInjection.cs` — remove commented `AddMessageBroker` line
- `src/Services/Production/Production.API/Production.API.csproj` — add reference to `Blocks.Messaging` if not already present

### Step 2 — Register EmailService in Production
- `src/Services/Production/Production.API/DependencyInjection.cs` — add `AddEmptyEmailService`
- `src/Services/Production/Production.API/Production.API.csproj` — add reference to `EmailService.Empty`

### Step 3 — Register MongoGridFS for reading from Review's storage
- `src/Services/Production/Production.API/DependencyInjection.cs` — add `AddMongoFileStorageAsScoped<ReviewFileStorageOptions>`
- `src/Services/Production/Production.API/ReviewFileStorageOptions.cs` — create (extends `MongoGridFsFileStorageOptions`)
- `src/Services/Production/Production.API/Production.API.csproj` — add reference to `FileService.MongoGridFS`
- `src/Services/Production/Production.API/appsettings.json` — add Review MongoDB connection string and `ReviewFileStorageOptions` section

### Step 4 — Add AcceptForProduction action type
- `src/Services/Production/Production.Domain/_Shared/Enums/ArticleActionType.cs` — add `AcceptForProduction`

### Step 5 — Add ProductionOfficer entity
- `src/Services/Production/Production.Domain/Articles/ProductionOfficer.cs` — create (extends `Person`)
- `src/Services/Production/Production.Persistence/EntityConfigurations/ProductionOfficerEntityConfiguration.cs` — create TPH config
- `src/Services/Production/Production.Persistence/ProductionDbContext.cs` — add `DbSet<ProductionOfficer>`, apply configuration
- EF migration

### Step 6 — Add ArticleAcceptedForProduction domain event
- `src/Services/Production/Production.Domain/Articles/Events/ArticleAcceptedForProduction.cs` — create

### Step 7 — Add Article.FromReview factory method
- `src/Services/Production/Production.Domain/Articles/Behavior/Article.cs` — add static factory method

### Step 8 — Create the consumer
- `src/Services/Production/Production.API/Features/Articles/InitializeFromReview/ArticleAcceptedForProductionConsumer.cs` — create

### Step 9 — Create the POF email notification handler
- `src/Services/Production/Production.API/Features/Articles/InitializeFromReview/NotifyProductionOfficeOnArticleAcceptedHandler.cs` — create

### Step 10 — Seed POF persons
- `src/Services/Production/Production.Persistence/Data/Test/Person.json` — add ProductionOfficer person(s)
- `src/Services/Production/Production.Application/Seed.cs` — verify `SeedFromJsonFile<Person>()` handles TPH discriminator for ProductionOfficer (it should, since Typesetter already works the same way)

## Numbered implementation steps

### Step 1 — Register MassTransit in Production API

Move MassTransit registration from Application to API (same pattern as ArticleHub and Journals — Production features live in API, so the consumer assembly is API).

In `Production.API/DependencyInjection.cs` `AddApiServices`, add:
```
.AddMassTransitWithRabbitMQ(configuration, Assembly.GetExecutingAssembly())
```

In `Production.Application/DependencyInjection.cs`, remove the commented-out `AddMessageBroker` line.

Verify `Production.API.csproj` references `Blocks.Messaging`. If not, add it.

Verify `appsettings.json` has `RabbitMqOptions` section. If not, copy from another service (e.g., Review).

**Reference:** `src/Services/ArticleHub/ArticleHub.API/DependecyInjection.cs` line 46 for the API-level registration pattern.

### Step 2 — Register EmailService in Production

In `Production.API/Production.API.csproj`, add project reference to `EmailService.Empty`.

In `Production.API/DependencyInjection.cs` `AddApiServices`, add:
```
services.AddEmptyEmailService(configuration);
```

Add `EmailOptions` section to `appsettings.json` with `EmailFromAddress` (copy from Auth's appsettings).

**Reference:** `src/Services/Auth/Auth.API/DependencyInjection.cs` line 63.

### Step 3 — Register MongoGridFS for reading from Review's storage

Production needs to download files from Review's MongoGridFS. Since Production's own storage is Azure Blob (`IFileService`), register MongoGridFS as a typed service to avoid collision.

Create `src/Services/Production/Production.API/ReviewFileStorageOptions.cs`:
- Class extending `MongoGridFsFileStorageOptions` — just a marker to distinguish it from Production's own storage

In `Production.API/DependencyInjection.cs`, add:
```
services.AddMongoFileStorageAsScoped<ReviewFileStorageOptions>(configuration);
```

In `Production.API/Production.API.csproj`, add reference to `FileService.MongoGridFS`.

In `appsettings.json`, add `ReviewFileStorageOptions` section with Review's MongoDB connection string, database name, and bucket name.

The consumer will inject `IFileService<ReviewFileStorageOptions>` for downloads and `IFileService` for Azure Blob uploads.

**Reference:** `src/Services/Review/Review.API/FileServiceFactoryRegistration.cs` for how Review registers multiple MongoGridFS instances.

### Step 4 — Add AcceptForProduction action type

In `Production.Domain/_Shared/Enums/ArticleActionType.cs`, add `AcceptForProduction` to the enum.

### Step 5 — Add ProductionOfficer entity + EF config

Create `src/Services/Production/Production.Domain/Articles/ProductionOfficer.cs`:
- Extends `Person` (same pattern as `Typesetter : Person`)
- No additional properties needed for now (can add later if needed)

Create `src/Services/Production/Production.Persistence/EntityConfigurations/ProductionOfficerEntityConfiguration.cs`:
- TPH discriminator configuration (follow `TypesetterEntityConfiguration` pattern)

In `ProductionDbContext.cs`:
- Add `DbSet<ProductionOfficer> ProductionOfficers`
- Add `modelBuilder.ApplyConfiguration(new ProductionOfficerEntityConfiguration())`

Run migration:
```
dotnet ef migrations add AddProductionOfficer -p Services/Production/Production.Persistence -s Services/Production/Production.API
```

**Reference:** `src/Services/Production/Production.Domain/Articles/Typesetter.cs` and its entity configuration.

### Step 6 — Add ArticleAcceptedForProduction domain event

Create `src/Services/Production/Production.Domain/Articles/Events/ArticleAcceptedForProduction.cs`:
- Record extending `DomainEvent(IArticleAction)`
- Carries the `Article` reference so the email handler can read article title and details

**Reference:** `TypesetterAssigned.cs` in the same folder.

### Step 7 — Add Article.FromReview factory method

In `Production.Domain/Articles/Behavior/Article.cs`, add static factory method:

```
public static Article FromReview(ArticleDto articleDto, IEnumerable<ArticleContributor> contributors, IEnumerable<Asset> assets, IArticleAction action)
```

- Creates Article with: Id from DTO, Title, Doi, JournalId, SubmittedById, SubmittedOn, AcceptedOn, Stage (from DTO — `AcceptedForProduction`)
- Adds contributors and assets to backing lists
- Raises `ArticleAcceptedForProduction` domain event
- Does NOT call `SetStage` — this is initialization, not a state transition

**Reference:** `src/Services/Review/Review.Domain/Articles/Behaviors/Article.cs` → `FromSubmission`.

### Step 8 — Create the consumer

Create `src/Services/Production/Production.API/Features/Articles/InitializeFromReview/ArticleAcceptedForProductionConsumer.cs`:

Sealed class implementing `IConsumer<ArticleAcceptedForProductionEvent>`. Injects:
- `ProductionDbContext` — for direct entity access and save
- `ArticleRepository` — for idempotency check
- `Repository<Person>` — for get-or-create persons
- `Repository<Journal>` — for get-or-create journal
- `IFileService<ReviewFileStorageOptions>` — download from Review's MongoGridFS
- `IFileService` — upload to Production's Azure Blob

Consumer logic:
1. **Idempotency** — check if article exists by Id, return early if it does
2. **Journal** — get or create from DTO
3. **Contributors** — for each actor in DTO with AUT/CORAUT role: get or create Person as Author, create `ArticleContributor`
4. **Assets** — for each asset in DTO: download file from Review's MongoGridFS via `IFileService<ReviewFileStorageOptions>`, upload to Azure Blob via `IFileService`, create Asset entity with file link
5. **Action** — create `IArticleAction` with `AcceptForProduction` type
6. **Article** — call `Article.FromReview(...)` to build aggregate
7. **Save** — add article and save (domain event dispatch happens via `TransactionalDispatchDomainEventsInterceptor`)

**Reference:** `src/Services/Review/Review.Application/Features/Articles/InitializeFromSubmission/ArticleApprovedForReviewConsumer.cs` — same structure.

### Step 9 — Create POF email notification handler

Create `src/Services/Production/Production.API/Features/Articles/InitializeFromReview/NotifyProductionOfficeOnArticleAcceptedHandler.cs`:

FastEndpoints `IEventHandler<ArticleAcceptedForProduction>` that:
1. Queries `ProductionDbContext.ProductionOfficers` for all POFs
2. For each POF, builds an `EmailMessage` with article title info
3. Sends via `IEmailService`

Inject: `ProductionDbContext`, `IEmailService`, `IOptions<EmailOptions>`

**Reference:** `src/Services/Auth/Auth.API/Features/Users/CreateAccount/SendConfirmationEmailOnUserCreatedHandler.cs`.

### Step 10 — Seed POF persons

In `src/Services/Production/Production.Persistence/Data/Test/Person.json`, add one or two ProductionOfficer entries with:
- `$type` discriminator matching `ProductionOfficer` type
- Email, name, affiliation
- `UserId` linking to a seeded Auth user with POF role

Verify the existing `SeedFromJsonFile<Person>()` handles TPH correctly (it already works for Typesetter, so it should work for ProductionOfficer too).

## Cross-service impacts

- No new integration events — consuming the existing `ArticleAcceptedForProductionEvent`
- No new gRPC contracts
- **File transfer:** Production reads from Review's MongoGridFS bucket (needs Review's MongoDB connection string in Production's appsettings)

## Migration notes

Step 5 requires a migration for the ProductionOfficer TPH discriminator:
```
dotnet ef migrations add AddProductionOfficer -p Services/Production/Production.Persistence -s Services/Production/Production.API
dotnet ef database update -p Services/Production/Production.Persistence -s Services/Production/Production.API
```

## Open questions

None — all decisions resolved.
