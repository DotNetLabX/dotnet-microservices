# Codebase Scan

Generated on `2026-03-22` for `D:\src\dotnet-microservices`.

This document is a repository scan, not a design proposal. It records the codebase as it exists on disk. Trees exclude generated/editor artifacts (`.vs/`, `bin/`, `obj/`) so the structure stays focused on source, docs, config, and deployable assets.


## Table of Contents

1. [Repository Structure](#1-repository-structure)
2. [Architecture Patterns](#2-architecture-patterns)
3. [Domain Models per Service](#3-domain-models-per-service)
4. [Endpoint Patterns](#4-endpoint-patterns)
5. [Persistence Patterns](#5-persistence-patterns)
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
│   │   ├── Behaviours/
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
│  ├─ Behaviours/
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

The repo uses central package version management from `src/Directory.Packages.props`. Review additionally has a local `src/Services/Review/Review.API/Directory.Packages.props` override file.

---

## 2. Architecture Patterns

### 2.1 Vertical Slice Structure

Each feature is a self-contained folder. A complete `CreateArticle` example in Submission:

**Folder layout:**
```
Submission.API/Endpoints/CreateArticleEndpoint.cs
Submission.Application/Features/CreateArticle/
    CreateArticleCommand.cs          # command + validator
    CreateArticleCommandHandler.cs   # handler
```

**Endpoint** (`src/Services/Submission/Submission.API/Endpoints/CreateArticleEndpoint.cs`):
```csharp
public static class CreateArticleEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/articles", async (CreateArticleCommand command, ISender sender) =>
        {
            var response = await sender.Send(command);
            return Results.Created($"/api/articles/{response.Id}", response);
        })
        .RequireRoleAuthorization(Role.Author)
        .WithName("CreateArticle")
        .WithTags("Articles")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
```

**Command + Validator** (`src/Services/Submission/Submission.Application/Features/CreateArticle/CreateArticleCommand.cs`):
```csharp
public record CreateArticleCommand(int JournalId, string Title, ArticleType Type, string Scope)
    : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.CreateArticle;
}

public class CreateArticleCommandValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmptyWithMessage(nameof(CreateArticleCommand.Title))
            .MaximumLengthWithMessage(MaxLength.C256, nameof(CreateArticleCommand.Title));

        RuleFor(x => x.Scope)
            .NotEmptyWithMessage(nameof(CreateArticleCommand.Scope))
            .MaximumLengthWithMessage(MaxLength.C2048, nameof(CreateArticleCommand.Scope));

        RuleFor(c => c.JournalId).GreaterThan(0).WithMessageForInvalidId(nameof(CreateArticleCommand.JournalId));
    }
}
```

**Handler** (`src/Services/Submission/Submission.Application/Features/CreateArticle/CreateArticleCommandHandler.cs`):
```csharp
public class CreateArticleCommandHandler(SubmissionDbContext _dbContext, Repository<Journal> _journalRepository, IJournalService _journalClient)
    : IRequestHandler<CreateArticleCommand, IdResponse>
{
    public async Task<IdResponse> Handle(CreateArticleCommand command, CancellationToken ct)
    {
        var journal = await _journalRepository.FindByIdAsync(command.JournalId);
        if (journal is null)
            journal = await CreateJournal(command);

        var article = journal.CreateArticle(command.Title, command.Type, command.Scope, command);

        await AssignCurrentUserAsAuthor(article, command);

        await _journalRepository.SaveChangesAsync(ct);

        return new IdResponse(article.Id);
    }
    // ...
}
```

### 2.2 Vertical Slice: Command Flow With Cross-Boundary Trace

`ApproveArticle` is the fuller example because it crosses endpoint -> validator -> command -> handler -> domain event -> integration event.

**Command + Validator** (`src/Services/Submission/Submission.Application/Features/ApproveArticle/ApproveArticleCommand.cs`):
```csharp
public record ApproveArticleCommand : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.ApproveDraft;
}

public class ApproveArticleCommandValidator : ArticleCommandValidator<ApproveArticleCommand>;
```

**Handler** (`src/Services/Submission/Submission.Application/Features/ApproveArticle/ApproveArticleCommandHandler.cs`):
```csharp
public async Task<IdResponse> Handle(ApproveArticleCommand command, CancellationToken ct)
{
    var article = await _articleRepository.FindByIdOrThrowAsync(command.ArticleId);

    if (!await IsEditorAssignedToJournal(article.JournalId, command.CreatedById))
        throw new BadRequestException($"Editor is not assigned to the article's Journal (Id: {article.JournalId})");

    var editor = await GetOrCreatePersonByUserId(command.CreatedById, command, ct);

    article.Approve(editor, command, _stateMachineFactory);

    await _articleRepository.SaveChangesAsync();

    return new IdResponse(article.Id);
}
```

**Domain Event Raised In Aggregate** (`src/Services/Submission/Submission.Domain/Behaviours/Article.cs`):
```csharp
public void Approve(Person editor, IArticleAction<ArticleActionType> action, ArticleStateMachineFactory _stateMachineFactory)
{
    _actors.Add(new ArticleActor { Person = editor, Role = UserRoleType.REVED });
    SetStage(ArticleStage.InitialApproved, action, _stateMachineFactory);
    AddDomainEvent(new ArticleApproved(this, action));
}
```

**Domain Event -> Integration Event Boundary** (`src/Services/Submission/Submission.Application/Features/ApproveArticle/PublishIntegrationEventOnArticleApprovedHandler.cs`):
```csharp
public class PublishIntegrationEventOnArticleApprovedHandler(ArticleRepository _articleRepository, IPublishEndpoint _publishEndpoint)
    : INotificationHandler<ArticleApproved>
{
    public async Task Handle(ArticleApproved notification, CancellationToken ct)
    {
        var article = await _articleRepository.GetFullArticleByIdAsync(notification.Article.Id);
        var articleDto = article.Adapt<ArticleDto>();

        await _publishEndpoint.Publish(new ArticleApprovedForReviewEvent(articleDto), ct);
    }
}
```

### 2.3 CQRS

Commands and queries are separated by base interface. All live in the Application project.

**Command base** (`src/BuildingBlocks/Blocks.MediatR/Abstractions/ICommand.cs`):
```csharp
public interface ICommand : ICommand<Unit> { }
public interface ICommand<out TResponse> : IRequest<TResponse> { }
```

**Query base** (`src/BuildingBlocks/Blocks.MediatR/Abstractions/IQuery.cs`):
```csharp
public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : notnull { }
```

**Article command base** (`src/Services/Submission/Submission.Application/Features/_Shared/ArticleCommand.cs`):
```csharp
public abstract record ArticleCommand : ArticleCommandBase<ArticleActionType>, ICommand<IdResponse>;
public abstract record ArticleCommand<TResponse> : ArticleCommandBase<ArticleActionType>, ICommand<TResponse>;
```

**`ArticleCommandBase<TActionType>`** (`src/BuildingBlocks/Articles.Abstractions/ArticleCommandBase.cs`):
```csharp
public abstract record ArticleCommandBase<TActionType>: IArticleAction<TActionType>
    where TActionType : Enum
{
    [JsonIgnore] public int ArticleId { get; set; }
    public string? Comment { get; init; }
    [JsonIgnore] public abstract TActionType ActionType { get; }
    [JsonIgnore] public string Action => ActionType.ToString();
    [JsonIgnore] public DateTime CreatedOn => DateTime.UtcNow;
    [JsonIgnore] public int CreatedById { get; set; }
}
```

### 2.4 MediatR Pipeline (Submission, Review)

Registered in Application `DependencyInjection.cs`:
```csharp
.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    config.AddOpenBehavior(typeof(AssignUserIdBehavior<,>));
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
})
```

Pipeline order: `AssignUserIdBehavior` -> `ValidationBehavior` -> `LoggingBehavior` -> handler.

**AssignUserIdBehavior** (`src/BuildingBlocks/Blocks.MediatR/Behaviours/AssignUserIdBehavior.cs`):
```csharp
public class AssignUserIdBehavior<TRequest, TResponse>
    (IClaimsProvider _claimsProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuditableAction
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var userId = _claimsProvider.TryGetUserId();
        if (userId is not null)
            request.CreatedById = userId.Value;

        return await next();
    }
}
```

**ValidationBehavior** (`src/BuildingBlocks/Blocks.MediatR/Behaviours/ValidationBehavior.cs`):
```csharp
public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
{
    var context = new ValidationContext<TRequest>(request);
    var validationResults =
        await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

    var failures = validationResults
        .Where(r => r.Errors.Any())
        .SelectMany(r => r.Errors)
        .ToList();

    if (failures.Any())
        throw new ValidationException(failures);

    return await next();
}
```

### 2.5 DDD Aggregate Pattern

Aggregates use partial classes to separate state from behavior. The strongest DDD shape is in Submission, Review, and Production.

**AggregateRoot base** (`src/BuildingBlocks/Blocks.Domain/Entities/AggregateRoot.cs`):
```csharp
public interface IAggregateRoot<TPrimaryKey> : IAuditedEntity<TPrimaryKey>
    where TPrimaryKey : struct
{
    public IReadOnlyList<IDomainEvent> DomainEvents { get; }
    public void AddDomainEvent(IDomainEvent eventItem);
    public void ClearDomainEvents();
}

public abstract class AggregateRoot<TPrimaryKey> : Entity<TPrimaryKey>, IAggregateRoot<TPrimaryKey>
    where TPrimaryKey : struct
{
    public TPrimaryKey CreatedById { get; init; }
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
    public TPrimaryKey? LastModifiedById { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    private List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    public void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

**State** (`src/Services/Submission/Submission.Domain/Entities/Article.cs`):
```csharp
public partial class Article : AggregateRoot
{
    internal Article() {}
    public required string Title { get; init; }
    public ArticleType Type { get; init; }
    public ArticleStage Stage { get; set; }
    public int JournalId { get; init; }
    public required Journal Journal { get; init; } = null!;

    private readonly List<Asset> _assets = new();
    public IReadOnlyList<Asset> Assets => _assets.AsReadOnly();
    private readonly List<ArticleActor> _actors = new();
    public IReadOnlyList<ArticleActor> Actors => _actors.AsReadOnly();
    private readonly List<StageHistory> _stageHistories = new();
    public IReadOnlyList<StageHistory> StageHistories => _stageHistories.AsReadOnly();
    private readonly List<ArticleAction> _actions = new();
    public IReadOnlyList<ArticleAction> Actions => _actions.AsReadOnly();
}
```

**Behavior** (`src/Services/Submission/Submission.Domain/Behaviours/Article.cs`):
```csharp
public partial class Article
{
    public void SetStage(ArticleStage newStage, IArticleAction<ArticleActionType> action, ArticleStateMachineFactory stateMachineFactory)
    {
        stateMachineFactory.ValidateStageTransition(Stage, action.ActionType);
        if (newStage == Stage) return;

        var currentStage = Stage;
        Stage = newStage;
        LastModifiedOn = action.CreatedOn;
        LastModifiedById = action.CreatedById;
        _stageHistories.Add(new StageHistory { ArticleId = Id, StageId = newStage, StartDate = DateTime.UtcNow });
        AddDomainEvent(new ArticleStageChanged(currentStage, newStage, action));
    }

    private void AddAction(IArticleAction<ArticleActionType> action)
    {
        _actions.Add(action.Adapt<ArticleAction>());
        AddDomainEvent(new ArticleActionExecuted(this, action));
    }
}
```

**Domain event definition** (`src/Services/Submission/Submission.Domain/Events/ArticleCreated.cs`):
```csharp
public record ArticleCreated(Article Article, IArticleAction action) : DomainEvent(action);
```

Where: `public record DomainEvent(IArticleAction Action) : DomainEvent<IArticleAction>(Action);`
And the global base: `public abstract record DomainEvent<TAction>(TAction Action) : IDomainEvent where TAction : IArticleAction;`

**DDD deviations:**

- Journals does not use `AggregateRoot`; it uses Redis entities and endpoint-published domain events.
- ArticleHub is a read model domain, not a behavior-rich aggregate domain.
- Auth has entities and value objects, but the `UserCreated` domain event is defined and not currently raised.

### 2.6 Repository Pattern

**Generic base** (`src/BuildingBlocks/Blocks.EntityFrameworkCore/Repositories/Repository.cs`):
```csharp
public abstract class RepositoryBase<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
    where TContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    protected readonly TContext _dbContext;
    protected readonly DbSet<TEntity> _entity;

    public TContext Context => _dbContext;
    public virtual IQueryable<TEntity> Query() => _entity;
    public virtual IQueryable<TEntity> QueryNotTracked() => _entity.AsNoTracking();
    public async Task<TEntity?> FindByIdAsync(TKey id) => await _entity.FindAsync(id);
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
        => await Query().SingleOrDefaultAsync(e => e.Id.Equals(id), ct);
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
        => (await _entity.AddAsync(entity, ct)).Entity;
    public virtual Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _dbContext.SaveChangesAsync(ct);
}
```

**Service-level concrete** (`src/Services/Submission/Submission.Persistence/Repositories/Repository.cs`):
```csharp
public class Repository<TEntity>(SubmissionDbContext dbContext)
    : RepositoryBase<SubmissionDbContext, TEntity>(dbContext)
        where TEntity : class, IEntity<int>
{
}
```

**Domain-specific** (`src/Services/Submission/Submission.Persistence/Repositories/ArticleRepository.cs`):
```csharp
public class ArticleRepository(SubmissionDbContext dbContext)
    : Repository<Article>(dbContext)
{
    public override IQueryable<Article> Query()
    {
        return base.Entity
            .Include(e => e.Actors).ThenInclude(e => e.Person)
            .Include(e => e.Assets);
    }

    public async Task<Article?> GetFullArticleByIdAsync(int id, CancellationToken ct = default)
    {
        return await Query()
            .Include(e => e.Journal)
            .Include(e => e.SubmittedBy)
            .SingleOrDefaultAsync(e => e.Id == id, ct);
    }
}
```

**Cached repository** (`src/BuildingBlocks/Blocks.EntityFrameworkCore/Repositories/CachedRepository.cs`):
```csharp
public abstract class CachedRepository<TDbContext, TEntity, TId>(TDbContext _dbContext, IMemoryCache _cache)
    where TDbContext : DbContext
    where TEntity : class, IEntity<TId>, ICacheable
    where TId : struct
{
    public IEnumerable<TEntity> GetAll()
        => _cache.GetOrCreateByType(entry => _dbContext.Set<TEntity>().AsNoTracking().ToList());

    public TEntity GetById(TId id)
        => GetAll().Single(e => e.Id.Equals(id));
}
```

**Redis repository** (`src/BuildingBlocks/Blocks.Redis/Repository.cs`):
```csharp
public class Repository<T> where T : Entity
{
    private readonly IRedisCollection<T> _collection;
    private readonly IDatabase _redisDb;

    public IRedisCollection<T> Collection => _collection;

    public async Task AddAsync(T entity)
    {
        if (entity.Id == 0)
            entity.Id = await GenerateNewId(); // Redis INCR counter
        await _collection.InsertAsync(entity);
    }

    public async Task ReplaceAsync(T entity)
    {
        // Workaround: Redis.OM doesn't properly update nested collections
        await _collection.DeleteAsync(entity);
        await _collection.InsertAsync(entity);
    }

    public async Task<int> GenerateNewId()
        => (int) await _redisDb.StringIncrementAsync($"{typeof(T).Name}:Id:Sequence");
}
```

**Registration** in `DependencyInjection.cs`:
```csharp
services.AddScoped(typeof(Repository<>));        // generic usage
services.AddDerivedTypesOf(typeof(Repository<>)); // scans and registers all subclasses
services.AddScoped<AssetTypeRepository>();        // cached repositories registered explicitly
```

---

## 3. Domain Models per Service

### 3.1 Auth Domain

**Aggregates:** `User` (extends `IdentityUser<int>`, manually implements `IAggregateRoot`), `Person` (extends `AggregateRoot`)

**Entities:** `UserRole`, `RefreshToken`, `Role`

**Value Objects:** `EmailAddress` (StringValueObject), `HonorificTitle`, `ProfessionalProfile`

**Domain Events:** `UserCreated(User User, string RessetPasswordToken)` — defined but not raised in active code path

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
- `ArticleStageChanged` — `Behaviours/Article.cs` `SetStage`
- `AuthorAssigned` — `Behaviours/Article.cs` `AssignAuthor`
- `ArticleApproved` — `Behaviours/Article.cs` `Approve`
- `ArticleRejected` — `Behaviours/Article.cs` `Reject`
- `ArticleActionExecuted` — `Behaviours/Article.cs` `AddAction`
- `ArticleCreated` — `Behaviours/Journal.cs` when article is created for a journal
- `AuthorCreated` — `Behaviours/Author.cs`

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
- `FileUploaded` — `Assets/Behavior/Asset.cs` `UploadFile`
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
- `ArticleAcceptedForProduction` — `Articles/Behavior/Article.cs` when materializing from review
- `ArticleStageChanged` — `Articles/Behavior/Article.cs` `SetStage`
- `TypesetterAssigned` — `Articles/Behavior/Article.cs` `AssignTypesetter`
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

---

## 4. Endpoint Patterns

Three endpoint frameworks are used across services. The framework is a per-service decision.

### 4.1 FastEndpoints (Auth, Journals, Production)

**Pattern:** Class extending `Endpoint<TRequest, TResponse>` with attribute-based routing. Validator is a separate class extending `Validator<T>` or `AbstractValidator<T>`. Handler logic in `HandleAsync()`. Domain events published via `PublishAsync()`.

**Complete example** — `CreateUserEndpoint` in Auth:
```csharp
// src/Services/Auth/Auth.API/Features/Users/CreateAccount/CreateUserEndpoint.cs
[Authorize(Roles = Articles.Security.Role.UserAdmin)]
[HttpPost("users")]
[Tags("Users")]
public class CreateUserEndpoint(UserManager<User> _userManager, PersonRepository _personRepository, AuthDbContext _dbContext)
    : Endpoint<CreateUserCommand, CreateUserResponse>
{
    public override async Task HandleAsync(CreateUserCommand command, CancellationToken ct)
    {
        var person = await _personRepository.GetByEmailAsync(command.Email, ct);
        if (person?.User != null)
            throw new BadRequestException($"User with email {command.Email} already exists");

        if (person is null)
            person = await CreatePersonAsync(command, ct);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        var user = Domain.Users.User.Create(command, person);
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            var errorMessages = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new BadRequestException($"Unable to create user: {errorMessages}");
        }

        person.AssignUser(user);
        await _personRepository.SaveChangesAsync(ct);

        var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        await PublishAsync(new UserCreated(user, resetPasswordToken));
        await transaction.CommitAsync(ct);

        await Send.OkAsync(new CreateUserResponse(command.Email, user.Id, resetPasswordToken));
    }
}
```

**Journals — handler logic inside endpoint** (no separate handler file):
```csharp
// src/Services/Journals/Journals.API/Features/Journals/Create/CreateJournalEndpoint.cs
[Authorize(Roles = Role.EditorAdmin)]
[HttpPost("journals")]
[Tags("Journals")]
public class CreateJournalEndpoint(Repository<Journal> _journalRepository, Repository<Editor> _editorRepository, IPersonService _personClient)
    : Endpoint<CreateJournalCommand, IdResponse>
{
    public override async Task HandleAsync(CreateJournalCommand command, CancellationToken ct)
    {
        if (_journalRepository.Collection.Any(j => j.Abbreviation == command.Abbreviation || j.NormalizedName == command.NormalizedName))
            throw new BadRequestException("Journal with the same name or abbreviation already exists");

        var journal = command.Adapt<Journal>();
        await _journalRepository.AddAsync(journal);
        await PublishAsync(new JournalCreated(journal));
        await Send.OkAsync(new IdResponse(journal.Id));
    }
}
```

**Production — BaseEndpoint with global preprocessor:**
```csharp
// src/Services/Production/Production.API/Features/Articles/AssignTypesetter/AssignTypesetterEndpoint.cs
[Authorize(Roles = Role.ProdAdmin)]
[HttpPut("articles/{articleId:int}/typesetter/{typesetterId:int}")]
[Tags("Articles")]
public class AssignTypesetterEndpoint(ArticleRepository articleRepository, ArticleStateMachineFactory _stateMachineFactory, ProductionDbContext _dbContext)
    : BaseEndpoint<AssignTypesetterCommand, IdResponse>(articleRepository)
{
    public override async Task HandleAsync(AssignTypesetterCommand command, CancellationToken ct)
    {
        _article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId);
        var typesetter = await _dbContext.Typesetters.FindByIdOrThrowAsync(command.TypesetterId);
        _article.AssignTypesetter(typesetter, _stateMachineFactory, command);
        await _articleRepository.SaveChangesAsync();
        await Send.OkAsync(new IdResponse(command.ArticleId));
    }
}
```

**FastEndpoints setup** (`src/BuildingBlocks/Blocks.FastEndpoints/Extensions.cs`):
```csharp
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
    c.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
    {
        var errorDict = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
        // ...
    };
});
```

Production adds `AssignUserIdPreProcessor` globally:
```csharp
// src/Services/Production/Production.API/Program.cs
app.UseFastEndpoints(config =>
{
    config.Endpoints.Configurator = ep =>
    {
        ep.PreProcessor<AssignUserIdPreProcessor>(FastEndpoints.Order.Before);
    };
});
```

### 4.2 Carter (Review, ArticleHub)

**Pattern:** Class implementing `ICarterModule` with `AddRoutes(IEndpointRouteBuilder)`. Route registered inside `AddRoutes`. Authorization via `.RequireRoleAuthorization(...)`.

**Complete example** — Review:
```csharp
// src/Services/Review/Review.API/Endpoints/Articles/AcceptArticleEndpoint.cs
public class AcceptArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{articleId:int}:accept", async (int articleId, AcceptArticleCommand command, ISender sender) =>
        {
            command.ArticleId = articleId;
            var response = await sender.Send(command);
            return Results.Ok(response);
        })
        .RequireRoleAuthorization(Role.Editor, Role.EditorAdmin)
        .WithName("AcceptArticle")
        .WithTags("Articles")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
