---
name: cqrs-patterns
description: CQRS command and query patterns including MediatR interfaces, handler structure, pipeline behaviors, and the load-mutate-save handler pattern. Loaded when implementing commands, queries, or handlers.
user-invocable: false
---

# CQRS Patterns

## Command and query interfaces

**File:** `src/BuildingBlocks/Blocks.MediatR/Abstractions/`

```csharp
public interface ICommand : ICommand<Unit> { }
public interface ICommand<out TResponse> : IRequest<TResponse> { }
public interface IQuery<out TResponse> : IRequest<TResponse> where TResponse : notnull { }
```

## Command record pattern

Article commands extend `ArticleCommandBase<TActionType>` (from `Articles.Abstractions`):

```csharp
public abstract record ArticleCommandBase<TActionType> : IArticleAction<TActionType>
{
    [JsonIgnore] public int ArticleId { get; set; }     // from route parameter
    public string? Comment { get; init; }
    [JsonIgnore] public abstract TActionType ActionType { get; }
    [JsonIgnore] public DateTime CreatedOn => DateTime.UtcNow;
    [JsonIgnore] public int CreatedById { get; set; }   // from JWT claims via pipeline
}
```

Each service defines a convenience base:
```csharp
public abstract record ArticleCommand : ArticleCommandBase<ArticleActionType>, IArticleAction, ICommand<IdResponse>;
```

Concrete commands:
```csharp
public record AcceptArticleCommand : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.AcceptArticle;
}
```

`[JsonIgnore]` fields are populated by: route parameters (ArticleId), `AssignUserIdBehavior` (CreatedById).

## Query record pattern

```csharp
public record GetArticleQuery(int ArticleId) : IQuery<GetArticleResponse>;
public record GetArticleResponse(ArticleDto ArticleSummary);
```

## Handler pattern: load → mutate → save

**Command handler:**
```csharp
public class AcceptArticleCommandHandler(ArticleRepository _articleRepository, ArticleStateMachineFactory _factory)
    : IRequestHandler<AcceptArticleCommand, IdResponse>
{
    public async Task<IdResponse> Handle(AcceptArticleCommand command, CancellationToken ct)
    {
        var article = await _articleRepository.FindByIdOrThrowAsync(command.ArticleId);
        article.Accept(_factory, command);
        await _articleRepository.SaveChangesAsync();
        return new IdResponse(article.Id);
    }
}
```

**Query handler:**
```csharp
public class GetArticleQueryHandler(ArticleRepository _articleRepository)
    : IRequestHandler<GetArticleQuery, GetArticleResponse>
{
    public async Task<GetArticleResponse> Handle(GetArticleQuery query, CancellationToken ct)
    {
        var article = Guard.NotFound(await _articleRepository.GetFullArticleByIdAsync(query.ArticleId));
        return new GetArticleResponse(article.Adapt<ArticleDto>());
    }
}
```

## Pipeline behaviors

Registered in Application layer DI (Submission, Review):

```csharp
config.AddOpenBehavior(typeof(AssignUserIdBehavior<,>));
config.AddOpenBehavior(typeof(ValidationBehavior<,>));
config.AddOpenBehavior(typeof(LoggingBehavior<,>));
```

| Behavior | What it does |
|----------|-------------|
| `AssignUserIdBehavior` | Sets `CreatedById` on `IAuditableAction` from `IClaimsProvider` |
| `ValidationBehavior` | Runs all `IValidator<TRequest>`, throws `ValidationException` on failure |
| `LoggingBehavior` | Logs request/response timing |

## FastEndpoints services (no MediatR)

Auth, Journals, Production put handler logic directly in `HandleAsync`:

```csharp
public class CreateJournalEndpoint(Repository<Journal> _repository)
    : Endpoint<CreateJournalCommand, IdResponse>
{
    public override async Task HandleAsync(CreateJournalCommand command, CancellationToken ct)
    {
        var journal = command.Adapt<Journal>();
        await _repository.AddAsync(journal);
        await SendOkAsync(new IdResponse(journal.Id), ct);
    }
}
```

No separate handler file. No `ISender` dispatch. The endpoint IS the handler.

## Which services use MediatR

| Service | MediatR | Handler location |
|---------|---------|-----------------|
| Submission | Yes | `Submission.Application/Features/` |
| Review | Yes | `Review.Application/Features/` |
| Auth | Partial | Some features use MediatR, others direct in endpoint |
| Journals | No | Logic in endpoint `HandleAsync` |
| Production | No | Logic in endpoint `HandleAsync` |
| ArticleHub | Partial | Mostly direct |
