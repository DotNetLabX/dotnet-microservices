# Architecture Reference — Codebase Inventory

This document is a **factual inventory of what exists** in `D:\src\dotnet-microservices`: repository structure, per-service domain models, ports, topology, and the concrete artifacts on disk. It is descriptive, not prescriptive.

The **verified pattern rules** — what the codebase does and *why*, each backed by an executed command — live in [`docs/reference-model.md`](reference-model.md) (75 rows across 16 dimensions). The two documents are complementary and must not duplicate each other: when this file needs to state a rule, it points to a registry dimension and row id rather than restating it.

> Originally scanned 2026-03-22; slimmed and corrected 2026-07-05 at commit `cd4d0b1` against the verified registry and its cross-check audit.

## Table of Contents

1. [Repository Structure](#1-repository-structure)
2. [Architecture Patterns](#2-architecture-patterns)
3. [Domain Models per Service](#3-domain-models-per-service)
4. [Endpoint Frameworks](#4-endpoint-frameworks)
5. [Persistence Inventory](#5-persistence-inventory)
6. [Cross-Service Communication (gRPC)](#6-cross-service-communication-grpc)
7. [Messaging and Events (MassTransit)](#7-messaging-and-events-masstransit)
8. [BuildingBlocks](#8-buildingblocks)
9. [Mapping Patterns](#9-mapping-patterns)
10. [Naming Conventions](#10-naming-conventions)
11. [Service-Specific Patterns](#11-service-specific-patterns)
12. [Testing Patterns](#12-testing-patterns)
13. [Configuration and Infrastructure](#13-configuration-and-infrastructure)

---

## 1. Repository Structure

### 1.1 Solution Files

Two solution files exist:

- `src/Articles.sln` — master solution containing all projects
- `src/Submission.sln` — focused solution for the Submission service only

### 1.2 Full Directory Tree

```
src/
├── ApiGateway/                                  # YARP reverse proxy
│   ├── Program.cs
│   └── appsettings.json
│
├── BuildingBlocks/
│   ├── Articles.Abstractions/                   # Shared article interfaces & enums
│   │   ├── ArticleCommandBase.cs                # Base record for all article commands
│   │   ├── DomainEvent.cs                       # Generic DomainEvent<TAction> base
│   │   ├── IArticleAction.cs                    # Interface implemented by all commands
│   │   ├── IdResponse.cs                        # Shared response type
│   │   ├── Enums/
│   │   │   ├── ArticleStage.cs                  # Full workflow stage enum (101-305)
│   │   │   ├── ArticleType.cs
│   │   │   ├── AssetCategory.cs
│   │   │   ├── AssetType.cs
│   │   │   ├── ContributionArea.cs
│   │   │   ├── Gender.cs
│   │   │   ├── Honorific.cs
│   │   │   └── UserRoleType.cs                  # Role codes (EOF, AUT, REVED, REV, POF, TSOF, USERADMIN)
│   │   ├── Events/
│   │   │   └── ArticleStageChanged.cs           # Cross-service domain event
│   │   └── Security/
│   │       └── IArticleAccessChecker.cs
│   │
│   ├── Articles.Grpc.Contracts/                 # Code-first gRPC contracts
│   │   ├── Auth/PersonContracts.cs              # IPersonService + DTOs with [ProtoContract]
│   │   └── Journals/JournalContracts.cs         # IJournalService + DTOs
│   │
│   ├── Articles.Integration.Contracts/          # MassTransit integration event records
│   │   ├── Articles/
│   │   │   ├── ArticleAcceptedForProductionEvent.cs
│   │   │   ├── ArticleApprovedForReviewEvent.cs
│   │   │   ├── ArticlePublishedEvent.cs
│   │   │   ├── ArticlerReviewedEvent.cs
│   │   │   └── Dtos/                            # ArticleDto, ActorDto, AssetDto, JournalDto, PersonDto
│   │   ├── Journals/
│   │   │   ├── JournalCreatedEvent.cs
│   │   │   ├── JournalUpdatedEvent.cs
│   │   │   └── Dtos/JournalDto.cs
│   │   └── Persons/
│   │       ├── PersonUpdatedEvent.cs
│   │       └── Dtos/PersonDto.cs
│   │
│   ├── Articles.Security/                       # JWT config, role constants, auth handlers
│   │   ├── ArticleAccessAuthorizationHandler.cs
│   │   ├── ArticleRoleRequirement.cs
│   │   ├── ConfigureAuthentication.cs
│   │   ├── Extensions.cs
│   │   └── Role.cs                              # Role constant strings
│   │
│   ├── Blocks.AspNetCore/                       # ASP.NET Core cross-cutting
│   │   ├── Extensions/
│   │   ├── Filters/AssignUserIdFilter.cs        # IEndpointFilter for Minimal APIs
│   │   ├── Grpc/
│   │   │   ├── GrpcClientRegistrationExtensions.cs  # AddCodeFirstGrpcClient<T>
│   │   │   └── GrpcServicesOptions.cs
│   │   ├── HttpContextProvider.cs               # Implements IClaimsProvider + IRouteProvider
│   │   ├── Middlewares/
│   │   │   ├── GlobalExceptionMiddleware.cs
│   │   │   ├── RequestContextMiddleware.cs
│   │   │   └── RequestDiagnosticsMiddleware.cs
│   │   └── ModelBinding/
│   │
│   ├── Blocks.Core/                             # Utilities, guards, caching, extensions
│   │   ├── Cache/
│   │   │   ├── ICacheable.cs
│   │   │   └── ThreadSafeMemoryCache.cs
│   │   ├── Context/RequestContext.cs            # CorrelationId, file-transfer flags
│   │   ├── FluentValidation/
│   │   │   ├── Extensions.cs                    # NotEmptyWithMessage, MaximumLengthWithMessage, etc.
│   │   │   └── ValidationMessages.cs
│   │   ├── Guard.cs                             # Guard.NotFound, Guard.ThrowIfNull, etc.
│   │   ├── GuardExtensions.cs                   # .OrThrowNotFound()
│   │   ├── Mapster/
│   │   │   ├── DependencyInjection.cs           # AddMapsterConfigsFromCurrentAssembly()
│   │   │   └── Extensions.cs                    # AdaptWith(), MapToConstructor()
│   │   ├── MaxLength.cs                         # C0, C8, C16, C32, C64, C128, C256, C512, C1024, C2048
│   │   └── Security/
│   │       ├── IClaimsProvider.cs
│   │       └── JwtOptions.cs
│   │
│   ├── Blocks.Domain/                           # Base domain types
│   │   ├── DomainException.cs
│   │   ├── Entities/
│   │   │   ├── AggregateRoot.cs                 # AggregateRoot<T> with audit + domain events
│   │   │   ├── Entity.cs                        # Entity<T> with identity/equality
│   │   │   ├── EnumEntity.cs
│   │   │   ├── IAuditedEntity.cs
│   │   │   └── TenantEntity.cs
│   │   ├── IDomainEvent.cs                      # Extends INotification + IEvent
│   │   ├── IDomainEventPublisher.cs
│   │   └── ValueObjects/
│   │       ├── ValueObject.cs                   # Abstract with GetEqualityComponents()
│   │       ├── SingleValueObject.cs             # For value structs
│   │       └── StringValueObject.cs             # For string values
│   │
│   ├── Blocks.EntityFrameworkCore/              # EF Core infrastructure
│   │   ├── ApplicationDbContext.cs              # Base with GetAllCached<T>()
│   │   ├── TenantDbContext.cs                   # Multi-tenancy query filters
│   │   ├── TenantOptions.cs
│   │   ├── EntityConfigurations/
│   │   │   ├── EntityConfiguration.cs           # Base IEntityTypeConfiguration<T>
│   │   │   ├── AuditedEntityConfiguration.cs    # Adds CreatedOn, CreatedById columns
│   │   │   ├── EnumEntityConfiguration.cs
│   │   │   ├── MetadataConfiguration.cs
│   │   │   └── TenantEntityConfiguration.cs
│   │   ├── Extensions/
│   │   │   ├── DbContextExtensions.DomainEvents.cs  # DispatchDomainEventsAsync()
│   │   │   └── ModelBuilderExtensions.cs        # UseEntityTypeNamesAsTables()
│   │   ├── Interceptors/
│   │   │   ├── DispatchDomainEventsInterceptor.cs
│   │   │   └── TransactionalDispatchDomainEventsInterceptor.cs
│   │   └── Repositories/
│   │       ├── IRepository.cs
│   │       ├── Repository.cs                    # RepositoryBase<TContext, TEntity, TKey>
│   │       ├── CachedRepository.cs
│   │       └── TenantRepositoryBase.cs
│   │
│   ├── Blocks.Exceptions/                       # HTTP exception hierarchy
│   │   ├── HttpException.cs
│   │   ├── BadRequestException.cs
│   │   ├── NotFoundException.cs
│   │   └── UnauthorizedException.cs
│   │
│   ├── Blocks.FastEndpoints/                    # FastEndpoints cross-cutting
│   │   ├── AssignUserIdPreProcessor.cs          # IGlobalPreProcessor implementation
│   │   ├── DomainEventPublisher.cs              # Uses FastEndpoints PublishAsync
│   │   └── Extensions.cs                        # UseCustomFastEndpoints()
│   │
│   ├── Blocks.Http.Abstractions/                # HTTP/file abstractions used across services
│   │
│   ├── Blocks.MediatR/                          # MediatR cross-cutting
│   │   ├── Abstractions/
│   │   │   ├── ICommand.cs                      # extends IRequest<T>
│   │   │   └── IQuery.cs                        # extends IRequest<T>
│   │   ├── Behaviors/
│   │   │   ├── AssignUserIdBehavior.cs
│   │   │   ├── LoggingBehavior.cs
│   │   │   └── ValidationBehavior.cs
│   │   └── DomainEventPublisher.cs              # Uses MediatR Publish
│   │
│   ├── Blocks.Messaging/                        # MassTransit/RabbitMQ
│   │   ├── MassTransit/
│   │   │   ├── DependencyInjection.cs           # AddMassTransitWithRabbitMQ()
│   │   │   └── SnakeCaseWithServiceSuffixNameFormatter.cs
│   │   └── RabbitMqOptions.cs
│   │
│   ├── Blocks.Hasura/                           # Hasura GraphQL metadata management
│   │
│   └── Blocks.Redis/                            # Redis.OM repository
│       ├── Entity.cs                            # Base entity with [RedisIdField]
│       ├── Repository.cs                        # Redis-backed Repository<T>
│       └── Extensions.cs
│
├── Modules/
│   ├── ArticleTimeline/                         # Cross-service timeline module
│   │   ├── ArticleTimeline.Application/
│   │   │   ├── EventHandlers/                   # Handles ArticleStageChanged, AssetActionExecuted
│   │   │   └── VariableResolvers/
│   │   ├── ArticleTimeline.Domain/
│   │   │   └── Entities/                        # Timeline, TimelineTemplate, TimelineVisibility
│   │   └── ArticleTimeline.Persistence/
│   │       └── Repositories/TimelineRepository.cs
│   │
│   ├── EmailService/
│   │   ├── EmailService.Contracts/              # IEmailService, EmailMessage, EmailOptions
│   │   ├── EmailService.Empty/                  # No-op dev implementation
│   │   ├── EmailService.SendGrid/               # SendGrid implementation
│   │   └── EmailService.Smtp/                   # SMTP implementation
│   │
│   └── FileService/
│       ├── FileService.Contracts/               # IFileService, FileMetadata, FileUploadRequest
│       ├── FileService.AzureBlob/               # Azure Blob Storage implementation
│       ├── FileService.MongoGridFS/             # MongoDB GridFS implementation
│       └── FileStorage.MinIO/                   # MinIO implementation
│
└── Services/
    ├── Auth/                                    # Port 4401/4451, FastEndpoints, SQL Server
    │   ├── Auth.API/
    │   ├── Auth.Application/
    │   ├── Auth.Domain/
    │   └── Auth.Persistence/
    ├── Journals/                                # Port 4402/4452, FastEndpoints, Redis
    │   ├── Journals.API/
    │   ├── Journals.Domain/
    │   └── Journals.Persistence/
    ├── ArticleHub/                              # Port 4403/4453, Carter, PostgreSQL
    │   ├── ArticleHub.API/
    │   ├── ArticleHub.Domain/
    │   └── ArticleHub.Persistence/
    ├── Submission/                              # Port 4404/4454, Minimal APIs + MediatR, SQL Server
    │   ├── Submission.API/
    │   ├── Submission.Application/
    │   ├── Submission.Domain/
    │   └── Submission.Persistence/
    ├── Review/                                  # Port 4405/4455, Carter + MediatR, SQL Server
    │   ├── Review.API/
    │   ├── Review.Application/
    │   ├── Review.Domain/
    │   └── Review.Persistence/
    └── Production/                              # Port 4406/4456, FastEndpoints, SQL Server + Azure Blob
        ├── Production.API/
        ├── Production.Application/
        ├── Production.Domain/
        └── Production.Persistence/
```

### 1.3 Service Internal Folder Pattern

- `*.API/` — host startup, DI, HTTP endpoints, gRPC services, consumers, OpenAPI
- `*.Application/` — MediatR commands/queries/handlers, validators, mapping, service adapters
- `*.Domain/` — aggregates, entities, value objects, enums, domain events, behaviors
- `*.Persistence/` — EF Core or Redis setup, entity configurations, repositories, seed files

Example: Submission service

```text
src/Services/Submission/
├─ Submission.API/
│  ├─ Endpoints/
│  ├─ Properties/
│  ├─ DependecyInjection.cs
│  ├─ Program.cs
│  └─ appsettings.json
├─ Submission.Application/
│  ├─ Features/
│  ├─ Mappings/
│  └─ DependencyInjection.cs
├─ Submission.Domain/
│  ├─ Behaviors/
│  ├─ Entities/
│  ├─ Enums/
│  ├─ Events/
│  ├─ ValueObjects/
│  └─ GlobalUsings.cs
└─ Submission.Persistence/
   ├─ Data/
   │  ├─ Master/
   │  └─ Test/
   ├─ EntityConfigurations/
   ├─ Repositories/
   ├─ SubmissionDbContext.cs
   └─ DependencyInjection.cs
```

### 1.4 Project Dependency Directions

Services follow this layering (each layer references only the layers below it):

```
API → Application → Domain
API → Persistence → Domain
API → BuildingBlocks (AspNetCore, Security, Messaging, Modules)
Application → BuildingBlocks (Core, MediatR, Messaging)
Persistence → BuildingBlocks (EntityFrameworkCore)
```

Notable exceptions:
- **Auth.API** — directly references Auth.Domain and Auth.Persistence (no Application indirection for most features)
- **Journals.API** — no Application layer; handler logic lives directly in endpoint classes
- **Production.API** — no Application layer; features live in API project; references ArticleTimeline module

*(rules: `reference-model.md` → Layering & BuildingBlocks, layering-1..5)*

### 1.5 Dependency Inventory By Service

#### Auth

- `Auth.API` → `Articles.Security`, `Blocks.AspNetCore`, `Blocks.FastEndpoints`, `Articles.Grpc.Contracts`, `EmailService.Empty`, `EmailService.Smtp`, `Auth.Application`, `Auth.Persistence`, `Auth.Domain`
- `Auth.Persistence` → `Blocks.EntityFrameworkCore`, `Auth.Domain`

#### Journals

- `Journals.API` → `Articles.Grpc.Contracts`, `Articles.Integration.Contracts`, `Articles.Security`, `Blocks.FastEndpoints`, `Blocks.Messaging`, `Journals.Domain`, `Journals.Persistence`
- `Journals.Persistence` → `Blocks.Redis`

#### Submission

- `Submission.API` → `Blocks.AspNetCore`, `Articles.Security`, `Articles.Grpc.Contracts`, `Blocks.Messaging`, `EmailService.Empty`, `FileStorage.MongoGridFS`, `Submission.Application`
- `Submission.Application` → `Articles.Grpc.Contracts`, `Blocks.MediatR`, `Blocks.Messaging`, `Blocks.Http.Abstractions`, `FileStorage.Contracts`, `Articles.Integration.Contracts`, `Submission.Persistence`
- `Submission.Persistence` → `Blocks.EntityFrameworkCore`, `Submission.Domain`

#### Review

- `Review.API` → `Blocks.AspNetCore`, `Articles.Security`, `Blocks.Messaging`, `EmailService.Empty`, `FileStorage.MongoGridFS`, `Review.Application`
- `Review.Application` → `Articles.Grpc.Contracts`, `Blocks.Core`, `Blocks.MediatR`, `Blocks.Messaging`, `Blocks.Http.Abstractions`, `EmailService.Contracts`, `FileStorage.Contracts`, `Review.Persistence`
- `Review.Persistence` → `Blocks.EntityFrameworkCore`, `Review.Domain`

#### Production

- `Production.API` → `Articles.Abstractions`, `Articles.Integration.Contracts`, `Articles.Security`, `Blocks.AspNetCore`, `Blocks.Core`, `Blocks.FastEndpoints`, `Blocks.MediatR`, `Blocks.Messaging`, `EmailService.Empty`, `FileStorage.AzureBlob`, `FileStorage.MongoGridFS`, `Production.Application`

#### ArticleHub

- `ArticleHub.API` → `Blocks.AspNetCore`, `Blocks.Messaging`, `Articles.Security`, `Articles.Integration.Contracts`, `ArticleHub.Persistence`

### 1.6 Central Package Management

The repo uses central package management via `src/Directory.Packages.props`, plus a nested override at `src/Services/Review/Review.API/Directory.Packages.props`.

*(rules: `reference-model.md` → Conventions & Naming, conventions-1)*

---

## 2. Architecture Patterns

Pattern rules — Clean Architecture layering, Vertical Slice organization, CQRS, the DDD aggregate/behavior split, the domain-event side channel, and the domain→integration event boundary — are maintained, evidence-backed, in [`docs/reference-model.md`](reference-model.md). This inventory does not restate them.

*(rules: `reference-model.md` → Layering & BuildingBlocks; Domain Modeling; Application / CQRS Pipeline; Boundaries & Messaging)*

---

## 3. Domain Models per Service

### 3.1 Auth Domain

**Aggregates:** `User` (extends `IdentityUser<int>`, manually implements `IAggregateRoot`), `Person` (extends `AggregateRoot`)

**Entities:** `UserRole`, `RefreshToken`, `Role`

**Value Objects:** `EmailAddress` (StringValueObject), `HonorificTitle`, `ProfessionalProfile`

**Domain Events:** `UserCreated(User User, string RessetPasswordToken)` — raised from `CreateUserEndpoint` via `PublishAsync` (the aggregate-level `AddDomainEvent` call is commented out), handled by `SendConfirmationEmailOnUserCreatedHandler`

**Files:**
- Persons: `Person.cs`, `Behaviors/Person.cs`, `ValueObjects/EmailAddress.cs`, `ValueObjects/HonorificTitle.cs`, `ValueObjects/ProfessionalProfile.cs`
- Roles: `Role.cs`
- Users: `User.cs`, `UserRole.cs`, `RefreshToken.cs`, `IUserCreationInfo.cs`, `Behaviors/User.cs`, `Behaviors/UserRole.cs`, `Events/UserCreated.cs`

**User aggregate** (`src/Services/Auth/Auth.Domain/Users/User.cs`):
```csharp
public partial class User : IdentityUser<int>, IAggregateRoot
{
    public DateTime RegistrationDate { get; init; } = DateTime.UtcNow;
    public int PersonId { get; set; }
    public Person Person { get; init; } = null!;

    private List<UserRole> _userRoles = new();
    public virtual IReadOnlyList<UserRole> UserRoles => _userRoles;
    private List<RefreshToken> _refreshTokens = new();
    public virtual IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens;

    // Audit — manual because Identity base class already exists
    public int CreatedById { get; init; } = default!;
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
    public int? LastModifiedById { get; set; } = 0;
    public DateTime? LastModifiedOn { get; set; } = DateTime.UtcNow;

    // Domain events — manual because IAggregateRoot is implemented explicitly
    private List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    public void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

**Validation / Invariants:**
- `User.AssignRefreshToken` guards against null
- `Person` value objects encapsulate email/profile shape
- Auth request validation is mostly endpoint/command-side, not domain-side

**Relationships:**
- `Person` owns `Email`, `Honorific`, and `ProfessionalProfile`
- `Person` <-> `User` is a persisted association
- `User` owns `_refreshTokens` and `_userRoles`

### 3.2 Journals Domain

**Note:** Journals uses Redis.OM, not EF Core. The domain entity extends `Blocks.Redis.Entity`, not `AggregateRoot`.

**Entity:** `Journal` (extends `Blocks.Redis.Entity`), `Section`, `Editor`

**Value Object:** `SectionEditor`

**Enums:** `EditorRole`

**Domain Events:** `JournalCreated`, `JournalUpdated`, `SectionUpdated` — raised from endpoints via FastEndpoints `PublishAsync`, not from aggregate root queue

**Journal entity** (`src/Services/Journals/Journals.Domain/Journals/Journal.cs`):
```csharp
[Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(Journal) })]
public partial class Journal : Entity
{
    [Indexed]
    public required string Abbreviation { get; set; }

    private string _name = null!;
    [Searchable]
    public required string Name
    {
        get => _name;
        set { _name = value; NormalizedName = _name.ToLowerInvariant(); }
    }
    [Indexed(Sortable = true)]
    public required string NormalizedName { get; set; }

    [Searchable]
    public required string Description { get; set; }
    public required string ISSN { get; set; }
    public int ChiefEditorId { get; set; }

    [Indexed(JsonPath = "$.Name")]
    public List<Section> Sections { get; set; } = new();

    public int ArticlesCount { get; set; }
}
```

**Relationships:** `Journal` contains sections, `Section` contains editor-role assignments via `SectionEditor`. All persisted as Redis documents.

### 3.3 Submission Domain

**Aggregate roots:** `Article`, `Asset`

**Child entities:** `ArticleActor`, `ArticleAuthor`, `ArticleAction`, `StageHistory`, `Stage`, `AssetTypeDefinition`, `ArticleStageTransition`, `Author` (extends `Person`), `Person`, `Journal`

**Value Objects:** `EmailAddress`, `AssetName`, `AssetNumber`, `FileName`, `FileExtension`, `FileExtensions`, `File`

**Enums (local):** `ArticleActionType`, `AssetState`, `AssetType`

**Domain Events And Where They Are Raised:**
- `ArticleStageChanged` — `Behaviors/Article.cs` `SetStage`
- `AuthorAssigned` — `Behaviors/Article.cs` `AssignAuthor`
- `ArticleApproved` — `Behaviors/Article.cs` `Approve`
- `ArticleRejected` — `Behaviors/Article.cs` `Reject`
- `ArticleActionExecuted` — `Behaviors/Article.cs` `AddAction`
- `ArticleCreated` — `Behaviors/Journal.cs` when article is created for a journal
- `AuthorCreated` — `Behaviors/Author.cs`

**Validation / Invariants:**
- Stage-machine checks gate legal article actions
- `Submit` verifies mandatory contribution areas
- `AssignAuthor` rejects duplicate role/person combinations
- Asset creation checks type count limits
- Query/command validators in `Submission.Application/Features/**`

**Relationships:**

```csharp
// src/Services/Submission/Submission.Persistence/EntityConfigurations/ArticleEntityConfiguration.cs
builder.HasOne(e => e.Journal).WithMany(e => e.Articles)
    .HasForeignKey(e => e.JournalId).IsRequired().OnDelete(DeleteBehavior.Restrict);

builder.HasMany(e => e.Assets).WithOne(e => e.Article)
    .HasForeignKey(e => e.ArticleId).IsRequired().OnDelete(DeleteBehavior.Cascade);

builder.HasMany(e => e.Actors).WithOne()
    .HasForeignKey(e => e.ArticleId).IsRequired().OnDelete(DeleteBehavior.Cascade);
```

### 3.4 Review Domain

**Aggregate roots:** `Article`, `Reviewer`, `ReviewInvitation`

**Supporting entities:** `ArticleActor`, `ArticleAuthor`, `ArticleAction`, `Asset`, `File`, `ReviewerSpecialization`, `Editor`, `Author`, `Stage`, `StageHistory`, `AssetTypeDefinition`

**Value Objects:** `EmailAddress`, `AssetName`, `AssetNumber`, `FileName`, `FileExtension`, `FileExtensions`, `File`, `InvitationToken`, `Invitee`

**Enums (local):** `ArticleActionType`, `AssetState`, `AssetTypeCategories`, `InvitationStatus`

**Domain Events And Where They Are Raised:**
- `ArticleStageChanged` — `Articles/Behaviors/Article.cs` `SetStage`
- `EditorAssigned` — `Articles/Behaviors/Article.cs` `AssignEditor`
- `ReviewerAssigned` — `Articles/Behaviors/Article.cs` `AssignReviewer`
- `ArticleAccepted` — `Articles/Behaviors/Article.cs` `Accept`
- `ArticleRejected` — `Articles/Behaviors/Article.cs` `Reject`
- `ReviewerInvited` — `Articles/Behaviors/Article.cs` `CreateInvitation`
- `ArticleActionExecuted` — `Articles/Behaviors/Article.cs` `AddAction`
- `FileUploaded` — `Assets/Behaviors/Asset.cs` `UploadFile`
- `ReviewerCreated` — `Reviewers/Behaviors/Reviewer.cs` factory/create path

**ReviewInvitation aggregate** (`src/Services/Review/Review.Domain/Invitations/ReviewInvitation.cs`):
```csharp
public partial class ReviewInvitation : AggregateRoot
{
    public required int ArticleId { get; init; }
    public int? UserId { get; init; }
    public required EmailAddress Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required int SentById { get; set; }
    public required DateTime ExpiresOn { get; init; }
    public bool IsExpired => ExpiresOn < DateTime.UtcNow;
    public required InvitationToken Token { get; init; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Open;
}
```

**Validation / Invariants:**
- Prevents duplicate editor/reviewer assignments
- Invitation creation blocks duplicate active invitations
- Reviewer specialization must match journal before inviting
- Asset/file rules enforce state/type transitions

### 3.5 Production Domain

**Aggregate roots:** `Article`, `Asset`

**Supporting entities:** `ArticleActor`, `Typesetter`, `ProductionOfficer`, `AssetAction`, `AssetCurrentFileLink`, `File`, `StageHistory`, `AssetTypeDefinition`, `AssetStateTransition`, `Author` (extends `Person`), `Person`, `Journal`

**Value Objects:** `AssetName`, `AssetNumber`, `FileName`, `FileExtension`, `AllowedFileExtensions`, `FileVersion`

**Custom exception:** `TypesetterAlreadyAssignedException` (extends `DomainException`)

**Domain Events And Where They Are Raised:**
- `ArticleAcceptedForProduction` — `Articles/Behaviors/Article.cs` when materializing from review
- `ArticleStageChanged` — `Articles/Behaviors/Article.cs` `SetStage`
- `TypesetterAssigned` — `Articles/Behaviors/Article.cs` `AssignTypesetter`
- `AssetActionExecuted` — `Assets/Behaviors/Asset.cs` `AddAction`

**Validation / Invariants:**
- Stage machine controls legal article actions
- Assigning a second typesetter throws `TypesetterAlreadyAssignedException`
- Asset/file actions are stage/type constrained

### 3.6 ArticleHub Domain

**Purpose:** Read-only projection. No aggregates, no domain logic.

**Entities:** `Article`, `ArticleActor`, `Journal`, `Person`

**DTOs:** `ArticleDto`, `ArticleMinimalDto`, `ActorDto`, `JournalDto`, `PersonDto`

Database: PostgreSQL (Npgsql). Writes come from integration consumers; reads via Hasura GraphQL and EF lookups.

*(rules: `reference-model.md` → Consumer / Projection, consumers-5)*

### 3.7 ArticleTimeline Module Domain

**Entities:** `Timeline`, `TimelineTemplate`, `TimelineVisibility`

This is a module domain, not a top-level microservice domain. It stores timeline projections generated from article domain events.

```csharp
// src/Modules/ArticleTimeline/ArticleTimeline.Application/EventHandlers/AddTimelineEventHandler.cs
public abstract class AddTimelineEventHandler<TDomainEvent, TAction>(TransactionProvider _transactionProvider, TimelineRepository _timelineRepository, DbContext _dbContext, VariableResolverFactory _variableResolverFactory)
    : INotificationHandler<TDomainEvent>
    where TDomainEvent : DomainEvent<TAction>
    where TAction : IArticleAction
{
    public async Task Handle(TDomainEvent eventModel, CancellationToken ct)
    {
        _dbContext.Database.UseTransaction(await _transactionProvider.GetCurrentTransaction(ct));
        // ...
        await _timelineRepository.AddAsync(timeline);
        await _timelineRepository.SaveChangesAsync();
    }
}
```

### 3.8 Shared Enums (Articles.Abstractions)

**ArticleStage** (`src/BuildingBlocks/Articles.Abstractions/Enums/ArticleStage.cs`):
```csharp
public enum ArticleStage : int
{
    None = 0,
    // Submission: 101-105
    Created = 101, ManuscriptUploaded = 102, Submitted = 103,
    InitialRejected = 104, InitialApproved = 105,
    // Review: 201-205
    UnderReview = 201, ReadyForDecision = 202, AwaitingRevision = 203,
    Rejected = 204, Accepted = 205,
    // Production: 300-305
    InProduction = 300, DraftProduction = 301, FinalProduction = 302,
    PublicationScheduled = 304, Published = 305
}
```

The enum additionally carries static `HashSet<ArticleStage>` groupings (additive helper sets).

**UserRoleType** (`src/BuildingBlocks/Articles.Abstractions/Enums/UserRoleType.cs`):
```csharp
public enum UserRoleType : int
{
    EOF = 1,        // Editorial Office Admin
    AUT = 11,       // Author
    CORAUT = 12,    // Corresponding Author
    REVED = 21,     // Review Editor
    REV = 22,       // Reviewer
    POF = 31,       // Production Office Admin
    TSOF = 32,      // Typesetter
    USERADMIN = 91  // User Admin
}
```

*(rules: `reference-model.md` → Security & Identity Wiring, security-1)*

---

## 4. Endpoint Frameworks

Three endpoint frameworks coexist; the choice is a per-service decision. This table is the inventory of which framework each service uses and where its handler logic physically lives.

| Service | Framework | Where handler logic lives |
| --- | --- | --- |
| Auth | FastEndpoints | in the endpoint's `HandleAsync()`; features under `Auth.API/Features/{Domain}/` |
| Journals | FastEndpoints | inside the endpoint class — no separate handler file; features under `Journals.API/Features/` |
| Submission | Minimal APIs + MediatR | static-class endpoints (`Map(IEndpointRouteBuilder)`) dispatch via `ISender`; handlers in `Submission.Application/Features/` |
| Review | Carter + MediatR | `ICarterModule` endpoints dispatch via `ISender`; handlers in `Review.Application/Features/` |
| Production | FastEndpoints | in the endpoint's `HandleAsync()` (via `BaseEndpoint<TCommand,TResponse>`); features in `Production.API/Features/` (no Application project) |
| ArticleHub | Carter | read-only; handler logic inline in the endpoint |

*(rules: `reference-model.md` → Endpoint & Feature Slicing, endpoints-1..5; Security & Identity Wiring, security-1..5)*

---

## 5. Persistence Inventory

Storage engine, DbContext, and domain-event interceptor variant per service. Journals has `JournalDbContext` wrapping `RedisConnectionProvider` (no EF); all other DbContexts derive from `ApplicationDbContext<T>` except Auth's, which derives from `IdentityDbContext<User, Role, int>`.

| Service | Storage engine | DbContext | Interceptor variant |
| --- | --- | --- | --- |
| Auth | SQL Server | `AuthDbContext` (`IdentityDbContext`) | none — domain events dispatched via FastEndpoints `PublishAsync`, not an interceptor |
| Journals | Redis (Redis.OM) | `JournalDbContext` (wraps `RedisConnectionProvider`) | n/a — no EF |
| ArticleHub | PostgreSQL (Npgsql) | `ArticleHubDbContext` | none — write model, no event-dispatch pipeline |
| Submission | SQL Server | `SubmissionDbContext` | `DispatchDomainEventsInterceptor` |
| Review | SQL Server | `ReviewDbContext` | `DispatchDomainEventsInterceptor` |
| Production | SQL Server | `ProductionDbContext` | `TransactionalDispatchDomainEventsInterceptor` |
| ArticleTimeline (module) | SQL Server | `ArticleTimelineDbContext` | joins the trigger's transaction via `TransactionProvider` |

Migrations are added/applied per the commands in the root `CLAUDE.md` and applied on startup via `app.Migrate<TDbContext>()`.

*(rules: `reference-model.md` → Persistence, persistence-1..5)*

---

## 6. Cross-Service Communication (gRPC)

Synchronous cross-service calls use code-first gRPC (protobuf-net.Grpc). The contract inventory:

**Contracts** — defined once in `Articles.Grpc.Contracts`, organized by owning service:
- `IPersonService` — `Auth/PersonContracts.cs`; DTOs `PersonInfo`, `GetPersonRequest`/`GetPersonResponse`, `GetPersonByUserIdRequest`, `GetPersonByEmailRequest`, `CreatePersonRequest`/`CreatePersonResponse`
- `IJournalService` — `Journals/JournalContracts.cs`; DTOs `JournalInfo`, `GetJournalByIdRequest`/`GetJournalResponse`, `IsEditorAssignedToJournalRequest`/`IsEditorAssignedToJournalResponse`

**Servers:**
- `PersonGrpcService` (implements `IPersonService`) — hosted by **Auth**, gRPC port `4401`
- `JournalGrpcService` (implements `IJournalService`) — hosted by **Journals**, gRPC port `4402`

**Clients:** Submission registers `IPersonService` + `IJournalService`; Journals registers `IPersonService`; Review references the contracts (`Review.Application` → `Articles.Grpc.Contracts`). ArticleHub registers **no** gRPC client — it is populated only from integration-event DTOs (datasharing-4).

**`.proto` files:** three `.proto` files sit alongside the code-first interfaces in `Articles.Grpc.Contracts` (including `Submission/ArticleRequest.proto`, which is absent from the §1.2 tree).

**Dead reference:** `src/Submission.sln` still references a missing legacy client project (`UserServiceGrpcClient.csproj`); `src/Articles.sln` likewise references a missing client project. No live `ClientServices/` folder exists on disk.

*(rules: `reference-model.md` → Cross-Service Data Access, datasharing-1..5; Boundaries & Messaging, boundaries-4)*

---

## 7. Messaging and Events (MassTransit)

Integration events are flat C# records in `Articles.Integration.Contracts`, grouped by owning domain:

- **Articles/** — `ArticleAcceptedForProductionEvent`, `ArticleApprovedForReviewEvent`, `ArticlePublishedEvent`, `ArticlerReviewedEvent` (filename typo is real)
- **Journals/** — `JournalCreatedEvent`, `JournalUpdatedEvent`
- **Persons/** — `PersonUpdatedEvent`

DTOs carried by these events (`ArticleDto`, `ActorDto`, `AssetDto`, `JournalDto`, `PersonDto`) live under each group's `Dtos/` folder.

*(rules: `reference-model.md` → Boundaries & Messaging, boundaries-1/2/4/5; Consumer / Projection, consumers-1..5; Domain-Event Publication Plumbing, eventpub-1..5)*

---

## 8. BuildingBlocks

Package inventory — one line of purpose each.

| Package | Purpose |
| --- | --- |
| `Articles.Abstractions` | Shared article action/event abstractions: `ArticleCommandBase`, `DomainEvent<T>`, `ArticleStageChanged`, `IArticleAction`, `ArticleStage`, `UserRoleType` |
| `Articles.Grpc.Contracts` | Code-first gRPC service interfaces + proto contracts (`IPersonService`, `IJournalService`, `PersonInfo`, `JournalInfo`) |
| `Articles.Integration.Contracts` | Cross-service MassTransit event records + their DTOs |
| `Articles.Security` | JWT auth and article access authorization (`ConfigureAuthentication`, `ArticleAccessAuthorizationHandler`, `Role`, `ArticleRoleRequirement`) |
| `Blocks.AspNetCore` | Middleware, HTTP providers, filters, gRPC client helpers (`GlobalExceptionMiddleware`, `HttpContextProvider`, `AssignUserIdFilter`, `AddCodeFirstGrpcClient<T>`) |
| `Blocks.Core` | Guards, FluentValidation extensions, caching, Mapster helpers, `MaxLength`, `IClaimsProvider`, `JwtOptions`, `RequestContext` |
| `Blocks.Domain` | Base domain types (`AggregateRoot<T>`, `Entity<T>`, `ValueObject`, `IDomainEvent`, `IDomainEventPublisher`, `DomainException`) |
| `Blocks.EntityFrameworkCore` | EF base context, repositories, interceptors, entity-configuration ladder |
| `Blocks.Exceptions` | HTTP exception hierarchy (`HttpException`, `BadRequestException`, `NotFoundException`, `UnauthorizedException`) |
| `Blocks.FastEndpoints` | FastEndpoints host helpers (`DomainEventPublisher`, `AssignUserIdPreProcessor`, `UseCustomFastEndpoints()`) |
| `Blocks.Http.Abstractions` | HTTP/file abstractions used by Submission and Review |
| `Blocks.MediatR` | CQRS abstractions and pipeline behaviors (`ICommand`, `IQuery<T>`, `DomainEventPublisher`, `AssignUserIdBehavior`, `ValidationBehavior`, `LoggingBehavior`) |
| `Blocks.Messaging` | MassTransit/RabbitMQ setup (`AddMassTransitWithRabbitMQ()`, `RabbitMqOptions`, `SnakeCaseWithServiceSuffixNameFormatter`) |
| `Blocks.Redis` | Redis.OM repository (`Entity`, `Repository<T>`) |
| `Blocks.Hasura` | Hasura GraphQL / metadata setup (`HasuraRegistration`, `HasuraMetadataInitService`) |

*(rules: `reference-model.md` → Layering & BuildingBlocks, layering-2; Error Handling, errors-1..5)*

---

## 9. Mapping Patterns

### 9.1 Mapster IRegister Pattern

All Mapster configurations implement `IRegister` and are discovered by assembly scan:

```csharp
// Registration
services.AddMapsterConfigsFromAssemblyContaining<GrpcMappings>();
services.AddMapsterConfigsFromCurrentAssembly();

// Internal: TypeAdapterConfig.GlobalSettings.Scan(assembly)
```

### 9.2 Configuration Examples

**gRPC mappings** (`src/Services/Submission/Submission.Application/Mappings/GrpcMappings.cs`):
```csharp
public class GrpcMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<string, EmailAddress>()
            .MapWith(src => EmailAddress.Create(src));

        config.ForType<PersonInfo, Author>()
            .Map(dest => dest.UserId, src => src.Id)
            .Ignore(dest => dest.Id);
    }
}
```

**REST endpoint mappings** (`src/Services/Submission/Submission.Application/Mappings/RestEndpointMappings.cs`):
```csharp
public class RestEndpointMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ArticleActor, ActorDto>()
            .Include<ArticleAuthor, ActorDto>();

        config.NewConfig<Person, PersonDto>()
            .Include<Author, PersonDto>();

        config.NewConfig<IArticleAction<ArticleActionType>, ArticleAction>()
            .Map(dest => dest.TypeId, src => src.ActionType);
    }
}
```

### 9.3 In-Handler Mapping

```csharp
// In a consumer
var article = articleDto.AdaptWith<Article>(article =>
{
    article.Journal = journal;
    article.SubmittedById = articleDto.SubmittedBy.Id;
});

// In a handler
return new GetArticleResonse(article.Adapt<ArticleDto>());

// In a gRPC service
return new GetJournalResponse { Journal = journal.Adapt<JournalInfo>() };
```

### 9.4 Where Mappings Are Registered

| Service | Mapping class | Location |
|---------|--------------|----------|
| Submission | `GrpcMappings`, `RestEndpointMappings` | `Submission.Application/Mappings/` |
| Review | `IntegrationEventMappings` (and others) | `Review.Application/Mappings/` |
| Journals | `MappingConfig` | `Journals.API/Features/_Shared/` |
| Production | `MappingConfig`, `FileResponseMappingProfile` | `Production.API/Features/_Shared/` |
| Auth | `GrpcTypeAdapterConfig` (singleton, gRPC-specific) | `Auth.API/Mappings/` |
| ArticleHub | `IntegrationEventsMappingConfig` | `ArticleHub.API/Articles/Consumers/` |

---

## 10. Naming Conventions

Naming rules — class/file/field naming, event naming, primary-ctor capture style, DbContext naming, table-name transforms, and route `:verb` conventions — live in [`docs/reference-model.md`](reference-model.md). The registry corrected several absolutes the original scan asserted: the primary-ctor capture style is not uniform (`dbContext` vs `_dbContext` between sibling repos), DbContext naming is not universally `{Service}DbContext` (Journals is `JournalDbContext`), and central-package management is not a single file (conventions-1/3/5, namingx-1).

*(rules: `reference-model.md` → Conventions & Naming, conventions-1..5; Cross-Stack Naming Transforms, namingx-1..4)*

---

## 11. Service-Specific Patterns

### 11.1 Port Assignments

| Service | Local Run | Docker Override | Notes |
| --- | --- | --- | --- |
| Auth | `4401` / `4451` | `4401:8080`, `4441:8081` | FastEndpoints + gRPC server |
| Journals | `4402` / `4452` | `4402:8080`, `4442:8081` | Redis-backed |
| ArticleHub | `4403` / `4453` | `4403:8080`, `4443:8081` | Postgres + Hasura |
| Submission | `4404` / `4454` | `4404:8080`, `4444:8081` | Minimal APIs + MediatR |
| Review | `4405` / `4455` | `4405:8080`, `4445:8081` | Carter + MediatR |
| Production | `4406` / `4456` | commented out | FastEndpoints + ArticleTimeline |
| ApiGateway | `5234` / `7131` | container profile only | YARP reverse proxy |

### 11.2 Auth Service

**Framework:** FastEndpoints (no MediatR for most features)

**Unique patterns:**
- Uses `AspNetCore.Identity` (`IdentityUser<int>`, `UserManager<User>`, `SignInManager<User>`)
- `User` implements `IAggregateRoot` manually (cannot extend `AggregateRoot<T>` because Identity already inherits from a base class)
- JWT token is generated by a `TokenFactory` application service
- gRPC server: `PersonGrpcService` — serves person data to all other services
- Domain events dispatched via FastEndpoints `PublishAsync()`, not via interceptor
- `AuthDbContext` extends `IdentityDbContext<User, Role, int>`, not `ApplicationDbContext`
- Email service registered: `AddEmptyEmailService` (can swap to `AddSmtpEmailService`)

**Startup:** `AddApiServices -> AddApplicationServices -> AddPersistenceServices -> Migrate<AuthDbContext> -> SeedTestData (dev) -> UseCustomFastEndpoints -> MapGrpcService<PersonGrpcService>`

### 11.3 Journals Service

**Framework:** FastEndpoints (no MediatR)

**Unique patterns:**
- Database is Redis (Redis.OM), not SQL Server — no EF Core, no migrations
- `Journal` extends `Blocks.Redis.Entity`, decorated with `[Document]`, `[Indexed]`, `[Searchable]`
- Handler logic lives directly in endpoint class (no separate handler file)
- ID generation via Redis `INCR` counter
- `ReplaceAsync()` used for full entity replacement (Redis.OM limitation with nested collections)
- gRPC server: `JournalGrpcService`
- gRPC client: `IPersonService` (to create editors from person data)
- `UseRedis()` creates Redis indices at startup
- MassTransit consumer: `ArticleApprovedForReviewConsumer` (increments `ArticlesCount`)

**Notable** — `SearchJournalsQueryHandler` combines query + handler in one endpoint class using Redis.OM `Raw()` for text search.

### 11.4 Submission Service

**Framework:** Minimal APIs + MediatR

**Unique patterns:**
- Endpoints are static classes with `Map(IEndpointRouteBuilder)` extension methods
- All endpoints wired via `EndpointRegistration.MapAllEndpoints()`
- `AssignUserIdFilter` added to the API group
- File upload endpoints use `[FromForm]` binding and `.DisableAntiforgery()`
- RPC-style URL convention: `{id}:upload` and `{id}:download`
- `ArticleStateMachineFactory` is a delegate factory
- No gRPC server; gRPC clients: `IPersonService` (Auth) + `IJournalService` (Journals)
- Database: SQL Server + MongoDB GridFS (files)
- MassTransit consumers: `JournalCreatedConsumer`, `JournalUpdatedConsumer`
- Request context / diagnostics middleware enabled in the host

### 11.5 Review Service

**Framework:** Carter + MediatR

**Unique patterns:**
- Carter endpoints implementing `ICarterModule`
- Route IDs set inline: `command.ArticleId = articleId` before sending to MediatR
- Two MongoDB GridFS connections: one for Review files, one for reading Submission files
- `FileServiceFactory` delegate: `Func<FileStorageType, IFileService>`

```csharp
// src/Services/Review/Review.API/FileServiceFactoryRegistration.cs
services.AddScoped<FileServiceFactory>(serviceProvider => fileStorageType =>
{
    return fileStorageType switch
    {
        FileStorageType.Submission => serviceProvider.GetRequiredService<IFileService<SubmissionFileStorageOptions>>(),
        FileStorageType.Review => serviceProvider.GetRequiredService<IFileService<MongoGridFsFileStorageOptions>>(),
        _ => throw new ApplicationException()
    };
});
```

- `ArticleAccessAuthorizationHandler` registered for resource-based authorization
- State machine for article workflow validation
- `DispatchDomainEventsInterceptor` (not transactional)

### 11.6 Production Service

**Framework:** FastEndpoints (no MediatR for features)

**Unique patterns:**
- No Application layer project; all features live in `Production.API/Features/`
- Uses `TransactionalDispatchDomainEventsInterceptor`
- Has `BaseEndpoint<TCommand, TResponse>` abstract class and `BaseValidator<T>` to reduce repetition
- `ArticleStateMachineFactory` is a delegate factory
- Loads `ArticleTimeline` module in-process, migrated side-by-side with main db
- Azure Blob as primary file store plus Mongo GridFS read access for review-file ingestion
- Both `AssignUserIdPreProcessor` (FastEndpoints) AND `AssignUserIdFilter` (Minimal APIs group) registered
- `MediatR` registered only for domain event publishing
- `IDomainEventPublisher` is `Blocks.MediatR.DomainEventPublisher`
- MassTransit consumer: `ArticleAcceptedForProductionConsumer`
- `TypesetterAlreadyAssignedException` — service-specific domain exception
- `production-api` commented out in docker-compose

### 11.7 ArticleHub Service

**Framework:** Carter

**Unique patterns:**
- Read-only service — no domain logic, no commands
- Database: PostgreSQL (Npgsql), not SQL Server
- Integrates with Hasura GraphQL engine for read endpoints
- `ArticleGraphQLReadStore` uses Hasura GraphQL API for queries
- No EF Core write operations in handlers; EF only used for query and in consumers
- All consumers write to the local PostgreSQL projection
- `HasuraMetadataInitService` (hosted service) tracks tables on startup

---

## 12. Testing Patterns

### 12.1 Automated Test Projects

None. No `*Tests*.csproj` files and no xUnit/NUnit/MSTest references are present anywhere in the repo.

### 12.2 Development/Test Seed Data

Several services bootstrap demo data from `Data/Test` JSON files at startup in development — Submission (`SeedFromJsonFile<Person>`/`<Journal>`), Auth (`LoadFromJson<Person>`, default password `Pass.123!`), and Journals (Redis `SeedFromJson<Editor>`/`<Journal>` plus `SetSequenceSeed`).

### 12.3 Manual API Testing

- `postman/ArticlesAPI.postman_collection.json` — organized by service (Auth & Users, Journals, Submission, Review, etc.)
- `postman/Demo.postman_environment.json`
- Sample uploaded files under `data/article-files/`

### 12.4 Gherkin / BDD Artifacts

A placeholder file `docs/gherkin/Review/AssignEditor.feature` exists but is empty (0 bytes).

---

## 13. Configuration and Infrastructure

### 13.1 Docker Compose Topology

```yaml
# src/docker-compose.yml
services:
  auth-api:          # depends_on: sqlserver
  journals-api:      # depends_on: journals-redisdb
  articleHub-api:    # depends_on: postgres, articlehub-hasura, rabbitmq
  submission-api:    # depends_on: sqlserver, rabbitmq, mongo-gridfs
  review-api:        # depends_on: sqlserver, rabbitmq
  # production-api:  # commented out

  sqlserver:         # mcr.microsoft.com/mssql/server
  journals-redisdb:  # redis/redis-stack
  postgres:          # postgres (with healthcheck)
  articlehub-hasura: # hasura/graphql-engine
  mongo-gridfs:      # mongo:6.0
  rabbitmq:          # rabbitmq:management
```

**Volumes:** `postgres_data`, `sqlserver_data`, `mongo_gridfs_data`, `rabbitmq_data`, `journals_redisdb_data`

**Infrastructure ports (docker override):**
- SQL Server: `1433`
- Redis Stack: `6379`, `8801`
- PostgreSQL: `5432`
- Hasura: `4493`
- MongoDB GridFS: `27018 -> 27017`
- RabbitMQ: `5672`, `15672`

Each service also carries its own `appsettings.json` (+ `appsettings.Development.json`) for connection strings, JWT, RabbitMQ, and gRPC service URLs.

### 13.2 API Gateway (YARP)

`src/ApiGateway/Program.cs` adds `AddReverseProxy().LoadFromConfig(...)` and injects an `X-Correlation-ID` header when absent before `MapReverseProxy()`.

**Drift:** the gateway route addresses do not match the current service HTTP ports for Submission, Production, and ArticleHub.

### 13.3 JWT Authentication

Configured once in `Articles.Security/ConfigureAuthentication.cs`: validates issuer + issuer-signing-key, skips audience, maps roles via `ClaimTypes.Role`.

*(rules: `reference-model.md` → Security & Identity Wiring, security-5)*

### 13.4 Configuration & Wiring Conventions

- Options bind through the shared `AddAndValidateOptions<T>()` (section-name = type-name, validated at startup) and `GetSectionByTypeName<T>()` helpers.
- Every layer of every service exposes a `DependencyInjection.cs` (`AddApiServices` / `AddApplicationServices` / `AddPersistenceServices`), composed in `Program.cs`. Note the recurring `DependecyInjection` typo in some API-layer files.
- Services share a middleware ordering: Swagger → routing → `GlobalExceptionMiddleware` → authentication → authorization, with framework-specific tails (`UseCustomFastEndpoints`, `MapCarter`, `MapAllEndpoints`, `UseRedis`).

*(rules: `reference-model.md` → Cross-Cutting Infrastructure, infra-4; Error Handling, errors-3)*

### 13.5 File Service Module Selection

| Service | Registration call | Storage backend |
|---------|-----------------|----------------|
| Submission | `AddMongoFileStorageAsSingletone(config)` | MongoDB GridFS |
| Review | `AddMongoFileStorageAsSingletone(config)` + `AddMongoFileStorageAsScoped<SubmissionFileStorageOptions>` | Two MongoDB GridFS buckets |
| Production | `AddAzureFileStorage(config)` + `AddMongoFileStorageAsScoped<ReviewFileStorageOptions>` | Azure Blob + MongoDB |

All implementations register as `IFileService` (or `IFileService<TOptions>` for multi-instance scenarios).

*(rules: `reference-model.md` → File-Storage Abstraction, filestorage-1..4)*

---

*Inventory of `src/` as of commit `cd4d0b1`. Verified pattern rules: [`docs/reference-model.md`](reference-model.md).*