```

**ArticleHub — Hasura GraphQL endpoint:**
```csharp
// src/Services/ArticleHub/ArticleHub.API/Articles/SearchArticles/SearchArticlesEndpoint.cs
public class SearchArticlesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/graphql", async (SearchArticlesQuery articlesQuery, ArticleGraphQLReadStore graphQLReadStore, CancellationToken ct) =>
        {
            var response = await graphQLReadStore.GetArticlesAsync(
                articlesQuery.Filter, articlesQuery.Pagination.Limit, articlesQuery.Pagination.Offset, ct);
            return Results.Json(response?.Items);
        })
        .RequireAuthorization();
    }
}
```

Carter setup: `var api = app.MapGroup("/api"); api.MapCarter();`

### 4.3 Minimal APIs (Submission)

**Pattern:** Static class with `Map(this IEndpointRouteBuilder app)` extension method. All endpoints wired via `EndpointRegistration.MapAllEndpoints()`.

**File upload pattern** (`src/Services/Submission/Submission.API/Endpoints/UploadManuscriptFileEndpoint.cs`):
```csharp
app.MapPost("/articles/{articleId:int}/assets/manuscript:upload",
    async ([FromRoute] int articleId, [FromForm] UploadManuscriptFileCommand command, ISender sender) =>
    {
        var response = await sender.Send(command with { ArticleId = articleId });
        return Results.Created($"/api/articles/{articleId}/assets/{response.Id}:download", response);
    })
