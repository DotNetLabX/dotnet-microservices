# Variant B — Carter + MediatR

**Service:** Review (declares `Endpoint framework: Carter + MediatR` in its `CLAUDE.md`). ArticleHub also uses Carter but is a read-only projection with no commands — for a read-only aggregate endpoint use a bare `.RequireAuthorization()` (security-2).

**Defining trait:** the endpoint is a thin `ICarterModule` shim that dispatches to a MediatR `IRequestHandler` via `ISender` (endpoints-2). The files split across two projects:
- `Review.Application/Features/{Area}/{Operation}/` — command/query record, co-located validator, handler.
- `Review.API/Endpoints/{Area}/` — the `ICarterModule` endpoint.

**Registration:** none. Carter modules are auto-discovered; `Program.cs` already calls `app.MapGroup("/api").MapCarter()`. The handler is MediatR-assembly-scanned and the validator is registered by `AddValidatorsFromAssemblyContaining` — both already wired in `Review.Application/DependencyInjection.cs`. You add no DI lines.

The MediatR pipeline `AssignUserId` then `Validation` then `Logging` is registered once in that same file — do not re-register it (appcqrs-2). `AssignUserId` stamps `CreatedById`, so the command never carries it on the wire (appcqrs-1).

---

## B1 — Article-scoped action (the common case)

Article writes inherit the service's `ArticleCommand` base (`Review.Application/Features/Articles/_Shared/ArticleCommand.cs`). It is `ArticleCommandBase<ArticleActionType>` plus the `ICommand<IdResponse>` marker, so all audit/provenance is `[JsonIgnore]`d and pipeline-set — only `Comment` and your own payload fields are wire-settable (appcqrs-1, appcqrs-5). Override `ActionType`. The validator extends `ArticleCommandValidator<T>`, which extends `AbstractValidator<T>` (endpoints-4).

For reference, the shared bases already in the service:
```csharp
// Review.Application/Features/Articles/_Shared/ArticleCommand.cs
public abstract record ArticleCommand : ArticleCommandBase<ArticleActionType>, IArticleAction, ICommand<IdResponse>;
public abstract record ArticleCommand<TResponse> : ArticleCommandBase<ArticleActionType>, IArticleAction, ICommand<TResponse>;

public abstract class ArticleCommandValidator<TFileActionCommand> : AbstractValidator<TFileActionCommand>
    where TFileActionCommand : IArticleAction
{
    public ArticleCommandValidator()
        => RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof(ArticleCommand.ArticleId));
}
```

`Review.Application/Features/Articles/AcceptArticle/AcceptArticleCommand.cs`
```csharp
using Review.Application.Features.Articles.Shared;
using Review.Domain.Shared.Enums;

namespace Review.Application.Features.Articles.AcceptArticle;

public record AcceptArticleCommand : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.AcceptArticle;
}

public class AcceptArticleCommandValidator : ArticleCommandValidator<AcceptArticleCommand>;
```

> A command with an extra payload field just declares it as an `init` property, for example `public int TypesetterId { get; init; }`, then adds a rule in a bodied validator (see Variant A2).

`Review.Application/Features/Articles/AcceptArticle/AcceptArticleCommandHandler.cs`
```csharp
namespace Review.Application.Features.Articles.AcceptArticle;

public class AcceptArticleCommandHandler(ArticleRepository _articleRepository, ArticleStateMachineFactory _stateMachineFactory)
    : IRequestHandler<AcceptArticleCommand, IdResponse>
{
    public async Task<IdResponse> Handle(AcceptArticleCommand command, CancellationToken cancellationToken)
    {
        var article = await _articleRepository.FindByIdOrThrowAsync(command.ArticleId);

        article.Accept(_stateMachineFactory, command);   // existing domain behavior — action is last; no role checks here (endpoints-5)

        await _articleRepository.SaveChangesAsync();

        return new IdResponse(article.Id);
    }
}
```

