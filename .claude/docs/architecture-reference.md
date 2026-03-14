# DotNetLabX Articles Application — Architecture Reference

This document is the definitive architecture reference for the Articles Application microservices project. It covers every major pattern with real file paths and code excerpts. It is intended for both AI agents and course students.

---

## Table of Contents

1. [Domain Modeling Patterns](#1-domain-modeling-patterns)
2. [Endpoint Patterns](#2-endpoint-patterns)
3. [CQRS Patterns](#3-cqrs-patterns)
4. [Validation Patterns](#4-validation-patterns)
5. [Mapping Patterns (Mapster)](#5-mapping-patterns-mapster)
6. [Error Handling](#6-error-handling)
7. [Auth and Security](#7-auth-and-security)
8. [Repository Patterns](#8-repository-patterns)
9. [Dependency Injection](#9-dependency-injection)
10. [Domain Events](#10-domain-events)
11. [Integration Events (MassTransit)](#11-integration-events-masstransit)
12. [gRPC Code-First](#12-grpc-code-first)
13. [EF Core Configuration](#13-ef-core-configuration)
14. [Multi-Tenancy](#14-multi-tenancy)
15. [File and Email Modules](#15-file-and-email-modules)
16. [Naming and Structural Conventions](#16-naming-and-structural-conventions)

---

## 1. Domain Modeling Patterns

### 1.1 AggregateRoot

**File:** `src/BuildingBlocks/Blocks.Domain/Entities/AggregateRoot.cs`

```csharp
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

// Convenience non-generic form (PK = int)
public abstract class AggregateRoot : AggregateRoot<int>, IAggregateRoot, IAuditedEntity;
```

Key points:
- Audit fields (`CreatedById`, `CreatedOn`, `LastModifiedById`, `LastModifiedOn`) live only on the aggregate — child entities inherit the aggregate's audit context.
- `_domainEvents` is a private `List<IDomainEvent>` exposed as `IReadOnlyList` to prevent external mutation.
- `AddDomainEvent` / `ClearDomainEvents` are the only mutation points.

### 1.2 Entity

**File:** `src/BuildingBlocks/Blocks.Domain/Entities/Entity.cs`

```csharp
public abstract class Entity<TPrimaryKey> : IEntity<TPrimaryKey>, IEquatable<Entity<TPrimaryKey>>
    where TPrimaryKey : struct
{
    public virtual TPrimaryKey Id { get; init; }
    public virtual bool IsNew => EqualityComparer<TPrimaryKey>.Default.Equals(Id, default);
    // Equality by Id; transient objects (IsNew) are never equal
}
```

### 1.3 Value Objects

**File:** `src/BuildingBlocks/Blocks.Domain/ValueObjects/ValueObject.cs`

Multi-property value objects extend `ValueObject` and implement `GetEqualityComponents()`:
```csharp
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
    // Equals/GetHashCode based on component sequence
}
```

Single-property value objects extend `StringValueObject` or `SingleValueObject<T>`:
```csharp
public abstract class StringValueObject : IValueObject
{
    public string Value { get; protected set; } = default!;
}
```

**Real examples:**

`src/Services/Auth/Auth.Domain/Persons/ValueObjects/EmailAddress.cs` — extends `StringValueObject`, static `Create()` factory with validation:
```csharp
public class EmailAddress : StringValueObject
{
    internal EmailAddress(string value)
    {
        Value = value;
        NormalizedEmail = value.ToUpperInvariant();
    }

    public static EmailAddress Create(string value)
    {
        Guard.ThrowIfNullOrWhiteSpace(value);
        Guard.ThrowIfFalse(IsValidEmail(value), "Invalid email format.");
        return new EmailAddress(value);
    }

    public static implicit operator EmailAddress(string value) => Create(value);
    public static implicit operator string(EmailAddress email) => email.Value;
}
```

`src/Services/Production/Production.Domain/Assets/ValueObjects/AssetName.cs` — simple `StringValueObject` with named factory:
```csharp
public class AssetName : StringValueObject
{
    private AssetName(string value) => Value = value;
    public static AssetName FromAssetType(AssetTypeDefinition assetType) => new AssetName(assetType.Name.ToString());
}
```

### 1.4 Aggregate Examples Per Service

**Submission — `Article`**

Files: `src/Services/Submission/Submission.Domain/Entities/Article.cs` (state/collections) + `src/Services/Submission/Submission.Domain/Behaviours/Article.cs` (behavior via `partial class`)

```csharp
public partial class Article : AggregateRoot
{
    public required string Title { get; init; }
    public ArticleStage Stage { get; set; }

    private readonly List<Asset> _assets = new();
    public IReadOnlyList<Asset> Assets => _assets.AsReadOnly();

    private readonly List<ArticleActor> _actors = new();
    public IReadOnlyList<ArticleActor> Actors => _actors.AsReadOnly();
}
```

Behavior (partial class):
```csharp
public void Approve(Person editor, IArticleAction<ArticleActionType> action, ArticleStateMachineFactory factory)
{
    _actors.Add(new ArticleActor { Person = editor, Role = UserRoleType.REVED });
    SetStage(ArticleStage.InitialApproved, action, factory);
    AddDomainEvent(new ArticleApproved(this, action));
}
```

**Auth — `User`**

File: `src/Services/Auth/Auth.Domain/Users/User.cs`

`User` extends `IdentityUser<int>` (ASP.NET Identity) and implements `IAggregateRoot` manually (no `AggregateRoot<T>` base class because `IdentityUser` does not permit it):
```csharp
public partial class User : IdentityUser<int>, IAggregateRoot
{
    private List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    public void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
    public void ClearDomainEvents() => _domainEvents.Clear();
    // Audit fields duplicated inline (Identity does not support init-only)
}
```

**Journals — `Journal`**

File: `src/Services/Journals/Journals.Domain/Journals/Journal.cs`

Journal is persisted in Redis, not SQL. It extends `Blocks.Redis.Entity` (not `AggregateRoot`) and is decorated for Redis.OM:
```csharp
[Document(StorageType = StorageType.Json, Prefixes = new[] { nameof(Journal) })]
public partial class Journal : Entity   // Blocks.Redis.Entity, not Blocks.Domain.Entity
{
    [Indexed] public required string Abbreviation { get; set; }
    [Searchable] public required string Name { get; ... }
    [Indexed(Sortable = true)] public required string NormalizedName { get; set; }
}
```

**Review and Production** both define `Article` as `partial class Article : AggregateRoot` following the Submission pattern, splitting state definition from behavior.

### 1.5 Partial Class Pattern (Behavior Split)

All services use `partial class` to separate:
- `Article.cs` — data shape (properties, backing collections)
- `Behaviors/Article.cs` or `Behaviour/Article.cs` — domain methods that mutate state and raise events

This keeps the data definition clean and the behavior co-located but visually separated.

---

## 2. Endpoint Patterns

Three distinct endpoint patterns are used across services.

### 2.1 FastEndpoints Pattern (Auth, Journals, Production)

FastEndpoints endpoints are classes that inherit `Endpoint<TRequest, TResponse>`. Route and method are declared via attributes or `Configure()` override. `HandleAsync` contains the logic.

**Pattern A — attribute-based configuration (most common):**

File: `src/Services/Auth/Auth.API/Features/Users/CreateAccount/CreateUserEndpoint.cs`

```csharp
[Authorize(Roles = Articles.Security.Role.UserAdmin)]
[HttpPost("users")]
[Tags("Users")]
public class CreateUserEndpoint(UserManager<User> _userManager, PersonRepository _personRepository, AuthDbContext _dbContext)
    : Endpoint<CreateUserCommand, CreateUserResponse>
{
    public override async Task HandleAsync(CreateUserCommand command, CancellationToken ct)
    {
        // validate, create domain object, save, publish events
        await PublishAsync(new UserCreated(user, resetPasswordToken));
        await Send.OkAsync(new CreateUserResponse(command.Email, user.Id, resetPasswordToken));
    }
}
```

**Pattern B — `Configure()` method split into partial class:**

File: `src/Services/Auth/Auth.API/Features/Users/SetPassword/SetPasswordEndpoint.Configure.cs`
```csharp
public partial class SetPasswordEndpoint
{
    public override void Configure()
    {
        AllowAnonymous();
        Post("/password/first-time", "/password/reset");
        Description(x => x
            .WithSummary("Set or reset user password")
            .WithTags("Password")
            .Produces<SetPasswordResponse>(StatusCodes.Status200OK));
    }
}
```

`src/Services/Auth/Auth.API/Features/Users/SetPassword/SetPasswordEndpoint.cs`
```csharp
public partial class SetPasswordEndpoint(UserManager<User> _userManager)
    : Endpoint<SetPasswordCommand, SetPasswordResponse>
{
    public override async Task HandleAsync(SetPasswordCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user == null) throw new BadRequestException($"User with email {command.Email} doesn't exist");
        await Send.OkAsync(new SetPasswordResponse(command.Email));
    }
}
```

**Journals combined FastEndpoints (handler merged with endpoint):**

File: `src/Services/Journals/Journals.API/Features/Journals/Search/SearchJournalsQueryHandler.cs`

In Journals, the FastEndpoints handler class also acts as the query handler — no separate MediatR layer:
```csharp
[Authorize]
[HttpGet("journals")]
[Tags("Journals")]
public class SearchJournalsQueryHandler(Repository<Journal> _repository, ...)
    : Endpoint<SearchJournalsQuery, SearchJournalsResponse>
{
    public override async Task HandleAsync(SearchJournalsQuery query, CancellationToken ct)
    {
        // query Redis directly, no MediatR dispatch
        await Send.OkAsync(response, cancellation: ct);
    }
}
```

**FastEndpoints app setup:**

File: `src/BuildingBlocks/Blocks.FastEndpoints/Extensions.cs`
```csharp
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
    c.Errors.ResponseBuilder = (failures, ctx, statusCode) => { ... };
});
```

### 2.2 Carter Pattern (Review, ArticleHub)

`ICarterModule` with `AddRoutes(IEndpointRouteBuilder)`. All Minimal API builder methods are called inline.

File: `src/Services/Review/Review.API/Endpoints/Articles/AcceptArticleEndpoint.cs`

```csharp
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
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
```

File: `src/Services/ArticleHub/ArticleHub.API/Articles/GetArticle/GetArticleEndpoint.cs`

```csharp
public class GetArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId:int}", async (int articleId, ArticleGraphQLReadStore graphQLReadStore, CancellationToken ct) =>
        {
            var article = await graphQLReadStore.GetArticleById(articleId, ct);
            return article == null ? Results.NotFound() : Results.Ok(article);
        })
        .RequireAuthorization()
        .WithName("GetArticle")
        .WithTags("Articles");
    }
}
```

Carter registration: `services.AddCarter()` in DI; `app.MapCarter()` in Program.cs.

### 2.3 Minimal APIs Pattern (Submission)

Each endpoint is a static class with a `Map(IEndpointRouteBuilder)` extension method.

File: `src/Services/Submission/Submission.API/Endpoints/CreateArticleEndpoint.cs`

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
        .Produces<IdResponse>(StatusCodes.Status201Created);
    }
}
```

All endpoints are composed via a registration class:

File: `src/Services/Submission/Submission.API/Endpoints/XEndpointRegistration.cs`

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

### 2.4 Comparison Table

| Service     | Framework       | Handler location       | MediatR dispatch |
|-------------|-----------------|------------------------|------------------|
| Auth        | FastEndpoints   | `HandleAsync` in endpoint | Yes (for some), direct for others |
| Journals    | FastEndpoints   | `HandleAsync` in endpoint | No (direct repository access) |
| Production  | FastEndpoints   | `HandleAsync` in endpoint | No (direct repository access) |
| Review      | Carter          | `IRequestHandler` in Application layer | Yes (ISender) |
| Submission  | Minimal APIs    | `IRequestHandler` in Application layer | Yes (ISender) |
| ArticleHub  | Carter          | Direct or none | Partial |

---

## 3. CQRS Patterns

### 3.1 Command Interface

File: `src/BuildingBlocks/Blocks.MediatR/Abstractions/ICommand.cs`

```csharp
public interface ICommand : ICommand<Unit> { }
public interface ICommand<out TResponse> : IRequest<TResponse> { }
```

### 3.2 Query Interface

File: `src/BuildingBlocks/Blocks.MediatR/Abstractions/IQuery.cs`

```csharp
public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : notnull
{ }
```

### 3.3 Command Record — Review

File: `src/Services/Review/Review.Application/Features/Articles/AcceptArticle/AcceptArticleCommand.cs`

```csharp
public record AcceptArticleCommand : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.AcceptArticle;
}
```

`ArticleCommand` base:
```csharp
public abstract record ArticleCommand : ArticleCommandBase<ArticleActionType>, IArticleAction, ICommand<IdResponse>;
```

`ArticleCommandBase` (from `Articles.Abstractions`):
```csharp
public abstract record ArticleCommandBase<TActionType> : IArticleAction<TActionType>
    where TActionType : Enum
{
    [JsonIgnore] public int ArticleId { get; set; }
    public string? Comment { get; init; }
    [JsonIgnore] public abstract TActionType ActionType { get; }
    [JsonIgnore] public DateTime CreatedOn => DateTime.UtcNow;
    [JsonIgnore] public int CreatedById { get; set; }
}
```

The `[JsonIgnore]` on `ArticleId` and `CreatedById` means these are not in the request body — they are populated from route parameters and JWT claims respectively.

### 3.4 Query Record — Review

File: `src/Services/Review/Review.Application/Features/Articles/GetArticle/GetArticleQuery.cs`

```csharp
public record GetArticleQuery(int ArticleId) : IQuery<GetArticleResonse>;
public record GetArticleResonse(ArticleDto ArticleSummary);
```

### 3.5 Command Handler — Review

File: `src/Services/Review/Review.Application/Features/Articles/AcceptArticle/AcceptArticleCommandHandler.cs`

```csharp
public class AcceptArticleCommandHandler(ArticleRepository _articleRepository, ArticleStateMachineFactory _stateMachineFactory)
    : IRequestHandler<AcceptArticleCommand, IdResponse>
{
    public async Task<IdResponse> Handle(AcceptArticleCommand command, CancellationToken cancellationToken)
    {
        var article = await _articleRepository.FindByIdOrThrowAsync(command.ArticleId);
        article.Accept(_stateMachineFactory, command);
        await _articleRepository.SaveChangesAsync();
        return new IdResponse(article.Id);
    }
}
```

Pattern: load aggregate from repository → call domain method → save → return response.

### 3.6 Query Handler — Review

File: `src/Services/Review/Review.Application/Features/Articles/GetArticle/GetArticleQueryHandler.cs`

```csharp
public class GetArticleQueryHandler(ArticleRepository _articleRepository)
    : IRequestHandler<GetArticleQuery, GetArticleResonse>
{
    public async Task<GetArticleResonse> Handle(GetArticleQuery command, CancellationToken ct)
    {
        var article = Guard.NotFound(await _articleRepository.GetFullArticleByIdAsync(command.ArticleId));
        return new GetArticleResonse(article.Adapt<ArticleDto>());
    }
}
```

### 3.7 Submission — Commands in Application layer

Submission separates the Application layer (`Submission.Application`) from the API layer (`Submission.API`). Handlers live in `Submission.Application`.

### 3.8 MediatR Pipeline Behaviors

All services using MediatR register three open behaviors:

```csharp
config.AddOpenBehavior(typeof(AssignUserIdBehavior<,>));
config.AddOpenBehavior(typeof(ValidationBehavior<,>));
config.AddOpenBehavior(typeof(LoggingBehavior<,>));
```

- **`AssignUserIdBehavior`** — sets `CreatedById` on any `IAuditableAction` from `IClaimsProvider`.
- **`ValidationBehavior`** — runs all `IValidator<TRequest>` and throws `ValidationException` on failure.
- **`LoggingBehavior`** — logs request/response timing.

---

## 4. Validation Patterns

### 4.1 FluentValidation Validator Structure

All validators extend `AbstractValidator<T>`:

File: `src/Services/Auth/Auth.API/Features/Users/CreateAccount/CreateUserCommandValidator.cs`

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmptyWithMessage(nameof(CreateUserCommand.FirstName));
        RuleFor(c => c.Email)
            .NotEmptyWithMessage(nameof(CreateUserCommand.Email))
            .EmailAddress().WithMessage("Email format is invalid.");
        RuleFor(c => c.UserRoles)
            .NotEmptyWithMessage(nameof(CreateUserCommand.UserRoles))
            .Must((c, roles) => AreUserRoleDatesValid(roles)).WithMessage("Invalid Role");
    }
}
```

Validators can be inlined in the same file as the command/query (Review pattern):

```csharp
// src/Services/Review/Review.Application/Features/Articles/GetArticle/GetArticleQuery.cs
public class GetArticleValidator : AbstractValidator<GetArticleQuery>
{
    public GetArticleValidator()
    {
        RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof(GetArticleQuery.ArticleId));
    }
}
```

Shared base validator for article commands:
```csharp
// src/Services/Review/Review.Application/Features/Articles/_Shared/ArticleCommand.cs
public abstract class ArticleCommandValidator<TFileActionCommand> : AbstractValidator<TFileActionCommand>
    where TFileActionCommand : IArticleAction
{
    public ArticleCommandValidator()
    {
        RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof(ArticleCommand.ArticleId));
    }
}
```

### 4.2 Custom Validation Extension Methods

File: `src/BuildingBlocks/Blocks.Core/FluentValidation/Extensions.cs`

```csharp
.NotEmptyWithMessage(propertyName)      // "PropertyName is required."
.WithMessageForInvalidId(propertyName)  // "The PropertyName should be greater than zero."
.WithMessageForMaxLength(name, maxLen)  // "PropertyName must not exceed N characters."
.GreaterThanWithMessageForInvalidId(0, propertyName)
.MaximumLengthWithMessage(maxLength, propertyName)
```

### 4.3 Validator Registration

MediatR services: `services.AddValidatorsFromAssemblyContaining<SomeValidator>()` — registers all validators as transient; `ValidationBehavior` pipeline automatically invokes them.

FastEndpoints services (Production, Journals, Auth): validators extend `Validator<T>` (FastEndpoints base) or `AbstractValidator<T>`. FastEndpoints discovers them automatically from the assembly.

Production uses a custom `BaseValidator<T> : Validator<T>` that logs validation failures:
```csharp
// src/Services/Production/Production.API/Features/_Shared/Validators.cs
public abstract class BaseValidator<T> : Validator<T>
{
    public BaseValidator()
    {
        RuleFor(command => command).NotEmpty().WithMessage(ValidatorsMessagesConstants.NotNull);
    }
}
```

---

## 5. Mapping Patterns (Mapster)

### 5.1 IRegister for TypeAdapterConfig

All mapping configurations implement `IRegister` and are auto-discovered at startup via scanning:

```csharp
public class RestEndpointMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ArticleActor, ActorDto>()
            .Include<ArticleAuthor, ActorDto>();
        config.NewConfig<Person, PersonDto>()
            .Include<Author, PersonDto>();
    }
}
```

### 5.2 Location Per Service

| Service    | Mapping file location                                                         |
|------------|-------------------------------------------------------------------------------|
| Submission | `src/Services/Submission/Submission.Application/Mappings/RestEndpointMappings.cs` |
| Submission | `src/Services/Submission/Submission.Application/Mappings/GrpcMappings.cs` |
| Review     | `src/Services/Review/Review.Application/Mappings/RestEndpointMappings.cs` |
| Review     | `src/Services/Review/Review.Application/Mappings/IntegrationEventMappings.cs` |
| Production | `src/Services/Production/Production.API/Features/_Shared/MappingConfig.cs` |
| Journals   | `src/Services/Journals/Journals.API/Features/_Shared/MappingConfig.cs` |
| Auth       | `src/Services/Auth/Auth.API/Mappings/GrpcTypeAdapterConfig.cs` |
| ArticleHub | `src/Services/ArticleHub/ArticleHub.API/Articles/Consumers/IntegrationEventsMappingConfig.cs` |

### 5.3 Registration

```csharp
// From a specific assembly
services.AddMapsterConfigsFromAssemblyContaining<GrpcMappings>();

// From the calling assembly (used in DI classes that are in the target assembly)
services.AddMapsterConfigsFromCurrentAssembly();

// Journals: manual scan
services.AddMapsterConfigsFromCurrentAssembly();
```

`src/BuildingBlocks/Blocks.Core/Mapster/DependencyInjection.cs` — `TypeAdapterConfig.GlobalSettings.Scan(assembly)`.

### 5.4 Common Patterns

**Inheritance mapping:**
```csharp
config.NewConfig<ArticleActor, ActorDto>().Include<ArticleAuthor, ActorDto>();
```

**Value object unwrapping:**
```csharp
config.NewConfig<Asset, AssetDto>()
    .Map(dest => dest.Name, src => src.Name.Value)  // StringValueObject.Value
    .Map(dest => dest.Number, src => src.Number.Value);
```

**Constructor mapping:**
```csharp
config.NewConfig<Journal, JournalDto>().MapToConstructor();
```

**gRPC-specific config (Auth):**
```csharp
public class GrpcTypeAdapterConfig : TypeAdapterConfig  // inherits TypeAdapterConfig
{
    public GrpcTypeAdapterConfig()
    {
        this.NewConfig<Person, PersonInfo>().IgnoreNullValues(true);
    }
}
```
This is registered as a singleton and injected into the gRPC service for explicit use with `person.Adapt<PersonInfo>(_typeAdapterConfig)`.

**Custom value object conversion:**
```csharp
config.ForType<string, EmailAddress>().MapWith(src => EmailAddress.Create(src));
```

---

## 6. Error Handling

### 6.1 Exception Hierarchy

File: `src/BuildingBlocks/Blocks.Exceptions/`

```
HttpException (base)
├── BadRequestException  → HTTP 400
├── NotFoundException    → HTTP 404
└── UnauthorizedException → HTTP 401
```

`DomainException` (in `Blocks.Domain`) also maps to HTTP 400 in the middleware.

### 6.2 Global Exception Middleware

File: `src/BuildingBlocks/Blocks.AspNetCore/Middlewares/GlobalExceptionMiddleware.cs`

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

Validation failures are serialized as `{ StatusCode, Message, TraceId, Errors: [{PropertyName, ErrorMessage}] }`.

All other exceptions produce `{ StatusCode, Message, TraceId, Details (stack trace in dev only) }`.

Registration in every `Program.cs`:
```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
```

### 6.3 Guard Class

File: `src/BuildingBlocks/Blocks.Core/Guard.cs`

```csharp
Guard.NotFound<T>(T? value)             // throws NotFoundException if null
Guard.AgainstNull<T>(T? value, name)    // throws ArgumentNullException
Guard.ThrowIfNullOrWhiteSpace(string)   // throws ArgumentException
Guard.ThrowIfFalse(bool, message)       // throws ArgumentException

// Extension method (same effect as Guard.NotFound):
value.OrThrowNotFound(message?)         // src/BuildingBlocks/Blocks.Core/GuardExtensions.cs
```

Usage in handlers:
```csharp
var article = Guard.NotFound(await _articleRepository.GetFullArticleByIdAsync(command.ArticleId));
```

---

## 7. Auth and Security

### 7.1 JWT Configuration

File: `src/BuildingBlocks/Articles.Security/ConfigureAuthentication.cs`

```csharp
public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
{
    var jwtOptions = configuration.GetSectionByTypeName<JwtOptions>();
    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(jwtOptions.Secret)),
                ValidateAudience = false,
                RoleClaimType = ClaimTypes.Role
            };
        });
    return services;
}
```

`JwtOptions` is bound from `appsettings.json` using `GetSectionByTypeName<T>()` (convention: section name = class name without "Options").

### 7.2 Role Constants

File: `src/BuildingBlocks/Articles.Security/Role.cs`

```csharp
public static class Role
{
    public const string UserAdmin  = nameof(UserRoleType.USERADMIN);
    public const string EditorAdmin = nameof(UserRoleType.EOF);
    public const string Author     = nameof(UserRoleType.AUT);
    public const string Editor     = nameof(UserRoleType.REVED);
    public const string Reviewer   = nameof(UserRoleType.REV);
    public const string ProdAdmin  = nameof(UserRoleType.POF);
    public const string Typesetter = nameof(UserRoleType.TSOF);
}
```

These map to the `UserRoleType` enum in `Articles.Abstractions`.

### 7.3 Role-Based Authorization on Endpoints

**FastEndpoints** (attribute-based):
```csharp
[Authorize(Roles = Role.UserAdmin)]
public class CreateUserEndpoint : Endpoint<...>
```

**Carter / Minimal API** (extension method):
```csharp
.RequireRoleAuthorization(Role.Editor, Role.EditorAdmin)
```

File: `src/BuildingBlocks/Articles.Security/Extensions.cs`
```csharp
public static TBuilder RequireRoleAuthorization<TBuilder>(this TBuilder builder, params string[] roles)
    => builder.RequireAuthorization(policy =>
    {
        policy.RequireRole(roles);
        policy.Requirements.Add(new ArticleRoleRequirement(roles));
    });
```

### 7.4 ArticleRoleRequirement and Handler

`ArticleRoleRequirement` carries the set of allowed `UserRoleType` values.

`ArticleAccessAuthorizationHandler` checks that the authenticated user has a matching role AND that the role is specifically tied to the article being accessed (via `IArticleAccessChecker`):

File: `src/BuildingBlocks/Articles.Security/ArticleAccessAuthorizationHandler.cs`

```csharp
protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ArticleRoleRequirement requirement)
{
    var userRoles = _httpProvider.GetUserRoles<UserRoleType>()
                        .Where(requirement.AllowedRoles.Contains)
                        .ToHashSet();

    if (userRoles.Count > 0 && await HasUserRoleForArticle(userRoles))
        context.Succeed(requirement);
}

private async Task<bool> HasUserRoleForArticle(IReadOnlySet<UserRoleType> userRoles)
    => await _articleRoleChecker.HasAccessAsync(
        _httpProvider.GetArticleId(), _httpProvider.GetUserId(), userRoles, ...);
```

Registration: `services.AddScoped<IAuthorizationHandler, ArticleAccessAuthorizationHandler>()` (in all services that use article-level authorization).

### 7.5 IClaimsProvider

File: `src/BuildingBlocks/Blocks.Core/Security/IClaimsProvider.cs`

```csharp
public interface IClaimsProvider
{
    int GetUserId();
    int? TryGetUserId();
    string GetUserEmail();
    IReadOnlySet<TEnum> GetUserRoles<TEnum>() where TEnum : struct, Enum;
    IReadOnlySet<string> GetUserRoles();
    string GetClaimValue(string claimName);
}
```

`HttpContextProvider` implements both `IClaimsProvider` and `IRouteProvider` and is registered as scoped:
```csharp
services.AddScoped<IClaimsProvider, HttpContextProvider>()
services.AddScoped<IRouteProvider, HttpContextProvider>()
services.AddScoped<HttpContextProvider>();
```

---

## 8. Repository Patterns

### 8.1 EF Core RepositoryBase

File: `src/BuildingBlocks/Blocks.EntityFrameworkCore/Repositories/Repository.cs`

```csharp
public class RepositoryBase<TContext, TEntity>(TContext dbContext)
    : RepositoryBase<TContext, TEntity, int>(dbContext)
    where TContext : DbContext
    where TEntity : class, IEntity<int>;

public abstract class RepositoryBase<TContext, TEntity, TKey>
    : IRepository<TEntity, TKey>
{
    protected readonly TContext _dbContext;
    protected readonly DbSet<TEntity> _entity;

    public virtual IQueryable<TEntity> Query() => _entity;
    public virtual IQueryable<TEntity> QueryNotTracked() => _entity.AsNoTracking();

    public async Task<TEntity?> FindByIdAsync(TKey id) => await _entity.FindAsync(id);
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default)
        => await Query().SingleOrDefaultAsync(e => e.Id.Equals(id), ct);
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
        => (await _entity.AddAsync(entity, ct)).Entity;
    public virtual Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _dbContext.SaveChangesAsync(ct);
    // ...also: UpsertAsync, DeleteByIdAsync, AddRangeAsync, etc.
}
```

**Concrete repositories** extend this, override `Query()` to include related data:

```csharp
// src/Services/Auth/Auth.Persistence/Repositories/PersonRepository.cs
public class PersonRepository(AuthDbContext dbContext) : RepositoryBase<AuthDbContext, Person>(dbContext)
{
    public override IQueryable<Person> Query()
        => base.Query().Include(p => p.User);

    public async Task<Person?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Query().SingleOrDefaultAsync(p => p.Email.NormalizedEmail == email.ToUpperInvariant(), ct);
}
```

**Generic Repository<T> pattern (Submission, Review, Production):**

Services also register `Repository<>` generically for entity types that don't need custom queries:
```csharp
services.AddScoped(typeof(Repository<>));
services.AddDerivedTypesOf(typeof(Repository<>)); // auto-registers subclasses
```

### 8.2 CachedRepository

File: `src/BuildingBlocks/Blocks.EntityFrameworkCore/Repositories/CachedRepository.cs`

For read-heavy static data (e.g., `AssetTypeDefinition`). Entities must implement `ICacheable`:
```csharp
public abstract class CachedRepository<TDbContext, TEntity, TId>(TDbContext _dbContext, IMemoryCache _cache)
{
    public IEnumerable<TEntity> GetAll()
        => _cache.GetOrCreateByType(entry => _dbContext.Set<TEntity>().AsNoTracking().ToList());

    public TEntity GetById(TId id) => GetAll().Single(e => e.Id.Equals(id));
}
```

`ApplicationDbContext` also supports caching directly:
```csharp
// src/BuildingBlocks/Blocks.EntityFrameworkCore/ApplicationDbContext.cs
public virtual IEnumerable<TEntity> GetAllCached<TEntity>()
    where TEntity : class, ICacheable
    => _cache.GetOrCreateByType(entry => this.Set<TEntity>().AsNoTracking().ToList());
```

### 8.3 Redis.OM Repository (Journals)

File: `src/BuildingBlocks/Blocks.Redis/Repository.cs`

```csharp
public class Repository<T> where T : Entity
{
    private readonly IRedisCollection<T> _collection;

    public IRedisCollection<T> Collection => _collection;
    public RedisAggregationSet<T> Aggregate => _provider.AggregationSet<T>();

    public async Task AddAsync(T entity)
    {
        if (entity.Id == 0) entity.Id = await GenerateNewId();
        await _collection.InsertAsync(entity);
    }

    public async Task UpdateAsync(T entity) => await _collection.UpdateAsync(entity);

    public async Task ReplaceAsync(T entity)
    {
        // Workaround for Redis.OM not updating child collections properly
        await _collection.DeleteAsync(entity);
        await _collection.InsertAsync(entity);
    }
}
```

ID generation uses Redis string increment: `await _redisDb.StringIncrementAsync($"{typeof(T).Name}:Id:Sequence")`.

Journals entities decorated with `[Document]`, `[Indexed]`, `[Searchable]` — see `Journal.cs`.

---

## 9. Dependency Injection

### 9.1 Convention: Per-Layer DI Classes

Each service has `DependencyInjection.cs` (or `DependecyInjection.cs`) in each layer:
- `ServiceName.API/DependencyInjection.cs` — registers API-level services (framework setup, gRPC clients, auth, modules)
- `ServiceName.Application/DependencyInjection.cs` — registers MediatR, validators, Mapster, MassTransit, domain services
- `ServiceName.Persistence/DependencyInjection.cs` — registers DbContext, interceptors, repositories

```csharp
// Program.cs pattern (all services)
builder.Services
    .ConfigureApiOptions(builder.Configuration)
    .AddApiServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddPersistenceServices(builder.Configuration);
```

### 9.2 gRPC Client Registration

File: `src/BuildingBlocks/Blocks.AspNetCore/Grpc/GrpcClientRegistrationExtensions.cs`

```csharp
public static IServiceCollection AddCodeFirstGrpcClient<TClient>(
    this IServiceCollection services, GrpcServicesOptions grpcOptions, string? serviceKey = null)
    where TClient : class
{
    // Resolves URL from GrpcServicesOptions["Person"], ["Journal"], etc.
    services.AddScoped(sp =>
    {
        var channel = GrpcChannel.ForAddress(serviceSettings.Url, ...);
        return channel.CreateGrpcService<TClient>();
    });
    return services;
}
```

Usage:
```csharp
services.AddCodeFirstGrpcClient<IPersonService>(grpcOptions, "Person");
services.AddCodeFirstGrpcClient<IJournalService>(grpcOptions, "Journal");
```

### 9.3 MassTransit Registration

File: `src/BuildingBlocks/Blocks.Messaging/MassTransit/DependencyInjection.cs`

```csharp
services.AddMassTransit(config =>
{
    config.SetEndpointNameFormatter(new SnakeCaseWithServiceSuffixNameFormatter(serviceName));
    config.AddConsumers(assembly);          // auto-discovers IConsumer<T> in the assembly
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitMqOptions.Host), rabbitMqOptions.VirtualHost, h =>
        {
            h.Username(rabbitMqOptions.UserName);
            h.Password(rabbitMqOptions.Password);
        });
        cfg.ConfigureEndpoints(context);
    });
});
```

Called from Application layer DI:
```csharp
services.AddMassTransitWithRabbitMQ(configuration, Assembly.GetExecutingAssembly());
```

### 9.4 Module Registration

**Email service:**
```csharp
services.AddEmptyEmailService(config);    // no-op implementation for dev/test
// services.AddSmtpEmailService(config);   // real SMTP
```

**File storage:**
```csharp
services.AddMongoFileStorageAsSingletone(config);  // default singleton
services.AddMongoFileStorageAsScoped<SubmissionFileStorageOptions>(config);  // scoped per options type
services.AddAzureFileStorage(configuration);       // Production uses Azure
services.AddFileServiceFactory();                   // factory pattern for multiple storages
```

### 9.5 gRPC Server Registration

```csharp
services.AddCodeFirstGrpc(options =>
{
    options.ResponseCompressionLevel = CompressionLevel.Fastest;
    options.EnableDetailedErrors = true;
});
// Then in app pipeline:
app.MapGrpcService<PersonGrpcService>();
```

---

## 10. Domain Events

### 10.1 IDomainEvent Interface

File: `src/BuildingBlocks/Blocks.Domain/IDomainEvent.cs`

```csharp
public interface IDomainEvent : INotification, IEvent;
// INotification = MediatR; IEvent = FastEndpoints
```

This dual inheritance allows domain events to be published via either MediatR or FastEndpoints without service-specific coupling.

### 10.2 DomainEvent Base Record

File: `src/BuildingBlocks/Articles.Abstractions/DomainEvent.cs`

```csharp
public abstract record DomainEvent<TAction>(TAction Action) : IDomainEvent
    where TAction : IArticleAction;
```

Service-specific convenience alias:
```csharp
// src/Services/Submission/Submission.Domain/Events/DomainEvent.cs
public record DomainEvent(IArticleAction Action) : DomainEvent<IArticleAction>(Action);
```

Events are simple records:
```csharp
public record ArticleApproved(Article Article, IArticleAction Action) : DomainEvent(Action);
public record ArticleStageChanged(ArticleStage CurrentStage, ArticleStage NewStage, IArticleAction action) : DomainEvent(action);
```

Journals uses `IDomainEvent` directly (no shared `DomainEvent` base needed):
```csharp
public record JournalCreated(Journal Journal) : IDomainEvent;
```

### 10.3 Raising Events in Aggregates

```csharp
// Inside Article behavior partial class
public void Approve(Person editor, IArticleAction<ArticleActionType> action, ArticleStateMachineFactory factory)
{
    // ... state changes ...
    AddDomainEvent(new ArticleApproved(this, action));
}
```

### 10.4 Dispatch via SaveChangesInterceptor

File: `src/BuildingBlocks/Blocks.EntityFrameworkCore/Interceptors/DispatchDomainEventsInterceptor.cs`

```csharp
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

`DispatchDomainEventsAsync` (extension method) scans `ChangeTracker` for all aggregates with pending events, clears them, then publishes each:
```csharp
var aggregates = ctx.ChangeTracker.Entries()
    .Select(a => a.Entity)
    .OfType<IAggregateRoot>()
    .Where(a => a.DomainEvents.Any())
    .ToList();
```

**Transactional variant** (`TransactionalDispatchDomainEventsInterceptor`, used by Production):
- Wraps save + dispatch in a single DB transaction.
- The transaction is committed after events are dispatched, so event handlers that save data use the same transaction.

### 10.5 IDomainEventPublisher Implementations

**MediatR publisher** (Submission, Review — uses MediatR `IMediator.Publish`):
```csharp
// src/BuildingBlocks/Blocks.MediatR/DomainEventPublisher.cs
public sealed class DomainEventPublisher(IMediator mediator) : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent @event, CancellationToken ct = default)
        => mediator.Publish(@event, ct);
}
```

**FastEndpoints publisher** (Auth, Journals, Production — uses FastEndpoints `IEvent.PublishAsync`):
```csharp
// src/BuildingBlocks/Blocks.FastEndpoints/DomainEventPublisher.cs
public sealed class DomainEventPublisher : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent @event, CancellationToken ct = default)
        => @event.PublishAsync(Mode.WaitForAll, ct);
}
```

### 10.6 Domain Event Handler Examples

**FastEndpoints handler** (Auth — sends email after user creation):
```csharp
// src/Services/Auth/Auth.API/Features/Users/CreateAccount/SendConfirmationEmailOnUserCreatedHandler.cs
public class SendConfirmationEmailOnUserCreatedHandler(...) : IEventHandler<UserCreated>
{
    public async Task HandleAsync(UserCreated notification, CancellationToken ct)
    {
        await emailService.SendEmailAsync(BuildConfirmationEmail(...));
    }
}
```

**MediatR handler** (Review — publishes integration event after article accepted):
```csharp
// src/Services/Review/Review.Application/Features/Articles/AcceptArticle/PublishIntegrationEventOnArticleAcceptedHandler.cs
public class PublishIntegrationEventOnArticleAcceptedHandler(...) : INotificationHandler<ArticleAccepted>
{
    public async Task Handle(ArticleAccepted notification, CancellationToken ct)
    {
        var article = await _articleRepository.GetFullArticleByIdAsync(notification.Article.Id);
        await _publishEndpoint.Publish(new ArticleAcceptedForProductionEvent(article.Adapt<ArticleDto>()), ct);
    }
}
```

**MediatR handler** (Journals — publishes integration event after journal created):
```csharp
// src/Services/Journals/Journals.API/Features/Journals/Create/PublishIntegrationEventOnJournalCreatedHandler.cs
public class PublishIntegrationEventOnJournalCreatedHandler(...) : IEventHandler<JournalCreated>
{
    public async Task HandleAsync(JournalCreated notification, CancellationToken ct)
    {
        await _publishEndpoint.Publish(new JournalCreatedEvent(notification.Journal.Adapt<JournalDto>()), ct);
    }
}
```

---

## 11. Integration Events (MassTransit)

### 11.1 Event Contract Structure

File: `src/BuildingBlocks/Articles.Integration.Contracts/`

All integration events are simple records with a DTO payload:
```csharp
// ArticleAcceptedForProductionEvent.cs
public record ArticleAcceptedForProductionEvent(ArticleDto Article);

// ArticleApprovedForReviewEvent.cs
public record ArticleApprovedForReviewEvent(ArticleDto Article);

// JournalCreatedEvent.cs
public record JournalCreatedEvent(JournalDto Journal);
```

DTOs (`ArticleDto`, `PersonDto`, `JournalDto`) carry the full article state — they are not EF/domain objects.

### 11.2 Publishing Integration Events

From within a domain event handler using `IPublishEndpoint` (MassTransit):
```csharp
await _publishEndpoint.Publish(new ArticleAcceptedForProductionEvent(articleDto), ct);
```

### 11.3 Consumer Implementation

File: `src/Services/ArticleHub/ArticleHub.API/Articles/Consumers/ArticleAcceptedForProductionConsumer.cs`

```csharp
public sealed class ArticleAcceptedForProductionConsumer(ArticleHubDbContext _dbContext)
    : IConsumer<ArticleAcceptedForProductionEvent>
{
    public async Task Consume(ConsumeContext<ArticleAcceptedForProductionEvent> ctx)
    {
        var articleDto = ctx.Message.Article;
        var article = await _dbContext.Articles
            .Include(a => a.Actors)
            .SingleOrThrowAsync(a => a.Id == articleDto.Id);

        article.Title = articleDto.Title;
        article.Stage = articleDto.Stage;

        await AddReviewers(articleDto, article);
        await _dbContext.SaveChangesAsync();
    }
}
```

Consumers must be idempotent. The ArticleHub consumer handles this by checking for existing records before inserting.

### 11.4 Consumer Registration

Consumers are auto-discovered from the assembly by MassTransit:
```csharp
config.AddConsumers(assembly); // discovers all IConsumer<T> in the assembly
```

Queue naming is handled by `SnakeCaseWithServiceSuffixNameFormatter` which appends the service name.

---

## 12. gRPC Code-First

### 12.1 Contract Definition Pattern

File: `src/BuildingBlocks/Articles.Grpc.Contracts/Auth/PersonContracts.cs`

Code-first gRPC uses `[ServiceContract]` and `[OperationContract]` from `System.ServiceModel` (ProtoBuf.Grpc):

```csharp
[ServiceContract]
public interface IPersonService
{
    [OperationContract]
    ValueTask<GetPersonResponse> GetPersonByIdAsync(GetPersonRequest request, CallContext context = default);
    [OperationContract]
    ValueTask<GetPersonByUserIdAsync(GetPersonByUserIdRequest request, CallContext context = default);
    // ...
}
```

Request/response messages use `[ProtoContract]` and `[ProtoMember(N)]`:
```csharp
[ProtoContract]
public class GetPersonResponse
{
    [ProtoMember(1)]
    public PersonInfo PersonInfo { get; set; } = default!;
}

[ProtoContract]
public class PersonInfo
{
    [ProtoMember(1)] public int Id { get; set; }
    [ProtoMember(2)] public string FirstName { get; set; } = default!;
    [ProtoMember(6, IsRequired = false)] public string? Honorific { get; set; }
    // ...
}
```

### 12.2 Server Implementation

File: `src/Services/Auth/Auth.API/Features/Persons/PersonGrpcService.cs`

The gRPC service implements the interface contract directly:
```csharp
public class PersonGrpcService(PersonRepository _personRepository, GrpcTypeAdapterConfig _typeAdapterConfig)
    : IPersonService
{
    public async ValueTask<GetPersonResponse> GetPersonByIdAsync(GetPersonRequest request, CallContext context = default)
        => await GetPersonResponseAsync(() => _personRepository.GetByIdAsync(request.PersonId));

    private async ValueTask<GetPersonResponse> GetPersonResponseAsync(Func<Task<Person?>> fetch)
    {
        var person = Guard.NotFound(await fetch());
        return new GetPersonResponse { PersonInfo = person.Adapt<PersonInfo>(_typeAdapterConfig) };
    }
}
```

Registered in `Program.cs`:
```csharp
app.MapGrpcService<PersonGrpcService>();
```

### 12.3 Client Registration

File: `src/BuildingBlocks/Blocks.AspNetCore/Grpc/GrpcClientRegistrationExtensions.cs`

```csharp
services.AddCodeFirstGrpcClient<IPersonService>(grpcOptions, "Person");
```

`grpcOptions` is loaded from `appsettings.json` under `GrpcServicesOptions.Services["Person"].Url`. The client is registered as scoped using `GrpcChannel.CreateGrpcService<TClient>()`.

### 12.4 Using gRPC Client in Handlers/Endpoints

Journals `CreateJournalEndpoint` calling Auth Person gRPC:
```csharp
public class CreateJournalEndpoint(Repository<Journal> _journalRepository, IPersonService _personClient)
    : Endpoint<CreateJournalCommand, IdResponse>
{
    private async Task<Editor> CreateEditor(int userId, CancellationToken ct)
    {
        var response = await _personClient.GetPersonByUserIdAsync(
            new GetPersonByUserIdRequest { UserId = userId },
            new CallOptions(cancellationToken: ct));
        var editor = Editor.Create(response.PersonInfo);
        // ...
    }
}
```

### 12.5 Contracts Location

- `src/BuildingBlocks/Articles.Grpc.Contracts/Auth/PersonContracts.cs` — Person service contract
- `src/BuildingBlocks/Articles.Grpc.Contracts/Journals/JournalContracts.cs` — Journal service contract

Both are shared across services that need them.

---

## 13. EF Core Configuration

### 13.1 DbContext Setup

**Auth (SQL Server + ASP.NET Identity):**
```csharp
// src/Services/Auth/Auth.Persistence/AuthDbContext.cs
public class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<User, Role, int>(options)
{
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
    }
}
```

**Submission/Review/Production (SQL Server):**
Same `ApplyConfigurationsFromAssembly` pattern. DbContext configured with interceptors:
```csharp
services.AddDbContext<SubmissionDbContext>((provider, options) =>
{
    options.AddInterceptors(provider.GetServices<ISaveChangesInterceptor>());
    options.UseSqlServer(dbConnection);
});
```

**Journals (Redis — no EF Core):**
`JournalDbContext` is a custom class wrapping `RedisConnectionProvider`, not EF at all.

### 13.2 Entity Configurations

All configurations extend base configuration classes from `Blocks.EntityFrameworkCore`:

- `EntityConfiguration<T>` — sets `HasKey(e => e.Id)`, handles generated/manual ID, seeds from JSON.
- `AuditedEntityConfiguration<T>` — adds `CreatedOn`, `CreatedById`, `LastModifiedOn`, `LastModifiedById` columns.

```csharp
// src/Services/Auth/Auth.Persistence/EntityConfigurations/PersonEntityConfiguration.cs
internal class PersonEntityConfiguration : AuditedEntityConfiguration<Person>
{
    public override void Configure(EntityTypeBuilder<Person> builder)
    {
        base.Configure(builder);
        builder.Property(e => e.FirstName).HasMaxLength(MaxLength.C64).IsRequired();
        builder.OwnsOne(e => e.Email, b =>
        {
            b.Property(n => n.Value).HasColumnName(nameof(Person.Email)).HasMaxLength(MaxLength.C64);
            b.HasIndex(e => e.NormalizedEmail).IsUnique();
        });
        builder.OwnsOne(e => e.ProfessionalProfile, b => { ... });
    }
}
```

Value objects are mapped using `OwnsOne` (preferred over `ComplexProperty` due to EF Core limitations with optional properties and indexes).

### 13.3 Interceptors

| Service    | Interceptor Used                               | Effect |
|------------|------------------------------------------------|--------|
| Submission | `DispatchDomainEventsInterceptor`              | Dispatches events after `SaveChanges`, no transaction wrapping |
| Review     | `DispatchDomainEventsInterceptor`              | Same as Submission |
| Production | `TransactionalDispatchDomainEventsInterceptor` | Wraps save + dispatch in a single SQL transaction |

Registration:
```csharp
services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
// or
services.AddScoped<ISaveChangesInterceptor, TransactionalDispatchDomainEventsInterceptor>();
```

### 13.4 Migration Workflow

```bash
# Add migration
dotnet ef migrations add MigrationName \
  -p Services/ServiceName/ServiceName.Persistence \
  -s Services/ServiceName/ServiceName.API

# Apply migration
dotnet ef database update \
  -p Services/ServiceName/ServiceName.Persistence \
  -s Services/ServiceName/ServiceName.API
```

Each service has its own `*DbContextDesignTimeFactory` for CLI migration support.

---

## 14. Multi-Tenancy

### 14.1 TenantDbContext

File: `src/BuildingBlocks/Blocks.EntityFrameworkCore/TenantDbContext.cs`

Applies a global query filter for all `IMultitenancy` entities using the configured `TenantId`:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(IMultitenancy).IsAssignableFrom(entityType.ClrType))
        {
            modelBuilder.Entity(entityType.ClrType)
                .AddQueryFilter<IMultitenancy>(e => e.TenantId.Equals(TenantId));
        }
    }
}
```

`SaveChangesAsync` automatically stamps `TenantId` on added entities.

### 14.2 TenantOptions

File: `src/BuildingBlocks/Blocks.EntityFrameworkCore/TenantOptions.cs`

```csharp
public class TenantOptions
{
    public int TenantId { get; set; }
}
```

Bound from configuration. Note: `TenantDbContext` is currently `internal` — it is a building block not yet used by any service but available for future use.

### 14.3 TenantRepositoryBase

File: `src/BuildingBlocks/Blocks.EntityFrameworkCore/Repositories/TenantRepositoryBase.cs`

Extends `RepositoryBase` and uses composite key (`TenantId + entityId`) for lookups via EF's `FindAsync`.

---

## 15. File and Email Modules

### 15.1 IFileService Interface

File: `src/Modules/FileService/FileService.Contracts/IFileService.cs`

```csharp
public interface IFileService
{
    string GenerateId();
    Task<FileMetadata> UploadAsync(string storagePath, IFormFile file, bool overwrite = false,
        Dictionary<string, string>? tags = null, CancellationToken ct = default);
    Task<IEnumerable<string>> FindFileIdsByTagAsync(string key, string value, CancellationToken ct = default);
    Task<bool> TryDeleteAsync(string fileId, CancellationToken ct = default);
    Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadAsync(string fileId, CancellationToken ct = default);
    Task<(Stream FileStream, FileMetadata FileMetadata)> DownloadByTagAsync(string key, string value, CancellationToken ct = default);
}

// Typed variant for supporting multiple storage backends in one service
public interface IFileService<TFileStorageOptions> : IFileService
    where TFileStorageOptions : IFileStorageOptions;
```

### 15.2 Available Implementations

| Implementation         | Package               | Used By    |
|------------------------|-----------------------|------------|
| `FileService.MongoGridFS` | MongoDB GridFS    | Submission, Review |
| `FileService.AzureBlob`   | Azure Blob Storage | Production |
| `FileStorage.MinIO`       | MinIO              | Available  |

### 15.3 Registration Patterns

```csharp
// Single storage singleton (Submission)
services.AddMongoFileStorageAsSingletone(config);

// Multiple storages in same service (Review — uses typed IFileService<TOptions>)
services.AddMongoFileStorageAsSingletone(config);
services.AddMongoFileStorageAsScoped<SubmissionFileStorageOptions>(config);
services.AddFileServiceFactory();

// Azure Blob (Production)
services.AddAzureFileStorage(configuration);
```

### 15.4 IEmailService Interface

File: `src/Modules/EmailService/EmailService.Contracts/IEmailService.cs`

```csharp
public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailMessage emailMessage, CancellationToken ct = default);
}
```

### 15.5 Email Implementations

| Implementation         | Notes                              |
|------------------------|------------------------------------|
| `EmailService.Empty`   | No-op; used in dev/test           |
| `EmailService.Smtp`    | Real SMTP via MailKit              |
| `EmailService.SendGrid` | SendGrid API                     |

Registration (all services default to Empty):
```csharp
services.AddEmptyEmailService(config);
// Swap to: services.AddSmtpEmailService(config);
```

### 15.6 Usage Pattern

```csharp
// In domain event handler
public class SendConfirmationEmailOnUserCreatedHandler(IEmailService emailService, ...)
    : IEventHandler<UserCreated>
{
    public async Task HandleAsync(UserCreated notification, CancellationToken ct)
    {
        var emailMessage = new EmailMessage(
            subject: "Your Account Has Been Created",
            content: new Content(ContentType.Html, body),
            from: new EmailAddress("articles", fromAddress),
            to: new List<EmailAddress> { new EmailAddress(user.FullName, user.Email!) }
        );
        await emailService.SendEmailAsync(emailMessage);
    }
}
```

---

## 16. Naming and Structural Conventions

### 16.1 File Naming Per Framework

| Pattern    | Files                                               |
|------------|-----------------------------------------------------|
| FastEndpoints | `{Feature}Endpoint.cs`, optionally split into `{Feature}Endpoint.Configure.cs` |
| Carter     | `{Feature}Endpoint.cs` (contains the `ICarterModule` class) |
| Minimal API | `{Feature}Endpoint.cs` (static class with `Map()`) |
| Command    | `{Feature}Command.cs` (also contains Response record and Validator if small) |
| Handler    | `{Feature}CommandHandler.cs` or `{Feature}QueryHandler.cs` |
| Domain event handler | `{Action}On{Event}Handler.cs` (e.g., `PublishIntegrationEventOnArticleAcceptedHandler`) |
| Mapping    | `{Context}MappingConfig.cs` or `{Context}Mappings.cs` |
| Validator  | `{Feature}Validator.cs` or `{Feature}CommandValidator.cs` |

### 16.2 Namespace Conventions

Namespaces follow the folder structure exactly:
- `Auth.API.Features.Users.CreateAccount`
- `Review.Application.Features.Articles.AcceptArticle`
- `Submission.Domain.Entities`
- `Blocks.EntityFrameworkCore.Interceptors`

### 16.3 Field and Variable Naming

| Scope              | Convention             | Example                      |
|--------------------|------------------------|------------------------------|
| Private fields     | `_camelCase`           | `_domainEvents`, `_dbContext` |
| Constructor params | `_camelCase` (primary ctor) | `(ArticleRepository _articleRepository)` |
| Locals/params      | descriptive `camelCase` | `article`, `articleDto`, `connectionString` |
| Public members     | `PascalCase`           | `DomainEvents`, `SaveChangesAsync` |
| Constants          | `PascalCase` or `ALLCAPS` enum | `Role.UserAdmin`, `UserRoleType.REVED` |

Primary constructor parameters that are kept as fields use the `_camelCase` prefix directly in the parameter list (C# 12 pattern).

### 16.4 Folder Structure Per Service

```
Services/{ServiceName}/
├── {ServiceName}.API/
│   ├── Features/
│   │   └── {Domain}/
│   │       └── {Feature}/
│   │           ├── {Feature}Endpoint.cs
│   │           ├── {Feature}Command.cs
│   │           └── {Feature}CommandValidator.cs
│   ├── DependencyInjection.cs
│   └── Program.cs
├── {ServiceName}.Application/    (if separate from API)
│   ├── Features/
│   │   └── {Domain}/{Feature}/
│   │       ├── {Feature}Command.cs
│   │       ├── {Feature}CommandHandler.cs
│   │       └── {Event}Handler.cs
│   ├── Mappings/
│   └── DependencyInjection.cs
├── {ServiceName}.Domain/
│   ├── {AggregateRoot}/
│   │   ├── {AggregateRoot}.cs       (state)
│   │   ├── Behaviors/{AggregateRoot}.cs  (partial — behavior)
│   │   ├── Events/
│   │   └── ValueObjects/
│   └── GlobalUsings.cs
└── {ServiceName}.Persistence/
    ├── {ServiceName}DbContext.cs
    ├── EntityConfigurations/
    ├── Repositories/
    ├── Migrations/
    └── DependencyInjection.cs
```

### 16.5 Inconsistencies and Variations Noted

| Item | Variation |
|------|-----------|
| DI class name | Most use `DependencyInjection.cs`; ArticleHub, Review, Submission use `DependecyInjection.cs` (typo retained) |
| Application layer | Auth and Journals/Production have a thin/missing Application layer; Review and Submission have a full Application project |
| Domain event handler interface | FastEndpoints services use `IEventHandler<T>.HandleAsync`; MediatR services use `INotificationHandler<T>.Handle` |
| Repository pattern | Journals uses Redis.OM `Repository<T>` not EF; Auth uses named repositories; Review/Submission use both generic `Repository<>` and named subclasses |
| Validator base | Production uses `Validator<T>` (FastEndpoints); others use `AbstractValidator<T>` (FluentValidation directly) |
| Mapster scan | Some services call `AddMapsterConfigsFromCurrentAssembly()`; others use `AddMapsterConfigsFromAssemblyContaining<T>()` |
| `_domainEvents` field naming | `AggregateRoot<T>` uses private `_domainEvents`; `User` (Auth) duplicates the pattern inline since it cannot inherit `AggregateRoot<T>` |