.RequireRoleAuthorization(Role.Author)
.DisableAntiforgery(); // required for IFormFile
```

**Registration** (`src/Services/Submission/Submission.API/Endpoints/XEndpointRegistration.cs`):
```csharp
public static class EndpointRegistration
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");
        GetArticleEndpoint.Map(api);
        CreateArticleEndpoint.Map(api);
        // ...
        return app;
    }
}
```

### 4.4 Authorization Patterns

**Role-based via `.RequireRoleAuthorization()`** (`src/BuildingBlocks/Articles.Security/Extensions.cs`):
```csharp
public static TBuilder RequireRoleAuthorization<TBuilder>(this TBuilder builder, params string[] roles)
    where TBuilder : IEndpointConventionBuilder
    => builder.RequireAuthorization(policy =>
    {
        policy.RequireRole(roles);
        policy.Requirements.Add(new ArticleRoleRequirement(roles));
    });
```

**Resource-based via `ArticleAccessAuthorizationHandler`** (`src/BuildingBlocks/Articles.Security/ArticleAccessAuthorizationHandler.cs`):
```csharp
public class ArticleAccessAuthorizationHandler(HttpContextProvider _httpProvider, IArticleAccessChecker _articleRoleChecker)
    : AuthorizationHandler<ArticleRoleRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ArticleRoleRequirement requirement)
    {
        var userRoles = _httpProvider.GetUserRoles<UserRoleType>()
                            .Where(requirement.AllowedRoles.Contains)
                            .ToHashSet();

        if (userRoles.Count > 0 && await HasUserRoleForArticle(userRoles))
            context.Succeed(requirement);
    }
}
```

Service-specific access checkers: `Submission.Application/ArticleAccessChecker.cs`, `Review.Application/ArticleAccessChecker.cs`, `Production.Application/ArticleAccessChecker.cs`.

### 4.5 User ID Assignment Mechanisms

Three mechanisms exist depending on the endpoint framework:

**MediatR Pipeline** (`AssignUserIdBehavior`) — Submission, Review

**Minimal API Filter** (`src/BuildingBlocks/Blocks.AspNetCore/Filters/AssignUserIdFilter.cs`):
```csharp
if (arg is IAuditableAction action && action.CreatedById == default)
{
    var userId = _claimsProvider.TryGetUserId();
    if (userId is not null)
        action.CreatedById = userId.Value;
}
```

**FastEndpoints PreProcessor** (`src/BuildingBlocks/Blocks.FastEndpoints/AssignUserIdPreProcessor.cs`):
```csharp
if (context.Request is IAuditableAction articleCommand)
{
    var claimsProvider = context.HttpContext.Resolve<IClaimsProvider>();
    articleCommand.CreatedById = claimsProvider.GetUserId();
}
```

Production registers both `AssignUserIdPreProcessor` (FastEndpoints) AND `AssignUserIdFilter` (Minimal APIs group).

---

## 5. Persistence Patterns

### 5.1 ApplicationDbContext

`ApplicationDbContext<TDbContext>` is the base for all SQL Server / PostgreSQL services:

```csharp
// src/BuildingBlocks/Blocks.EntityFrameworkCore/ApplicationDbContext.cs
public class ApplicationDbContext<TDbContext>(DbContextOptions<TDbContext> _options, IMemoryCache _cache)
    : DbContext(_options)
    where TDbContext : DbContext
{
    public virtual IEnumerable<TEntity> GetAllCached<TEntity>()
        where TEntity : class, ICacheable
        => _cache.GetOrCreateByType(entry => this.Set<TEntity>().AsNoTracking().ToList());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        modelBuilder.UseEntityTypeNamesAsTables();
    }

    public async override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        this.UnTrackCacheableEntities();
        return await base.SaveChangesAsync(ct);
    }
}
```

**Database choices:**
- Auth, Submission, Review, Production → SQL Server
- ArticleHub → PostgreSQL (Npgsql)
- Journals → Redis (Redis.OM)
- ArticleTimeline module → SQL Server

### 5.2 Entity Type Configurations

Base classes in `Blocks.EntityFrameworkCore/EntityConfigurations/`:

```csharp
// EntityConfiguration<T> — base for all
public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IEntity<int>
{
    protected virtual bool HasGeneratedId => true;
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id);
        if (HasGeneratedId)
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.SeedFromJsonFile(); // seeds from a JSON file by convention
    }
}