`Review.API/Endpoints/Articles/AcceptArticleEndpoint.cs`
```csharp
using Review.Application.Features.Articles.AcceptArticle;

namespace Review.API.Endpoints.Articles;

public class AcceptArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{articleId:int}:accept", async (int articleId, AcceptArticleCommand command, ISender sender) =>
        {
            command.ArticleId = articleId;                 // route-id binding (Carter mutates the command)
            var response = await sender.Send(command);
            return Results.Ok(response);
        })
        .RequireRoleAuthorization(Role.Editor, Role.EditorAdmin)   // two-layer role + resource gate (security-2)
        .WithName("AcceptArticle")
        .WithTags("Articles")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
```

Note the `:accept` RPC suffix — a non-CRUD action (namingx-4). Plain CRUD (create/get) drops it.

---

## B2 — Query

A query is dispatched through `ISender` exactly like a command; the record carries the `IQuery<TResponse>` marker (appcqrs-5).

```csharp
// Review.Application/Features/Articles/GetArticle/GetArticleQuery.cs
public record GetArticleQuery(int ArticleId) : IQuery<GetArticleResponse>;
public record GetArticleResponse(ArticleDto Article);

public class GetArticleValidator : AbstractValidator<GetArticleQuery>
{
    public GetArticleValidator()
        => RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof(GetArticleQuery.ArticleId));
}

// Review.API/Endpoints/Articles/GetArticleEndpoint.cs
public class GetArticleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId:int}", async (int articleId, ISender sender) =>
        {
            var response = await sender.Send(new GetArticleQuery(articleId));
            return Results.Ok(response);
        })
        .RequireRoleAuthorization(Role.Editor, Role.EditorAdmin)
        .WithName("GetReviewArticle")
        .WithTags("Articles")
        .Produces<GetArticleResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
```

## B3 — File upload (multipart)

An upload command carries an `IFormFile`, so the Carter endpoint binds it `[FromForm]` and must call `.DisableAntiforgery()`. Review keeps a shared `UploadFileCommand : ArticleCommand` base (adds `AssetType` + `IFormFile File`) and a `UploadFileValidator<T>` base; the concrete command overrides `ActionType`.

```csharp
// Review.Application/Features/Assets/UploadFiles/UploadReviewReport/UploadReviewReportCommand.cs
public record UploadReviewReportCommand : UploadFileCommand
{
    public override ArticleActionType ActionType => ArticleActionType.UploadReviewReport;
}

// Review.API/Endpoints/Assets/UploadReviewReportEndpoint.cs
public class UploadReviewReportEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{articleId:int}/assets/review-reports:upload",
            async ([FromRoute] int articleId, [FromForm] UploadReviewReportCommand command, ISender sender) =>
        {
            command.ArticleId = articleId;
            var response = await sender.Send(command);
            return Results.Created($"/api/articles/{command.ArticleId}/assets/{response.Id}:download", response);
        })
        .RequireRoleAuthorization(Role.Reviewer)
        .WithName("UploadReviewReport")
        .WithTags("Assets")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .DisableAntiforgery();       // required because the command binds an IFormFile
    }
}
```

`[FromRoute]`/`[FromForm]` need `using Microsoft.AspNetCore.Mvc;`. The `review-reports:upload` route is a non-CRUD action (namingx-4). The handler's compensating `TryDeleteAsync` on failure is the `file-storage-patterns` skill; this slice shows only the endpoint + command shape.

## Checklist (Variant B)

- [ ] Command/validator/handler in `Review.Application/Features/{Area}/{Operation}/`; endpoint in `Review.API/Endpoints/{Area}/` (endpoints-2, endpoints-3).
- [ ] Article writes inherit `ArticleCommand`, override `ActionType`, add no provenance wire fields (appcqrs-1).
- [ ] Validator extends `AbstractValidator<T>` or `ArticleCommandValidator<T>` (endpoints-4).
- [ ] Handler is `IRequestHandler<TCommand, TResponse>`; no role check inside it (endpoints-5). No DI edit — assembly-scanned.
- [ ] `ICarterModule` shim dispatches via `ISender`; binds `{articleId}` with `command.ArticleId = articleId`. No manual registration (`MapCarter` covers it).
- [ ] `.RequireRoleAuthorization(...)` for writes; bare `.RequireAuthorization()` for a read-only aggregate endpoint (security-2). `:verb` for non-CRUD (namingx-4).