// AuditedEntityConfiguration<T> — for aggregates with audit fields
public abstract class AuditedEntityConfiguration<TEntity> : EntityConfiguration<TEntity>
    where TEntity : class, IAuditedEntity<int>
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);
        builder.Property(e => e.CreatedOn).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(e => e.CreatedById).IsRequired();
        builder.Property(e => e.LastModifiedOn);
        builder.Property(e => e.LastModifiedById);
    }
}
```

**Table naming strategy** (`src/BuildingBlocks/Blocks.EntityFrameworkCore/Extensions/ModelBuilderExtensions.cs`):
```csharp
public static void UseEntityTypeNamesAsTables(this ModelBuilder modelBuilder, INameRewriter? nameRewriter = null)
{
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(IValueObject).IsAssignableFrom(entity.ClrType))
            continue;

        var rootType = entity;
        while (rootType.BaseType != null)
            rootType = rootType.BaseType;

        var tableName = rootType.ClrType.Name;
        modelBuilder.Entity(entity.ClrType).ToTable(tableName);
    }
}
```

### 5.3 Concrete EF Configuration Examples

**Auth — owned value objects** (`src/Services/Auth/Auth.Persistence/EntityConfigurations/PersonEntityConfiguration.cs`):
```csharp
builder.OwnsOne(
     e => e.Email, b =>
     {
         b.Property(n => n.Value).HasColumnName(nameof(Person.Email)).HasMaxLength(MaxLength.C64);
         b.Property(e => e.NormalizedEmail).HasColumnName(nameof(EmailAddress.NormalizedEmail)).HasMaxLength(MaxLength.C64);
         b.HasIndex(e => e.NormalizedEmail).IsUnique();
     });

builder.OwnsOne(e => e.ProfessionalProfile, b =>
{
    b.Property(e => e.Position).HasMaxLength(MaxLength.C64).HasColumnName(nameof(ProfessionalProfile.Position));
    b.Property(e => e.CompanyName).HasMaxLength(MaxLength.C128).HasColumnNameSameAsProperty();
    b.Property(e => e.Affiliation).HasMaxLength(MaxLength.C256).HasColumnNameSameAsProperty();
});
```

**Review — ComplexProperty** (`src/Services/Review/Review.Persistence/EntityConfigurations/ReviewInvitationEntityConfiguration.cs`):
```csharp
builder.Property(e => e.Status).IsRequired().HasEnumToStringConversion(MaxLength.C8);

builder.ComplexProperty(
     o => o.Token, builder =>
     {
         builder.Property(n => n.Value)
             .HasColumnName(builder.Metadata.PropertyInfo!.Name)
             .HasMaxLength(MaxLength.C64);
     });
```

**ArticleHub — PostgreSQL timestamp** (`src/Services/ArticleHub/ArticleHub.Persistence/EntityConfigurations/ArticleEntityConfiguration.cs`):
```csharp
builder.Property(e => e.SubmittedOn).HasColumnType("timestamp without time zone").IsRequired();
builder.Property(e => e.AcceptedOn).HasColumnType("timestamp without time zone");
builder.Property(e => e.PublishedOn).HasColumnType("timestamp without time zone");
```

### 5.4 Domain Event Dispatch via SaveChangesInterceptor

**Standard interceptor** (Submission, Review):
```csharp
// src/BuildingBlocks/Blocks.EntityFrameworkCore/Interceptors/DispatchDomainEventsInterceptor.cs
public class DispatchDomainEventsInterceptor(IDomainEventPublisher _publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        result = await base.SavedChangesAsync(eventData, result, ct);
        if (eventData.Context is not null)
            await eventData.Context.DispatchDomainEventsAsync(_publisher, ct);
        return result;
    }
}
```

**Transactional interceptor** (Production — wraps save + dispatch in a single transaction):
```csharp
// src/BuildingBlocks/Blocks.EntityFrameworkCore/Interceptors/TransactionalDispatchDomainEventsInterceptor.cs
public class TransactionalDispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (_transactionOptions.UseSingleTransaction
            && eventData.Context is not null
            && eventData.Context.Database.CurrentTransaction is null
            && _transaction is null)
        {
            _transaction = await _transactionProvider.BeginTransactionAsync(ct);
            await eventData.Context.Database.UseTransactionAsync(_transaction, ct);
        }
        return await base.SavingChangesAsync(eventData, result, ct);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        result = await base.SavedChangesAsync(eventData, result, ct);
        if (eventData.Context is not null)
            await eventData.Context.DispatchDomainEventsAsync(_eventPublisher, ct);
        if (_transaction != null)
            await _transaction.CommitAsync(ct);
        return result;
    }
}
```

**Domain event dispatch mechanism** (`DbContextExtensions.DomainEvents.cs`):
```csharp
public static async Task<int> DispatchDomainEventsAsync(this DbContext ctx, IDomainEventPublisher eventPublisher, CancellationToken ct = default)
{
    var aggregates = ctx.ChangeTracker.Entries()
        .Select(a => a.Entity)
        .OfType<IAggregateRoot>()
        .Where(a => a.DomainEvents.Any())
        .ToList();

    var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();
    aggregates.ForEach(a => a.ClearDomainEvents());

    foreach (var domainEvent in domainEvents)
        await eventPublisher.PublishAsync(domainEvent, ct);

    return domainEvents.Count;
}
```

**Where interceptors are used:**
- Submission, Review → `DispatchDomainEventsInterceptor`
- Production → `TransactionalDispatchDomainEventsInterceptor`
- Auth → EF/Identity, no aggregate event-dispatch flow
- ArticleHub → EF write model, no event-dispatch pipeline
- Journals → Redis.OM, not EF Core

### 5.5 Migration Strategy

```bash
# Add migration
dotnet ef migrations add Name -p Services/{Svc}/{Svc}.Persistence -s Services/{Svc}/{Svc}.API

# Apply migration
dotnet ef database update -p Services/{Svc}/{Svc}.Persistence -s Services/{Svc}/{Svc}.API
```

Migrations are applied on startup:
```csharp
app.Migrate<SubmissionDbContext>();
app.Migrate<ArticleTimelineDbContext>(); // if module is included
```

### 5.6 Redis Repository (Journals)

```csharp
// src/BuildingBlocks/Blocks.Redis/Repository.cs — see section 2.6 for full code

// Registration
services.AddSingleton(new RedisConnectionProvider(connectionString!));
services.AddSingleton<IConnectionMultiplexer>(redis);
services.AddScoped(typeof(Repository<>));

// Index creation at startup
var provider = scope.ServiceProvider.GetRequiredService<RedisConnectionProvider>();
provider.Connection.CreateIndex(typeof(Editor));
provider.Connection.CreateIndex(typeof(Journal));
```

---

## 6. Cross-Service Communication (gRPC)

### 6.1 Code-First Contract Definition

gRPC contracts use `protobuf-net` code-first approach: C# interface with `[ServiceContract]`, DTOs with `[ProtoContract]`/`[ProtoMember]`.

**Person service contract** (`src/BuildingBlocks/Articles.Grpc.Contracts/Auth/PersonContracts.cs`):
```csharp
[ServiceContract]
public interface IPersonService
{
    [OperationContract]
    ValueTask<GetPersonResponse> GetPersonByIdAsync(GetPersonRequest request, CallContext context = default);
    [OperationContract]
    ValueTask<GetPersonResponse> GetPersonByUserIdAsync(GetPersonByUserIdRequest request, CallContext context = default);
    [OperationContract]
    ValueTask<GetPersonResponse> GetPersonByEmailAsync(GetPersonByEmailRequest request, CallContext context = default);
    [OperationContract]
    ValueTask<CreatePersonResponse> GetOrCreatePersonAsync(CreatePersonRequest request, CallContext context = default);
}

[ProtoContract]
public class PersonInfo
{
    [ProtoMember(1)] public int Id { get; set; }
    [ProtoMember(2)] public string FirstName { get; set; } = default!;
    [ProtoMember(3)] public string LastName { get; set; } = default!;
    [ProtoMember(4)] public string Email { get; set; } = default!;
    [ProtoMember(5)] public Gender Gender { get; set; }
    [ProtoMember(6, IsRequired = false)] public string? Honorific { get; set; }
    // ...
}
```

The repo also has `.proto` files alongside code-first interfaces in `Articles.Grpc.Contracts`.

### 6.2 gRPC Server Implementations

**PersonGrpcService** (`src/Services/Auth/Auth.API/Features/Persons/PersonGrpcService.cs`):
```csharp
public class PersonGrpcService(PersonRepository _personRepository, GrpcTypeAdapterConfig _typeAdapterConfig) : IPersonService
{
    public async ValueTask<GetPersonResponse> GetPersonByIdAsync(GetPersonRequest request, CallContext context = default)
        => await GetPersonResponseAsync(() => _personRepository.GetByIdAsync(request.PersonId));

    public async ValueTask<CreatePersonResponse> GetOrCreatePersonAsync(CreatePersonRequest request, CallContext context = default)
    {
        var person = await _personRepository.GetByEmailAsync(request.Email);
        if (person is null)
        {
            person = Person.Create(request);
            await _personRepository.AddAsync(person, context.CancellationToken);
            await _personRepository.SaveChangesAsync(context.CancellationToken);
        }
        return new CreatePersonResponse { PersonInfo = person.Adapt<PersonInfo>(_typeAdapterConfig) };
    }

    private async ValueTask<GetPersonResponse> GetPersonResponseAsync(Func<Task<Person?>> fetch)
    {
        var person = Guard.NotFound(await fetch());
        return new GetPersonResponse { PersonInfo = person.Adapt<PersonInfo>(_typeAdapterConfig) };
    }
}
```

**JournalGrpcService** (`src/Services/Journals/Journals.API/Features/Journals/JournalGrpcService.cs`):
```csharp
public class JournalGrpcService(Repository<Journal> _journalRepository) : IJournalService
{
    public async ValueTask<GetJournalResponse> GetJournalByIdAsync(GetJournalByIdRequest request, CallContext context = default)
    {
        var journal = await _journalRepository.GetByIdOrThrowAsync(request.JournalId);
        return new GetJournalResponse { Journal = journal.Adapt<JournalInfo>() };
    }

    public async ValueTask<IsEditorAssignedToJournalResponse> IsEditorAssignedToJournalAsync(IsEditorAssignedToJournalRequest request, CallContext context = default)
    {
        var journal = await _journalRepository.GetByIdOrThrowAsync(request.JournalId);
        return new IsEditorAssignedToJournalResponse { IsAssigned = journal.ChiefEditorId == request.UserId };
    }
}
```

### 6.3 gRPC Client Registration

**`AddCodeFirstGrpcClient<T>`** (`src/BuildingBlocks/Blocks.AspNetCore/Grpc/GrpcClientRegistrationExtensions.cs`):
```csharp
public static IServiceCollection AddCodeFirstGrpcClient<TClient>(
    this IServiceCollection services, GrpcServicesOptions grpcOptions, string? serviceKey = null)
    where TClient : class
{
    serviceKey ??= typeof(TClient).Name.Replace("Client", "").Replace("Service", "");

    if (!grpcOptions.Services.TryGetValue(serviceKey, out var serviceSettings))
        throw new InvalidOperationException($"Missing GrpcService config for: {typeof(TClient).Name}");

    services.AddScoped(sp =>
    {
        var channel = GrpcChannel.ForAddress(serviceSettings.Url, new GrpcChannelOptions
        {
            HttpHandler = new HttpClientHandler
            {
#if DEBUG
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
#endif
            }
        });
        return channel.CreateGrpcService<TClient>();
    });

    return services;
}
```

**Registration example** (Submission):
```csharp
var grpcOptions = config.GetSectionByTypeName<GrpcServicesOptions>();
services.AddCodeFirstGrpcClient<IPersonService>(grpcOptions, "Person");
services.AddCodeFirstGrpcClient<IJournalService>(grpcOptions, "Journal");
```

### 6.4 gRPC Client Usage in Handlers

```csharp
// CreateArticleCommandHandler — fetching journal via gRPC
private async Task<Journal> CreateJournal(CreateArticleCommand command)
{
    var response = await _journalClient.GetJournalByIdAsync(new GetJournalByIdRequest { JournalId = command.JournalId });
    var journal = Journal.Create(response.Journal, command);
    await _journalRepository.AddAsync(journal);
    return journal;
}

// ApproveArticleCommandHandler — checking editor assignment
private async Task<bool> IsEditorAssignedToJournal(int journalId, int userId)
{
    var response = await _journalClient.IsEditorAssignedToJournalAsync(new IsEditorAssignedToJournalRequest
    {
        JournalId = journalId,
        UserId = userId
    });
    return response.IsAssigned;
}
```

### 6.5 ClientServices/ Observation

The current repo does not use a live `ClientServices/` folder on disk. The active client pattern is:

- Shared gRPC contracts in `Articles.Grpc.Contracts`
- `AddCodeFirstGrpcClient<TClient>()` registration
- Consuming interfaces directly in handlers/endpoints

The solution files still reference a missing legacy client project (`src/Submission.sln`, `src/Articles.sln`).

---

## 7. Messaging and Events (MassTransit)

### 7.1 Integration Event Contracts

All integration events are C# records in `Articles.Integration.Contracts`:

```csharp
public record ArticleApprovedForReviewEvent(ArticleDto Article);
public record ArticleAcceptedForProductionEvent(ArticleDto Article);
public record JournalCreatedEvent(JournalDto Journal);

public record ArticleDto(
    int Id, string Title, string Scope, string? Doi,
    ArticleType Type, ArticleStage Stage,
    JournalDto Journal, PersonDto SubmittedBy,
    DateTime SubmittedOn, DateTime? AcceptedOn, DateTime? PublishedOn,
    List<ActorDto> Actors, List<AssetDto> Assets
);
```

### 7.2 MassTransit Setup

```csharp
// src/BuildingBlocks/Blocks.Messaging/MassTransit/DependencyInjection.cs
public static IServiceCollection AddMassTransitWithRabbitMQ(
    this IServiceCollection services, IConfiguration configuration, Assembly assembly)
{
    var rabbitMqOptions = configuration.GetSectionByTypeName<RabbitMqOptions>();
    var serviceName = assembly.GetServiceName();

    services.AddMassTransit(config =>
    {
        config.SetEndpointNameFormatter(new SnakeCaseWithServiceSuffixNameFormatter(serviceName));
        config.AddConsumers(assembly);
        config.UsingRabbitMq((context, rabbitConfig) =>
        {
            rabbitConfig.Host(new Uri(rabbitMqOptions.Host), rabbitMqOptions.VirtualHost, h =>
            {
                h.Username(rabbitMqOptions.UserName);
                h.Password(rabbitMqOptions.Password);
            });
            rabbitConfig.ConfigureEndpoints(context);
        });
    });

    return services;
}
```

### 7.3 Publishing Integration Events (from Domain Event Handlers)

Integration events are published by domain event handlers. Two styles exist:

**MediatR style** (Submission, Review):
```csharp
// src/Services/Review/Review.Application/Features/Articles/AcceptArticle/PublishIntegrationEventOnArticleAcceptedHandler.cs
public class PublishIntegrationEventOnArticleAcceptedHandler(ArticleRepository _articleRepository, IPublishEndpoint _publishEndpoint)
    : INotificationHandler<ArticleAccepted>
{
    public async Task Handle(ArticleAccepted notification, CancellationToken ct)
    {
        var article = await _articleRepository.GetFullArticleByIdAsync(notification.Article.Id);
        var articleDto = article.Adapt<ArticleDto>();
        await _publishEndpoint.Publish(new ArticleAcceptedForProductionEvent(articleDto), ct);
    }
}
```

**FastEndpoints style** (Journals):
```csharp
// src/Services/Journals/Journals.API/Features/Journals/Create/PublishIntegrationEventOnJournalCreatedHandler.cs
public class PublishIntegrationEventOnJournalCreatedHandler(Repository<Journal> _journalRepository, IPublishEndpoint _publishEndpoint)
    : IEventHandler<JournalCreated>
{
    public async Task HandleAsync(JournalCreated notification, CancellationToken ct)
    {
        var journal = await _journalRepository.GetByIdAsync(notification.Journal.Id);
        var journalDto = journal.Adapt<JournalDto>();
        await _publishEndpoint.Publish(new JournalCreatedEvent(journalDto), ct);
    }
}
```

### 7.4 Consumer Implementations

**Review consumer initializes from Submission** (`src/Services/Review/Review.Application/Features/Articles/InitializeFromSubmission/ArticleApprovedForReviewConsumer.cs`):
```csharp
public class ArticleApprovedForReviewConsumer(...) : IConsumer<ArticleApprovedForReviewEvent>
{
    public async Task Consume(ConsumeContext<ArticleApprovedForReviewEvent> context)
    {
        var articleDto = context.Message.Article;

        await _articleRepository.EnsureNotExistsOrThrowAsync(articleDto.Id, context.CancellationToken);

        var actors = await CreateActors(articleDto);
        var assets = await CreateAssets(articleDto, context.CancellationToken);
        var journal = await GetOrCreateJournal(articleDto);

        var article = Article.FromSubmission(articleDto, actors, assets, _stateMachineFactory, action);
        await _articleRepository.AddAsync(article);
    }
}
```

**Production consumer initializes from Review** (`src/Services/Production/Production.API/Features/Articles/InitializeFromReview/ArticleAcceptedForProductionConsumer.cs`):
```csharp
public sealed class ArticleAcceptedForProductionConsumer(...) : IConsumer<ArticleAcceptedForProductionEvent>
{
    public async Task Consume(ConsumeContext<ArticleAcceptedForProductionEvent> context)
    {
        var articleDto = context.Message.Article;
        if (await articleRepository.ExistsAsync(articleDto.Id))
            return;

        var journal = await GetOrCreateJournal(articleDto);
        var actors = await CreateActors(articleDto, ct);
        var assets = await CreateAssets(articleDto, ct);

        var article = Article.FromReview(articleDto, actors, assets, action);
        await articleRepository.AddAsync(article);
        await dbContext.SaveChangesAsync(ct);
    }
}
```

**ArticleHub consumer projects into read model** (`src/Services/ArticleHub/ArticleHub.API/Articles/Consumers/ArticleApprovedForReviewConsumer.cs`):
```csharp
public class ArticleApprovedForReviewConsumer(ArticleHubDbContext _dbContext)
    : IConsumer<ArticleApprovedForReviewEvent>
{
    public async Task Consume(ConsumeContext<ArticleApprovedForReviewEvent> context)
    {
        var articleDto = context.Message.Article;

        if (await _dbContext.Articles.AnyAsync(a => a.Id == articleDto.Id, context.CancellationToken))
            throw new BadRequestException("Article was already approved for review.");

        var journal = await GetOrCreateJournalAsync(articleDto, context.CancellationToken);
        var article = articleDto.AdaptWith<Article>(article =>
        {
            article.Journal = journal;
            article.SubmittedById = articleDto.SubmittedBy.Id;
        });

        await CreateActorsAsync(articleDto, article, context.CancellationToken);
        await _dbContext.Articles.AddAsync(article);
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
```

**Journal sync consumer** in Submission (idempotent):
```csharp
public class JournalCreatedConsumer(Repository<Journal> _journalRepository) : IConsumer<JournalCreatedEvent>
{
    public async Task Consume(ConsumeContext<JournalCreatedEvent> context)
    {
        var existing = await _journalRepository.GetByIdAsync(context.Message.Journal.Id);
        if (existing is not null) return; // idempotency

        var journal = new Journal { Id = journalDto.Id, Name = journalDto.Name, Abbreviation = journalDto.Abbreviation };
        await _journalRepository.AddAsync(journal);
        await _journalRepository.SaveChangesAsync();
    }
}
```

### 7.5 Domain Events vs Integration Events

| Aspect | Domain Event | Integration Event |
|--------|-------------|-------------------|
| Scope | Within one service | Cross-service |
| Type | `IDomainEvent` (extends `INotification + IEvent`) | Plain C# record |
| Raised in | Aggregate method via `AddDomainEvent()` | Integration event handler after domain event |
| Dispatched via | `SaveChangesInterceptor` -> `IDomainEventPublisher` | MassTransit `IPublishEndpoint` |
| Handlers | `INotificationHandler<T>` (MediatR) or `IEventHandler<T>` (FastEndpoints) | `IConsumer<T>` (MassTransit) |

---

## 8. BuildingBlocks

### 8.1 Project Inventory

| Project | Purpose | Key Types | Used By |
| --- | --- | --- | --- |
| `Articles.Abstractions` | Shared article action/event abstractions | `ArticleCommandBase`, `DomainEvent<T>`, `ArticleStageChanged`, `IArticleAction`, `ArticleStage`, `UserRoleType` | Domain-heavy services and contracts |
| `Articles.Grpc.Contracts` | gRPC service interfaces + proto contracts | `IPersonService`, `IJournalService`, `PersonInfo`, `JournalInfo` | Auth, Journals, Submission, Review |
| `Articles.Integration.Contracts` | Cross-service event contracts | `ArticleApprovedForReviewEvent`, `ArticleAcceptedForProductionEvent`, `ArticleDto` | Submission, Review, Production, ArticleHub, Journals |
| `Articles.Security` | JWT auth and article access authorization | `ConfigureAuthentication`, `ArticleAccessAuthorizationHandler`, `Role`, `ArticleRoleRequirement` | All API services except gateway |
| `Blocks.AspNetCore` | Middleware, HTTP providers, filters, gRPC client helpers | `GlobalExceptionMiddleware`, `HttpContextProvider`, `AssignUserIdFilter`, `AddCodeFirstGrpcClient<T>`, `RequestContextMiddleware` | API hosts |
| `Blocks.Core` | Guards, FluentValidation extensions, caching, Mapster helpers | `Guard`, `MaxLength`, `IClaimsProvider`, `JwtOptions`, `RequestContext`, `NotEmptyWithMessage` | Most projects |
| `Blocks.Domain` | Base domain types | `AggregateRoot<T>`, `Entity<T>`, `ValueObject`, `IDomainEvent`, `IDomainEventPublisher`, `DomainException` | All rich domain services |
| `Blocks.EntityFrameworkCore` | EF base context, repositories, interceptors | `ApplicationDbContext<T>`, `RepositoryBase`, `CachedRepository`, `DispatchDomainEventsInterceptor`, `AuditedEntityConfiguration<T>` | SQL-backed services/modules |
| `Blocks.Exceptions` | HTTP exception hierarchy | `HttpException`, `BadRequestException`, `NotFoundException`, `UnauthorizedException` | Shared |
| `Blocks.FastEndpoints` | FastEndpoints host helpers | `DomainEventPublisher`, `AssignUserIdPreProcessor`, `UseCustomFastEndpoints()` | Auth, Journals, Production |
| `Blocks.Http.Abstractions` | HTTP/file abstractions | File/download abstractions | Submission, Review |
| `Blocks.MediatR` | CQRS abstractions and pipeline behaviors | `ICommand`, `IQuery<T>`, `DomainEventPublisher`, `AssignUserIdBehavior`, `ValidationBehavior`, `LoggingBehavior` | Submission, Review, Production |
| `Blocks.Messaging` | MassTransit/RabbitMQ setup | `AddMassTransitWithRabbitMQ()`, `RabbitMqOptions`, `SnakeCaseWithServiceSuffixNameFormatter` | Messaging-enabled services |
| `Blocks.Redis` | Redis.OM repository | `Entity`, `Repository<T>` | Journals |
| `Blocks.Hasura` | Hasura GraphQL/metadata setup | `HasuraRegistration`, `HasuraMetadataInitService` | ArticleHub |

### 8.2 Exception Hierarchy

```
HttpException (has HttpStatusCode property)
├── BadRequestException  (400)
├── NotFoundException    (404)
└── UnauthorizedException (401)
```

`GlobalExceptionMiddleware` maps each type:
```csharp
private static HttpStatusCode MapStatusCode(Exception ex) => ex switch
{
    ValidationException    => HttpStatusCode.BadRequest,
    ArgumentException      => HttpStatusCode.BadRequest,
    BadRequestException    => HttpStatusCode.BadRequest,
    NotFoundException      => HttpStatusCode.NotFound,
    DomainException        => HttpStatusCode.BadRequest,
    UnauthorizedException  => HttpStatusCode.Unauthorized,
    _                      => HttpStatusCode.InternalServerError
};
```

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

### 10.1 Class Naming

| Pattern | Convention | Example |
|---------|-----------|---------|
| FastEndpoints endpoint | `{Feature}Endpoint` | `CreateUserEndpoint`, `AssignTypesetterEndpoint` |
| Carter endpoint | `{Feature}Endpoint : ICarterModule` | `AcceptArticleEndpoint`, `InviteReviewerEndpoint` |
| Minimal API endpoint | `static class {Feature}Endpoint` | `CreateArticleEndpoint`, `GetArticleEndpoint` |
| Command | `{Feature}Command : record` | `CreateArticleCommand`, `AcceptArticleCommand` |
| Query | `{Feature}Query : record` | `GetArticleQuery`, `SearchJournalsQuery` |
| Command handler | `{Feature}CommandHandler` | `CreateArticleCommandHandler` |
| Query handler | `{Feature}QueryHandler` | `GetArticleQueryHandler` |
| Validator (MediatR) | `{Command}Validator` in same file | `CreateArticleCommandValidator` |
| Validator (FastEndpoints) | `{Command}Validator : Validator<T>` in same file | `CreateJournalCommandValidator` |
| Integration event | `{Event}Event` | `ArticleAcceptedForProductionEvent` |
| Domain event | `{Event}` (no suffix) | `ArticleApproved`, `TypesetterAssigned` |
| Consumer | `{Event}Consumer : IConsumer<T>` | `ArticleApprovedForReviewConsumer` |
| Integration event handler | `{Action}On{Event}Handler` | `PublishIntegrationEventOnArticleAcceptedHandler` |
| Repository | `{Entity}Repository` | `ArticleRepository`, `PersonRepository` |
| DbContext | `{Service}DbContext` | `SubmissionDbContext`, `ReviewDbContext` |
| EF config | `{Entity}EntityConfiguration` | `ArticleEntityConfiguration` |
| Mapster config | `{Context}Mappings : IRegister` | `GrpcMappings`, `RestEndpointMappings` |

### 10.2 File Naming Per Feature

Each feature has its own folder. Files within:
- `{Feature}Command.cs` — command record + validator (co-located)
- `{Feature}CommandHandler.cs` — handler
- `{Feature}Endpoint.cs` — endpoint
- `{Feature}Summary.cs` — FastEndpoints Swagger summary (Production)

### 10.3 Variable and Field Naming

- Private fields: `_camelCase` (e.g., `_articleRepository`, `_domainEvents`)
- Constructor parameters (primary constructors): `_camelCase` (e.g., `(ArticleRepository _articleRepository, ...)`)
- Local variables and parameters: `camelCase` (e.g., `command`, `article`)
- Public properties: `PascalCase`
- No abbreviated names: full words always (never `req`, `cmd`, `res`, `ops`, `_q`, `_m`)

### 10.4 Event Naming

- Domain events are past-tense and local: `ArticleApproved`, `ReviewerInvited`, `TypesetterAssigned`
- Integration events make cross-service boundary explicit: `ArticleApprovedForReviewEvent`, `ArticleAcceptedForProductionEvent`, `JournalUpdatedEvent`

### 10.5 Folder Structure Conventions

Services with Application layer (Submission, Review):
```
{Service}.API/
    Endpoints/                    # one file per endpoint
    DependecyInjection.cs
    Program.cs

{Service}.Application/
    Features/
        {FeatureName}/
            {Feature}Command.cs   # or Query
            {Feature}Handler.cs
        _Shared/                  # base types shared across features
    Mappings/
    Dtos/
    StateMachines/
    DependencyInjection.cs

{Service}.Domain/
    Entities/                     # or named by aggregate in Production/Review
    Behaviours/                   # or Behavior/
    Events/
    ValueObjects/
    Enums/

{Service}.Persistence/
    EntityConfigurations/
    Repositories/
    Data/
        Master/                   # seed data
        Test/                     # test seed data
    {Service}DbContext.cs
    DependencyInjection.cs
```

Services without Application layer (Auth, Journals, Production):
```
{Service}.API/
    Features/
        {Domain}/
            {FeatureName}/
                {Feature}Command.cs
                {Feature}Endpoint.cs  # handler logic here
```

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

**Startup:**
```csharp
builder.Services
    .AddApiServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddPersistenceServices(builder.Configuration);

builder.Services
    .AddArticleTimeline(builder.Configuration)
    .AddAzureFileStorage(builder.Configuration);
```

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

No unit-test or integration-test projects were found. No `*Tests*.csproj` files or xUnit/NUnit/MSTest references are present.

### 12.2 Development/Test Seed Data

Multiple services bootstrap demo data from `Data/Test` JSON files during startup in development:

```csharp
// Submission
services.SeedTestData<SubmissionDbContext>(context =>
{
    context.SeedFromJsonFile<Person>();
    context.SeedFromJsonFile<Journal>();
});

// Auth
const string DefaultPassword = "Pass.123!";
var persons = context.LoadFromJson<Person>();
context.UseManualGenerateId<User>(true);

// Journals (Redis)
await provider.SeedFromJson<Editor>(redisDb);
await provider.SeedFromJson<Journal>(redisDb);
await redisDb.SetSequenceSeed<Journal>(7);
await redisDb.SetSequenceSeed<Section>(14);
```

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

### 13.2 API Gateway (YARP)

```csharp
// src/ApiGateway/Program.cs
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

app.Use(async (context, next) =>
{
    const string header = "X-Correlation-ID";
    if (!context.Request.Headers.ContainsKey(header))
        context.Request.Headers[header] = Guid.NewGuid().ToString();
    await next();
});
app.MapReverseProxy();
```

**Drift:** the gateway route addresses do not match current service HTTP ports for Submission, Production, and ArticleHub.

### 13.3 JWT Authentication Setup

```csharp
// src/BuildingBlocks/Articles.Security/ConfigureAuthentication.cs
public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
{
    var jwtOptions = configuration.GetSectionByTypeName<JwtOptions>();
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(jwtOptions.Secret)),
                ValidateAudience = false,
                RequireExpirationTime = true,
                RoleClaimType = ClaimTypes.Role
            };
        });
    return services;
}
```

### 13.4 DependencyInjection.cs Convention

Every layer of every service has a `DependencyInjection.cs` file with extension methods:

```csharp
// API layer
public static class DependecyInjection  // note: common typo in codebase
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config) { ... }
}

// Application layer
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config) { ... }
}

// Persistence layer
public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration config) { ... }
}
```

Called in `Program.cs`:
```csharp
builder.Services
    .ConfigureApiOptions(builder.Configuration)
    .AddApiServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddPersistenceServices(builder.Configuration);
```

### 13.5 Options Configuration Convention

`AddAndValidateOptions<T>()` binds and validates options by type name:
```csharp
services.AddAndValidateOptions<JwtOptions>(configuration)
services.AddAndValidateOptions<RabbitMqOptions>(configuration)
services.AddAndValidateOptions<TransactionOptions>(configuration)
```

`GetSectionByTypeName<T>()` reads the same section without DI:
```csharp
var grpcOptions = config.GetSectionByTypeName<GrpcServicesOptions>();
```

### 13.6 Middleware Pipeline Convention

All services share a similar ordering:
```csharp
app.UseSwagger()
   .UseSwaggerUI()
   .UseRouting()
   .UseMiddleware<GlobalExceptionMiddleware>()
   .UseAuthentication()
   .UseAuthorization();
```

Variations:
- FastEndpoints services: `.UseCustomFastEndpoints().UseSwaggerGen()`
- Carter services: `var api = app.MapGroup("/api"); api.MapCarter()`
- Submission: `app.MapAllEndpoints()`
- Journals: `.UseRedis()` for Redis.OM index initialization

### 13.7 File Service Module Selection

| Service | Registration call | Storage backend |
|---------|-----------------|----------------|
| Submission | `AddMongoFileStorageAsSingletone(config)` | MongoDB GridFS |
| Review | `AddMongoFileStorageAsSingletone(config)` + `AddMongoFileStorageAsScoped<SubmissionFileStorageOptions>` | Two MongoDB GridFS buckets |
| Production | `AddAzureFileStorage(config)` + `AddMongoFileStorageAsScoped<ReviewFileStorageOptions>` | Azure Blob + MongoDB |

All implementations register as `IFileService` (or `IFileService<TOptions>` for multi-instance scenarios).

---

*Document generated from scan of `src/` directory — all snippets extracted from actual source files.*
