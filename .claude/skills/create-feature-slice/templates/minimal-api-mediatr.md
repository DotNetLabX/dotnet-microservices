# Variant C — Minimal APIs + MediatR

**Service:** Submission (declares `Endpoint framework: Minimal APIs + MediatR` in its `CLAUDE.md`).

**Defining trait:** the endpoint is a static class with a `Map(this IEndpointRouteBuilder)` extension that dispatches to a MediatR `IRequestHandler` via `ISender` (endpoints-2). The files split across two projects:
- `Submission.Application/Features/{Operation}/` — command/query record, co-located validator, handler. Submission uses a **flat** feature folder (no `{Area}/` nesting).
- `Submission.API/Endpoints/` — the static endpoint class.

**Registration:** the one variant that needs a manual edit. Add a line to `EndpointRegistration.MapAllEndpoints()` (`Submission.API/Endpoints/XEndpointRegistration.cs`) calling your new `Map(api)`. The handler is MediatR-assembly-scanned and the validator is registered by `AddValidatorsFromAssemblyContaining` — both already wired in `Submission.Application/DependencyInjection.cs`. No other DI lines.

The MediatR pipeline `AssignUserId` then `Validation` then `Logging` is registered once in that same file — do not re-register it (appcqrs-2). `AssignUserId` stamps `CreatedById`, so the command never carries it on the wire (appcqrs-1).

---

## C1 — Non-article command with a payload (create)

The service's `ArticleCommand` base (`Submission.Application/Features/_Shared/ArticleCommand.cs`) is `ArticleCommandBase<ArticleActionType>` plus the `ICommand<IdResponse>` marker; audit/provenance is `[JsonIgnore]`d and pipeline-set (appcqrs-1, appcqrs-5). A creation command inherits it, adds its own payload as positional record parameters, and overrides `ActionType`. The validator extends `AbstractValidator<T>` (endpoints-4).

For reference, the shared base already in the service:
```csharp
// Submission.Application/Features/_Shared/ArticleCommand.cs
public abstract record ArticleCommand : ArticleCommandBase<ArticleActionType>, ICommand<IdResponse>;
public abstract record ArticleCommand<TResponse> : ArticleCommandBase<ArticleActionType>, ICommand<TResponse>;

public abstract class ArticleCommandValidator<TFileActionCommand> : AbstractValidator<TFileActionCommand>
    where TFileActionCommand : IArticleAction
{
    public ArticleCommandValidator()
        => RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof(ArticleCommand.ArticleId));
}
```

`Submission.Application/Features/CreateArticle/CreateArticleCommand.cs`
```csharp
namespace Submission.Application.Features.CreateArticle;

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
> `NotEmptyWithMessage` / `MaximumLengthWithMessage` and the `MaxLength.C*` constants come from `Blocks.Core` — use them, not raw string literals or numeric lengths (conventions-4).

`Submission.Application/Features/CreateArticle/CreateArticleCommandHandler.cs`
```csharp
namespace Submission.Application.Features.CreateArticle;

public class CreateArticleCommandHandler(Repository<Journal> _journalRepository)
    : IRequestHandler<CreateArticleCommand, IdResponse>
{
    public async Task<IdResponse> Handle(CreateArticleCommand command, CancellationToken ct)
    {
        var journal = Guard.NotFound(await _journalRepository.FindByIdAsync(command.JournalId));

        var article = journal.CreateArticle(command.Title, command.Type, command.Scope, command);  // existing domain behavior — action is last

        await _journalRepository.SaveChangesAsync(ct);

        return new IdResponse(article.Id);
    }
}
```

`Submission.API/Endpoints/CreateArticleEndpoint.cs`
```csharp
using Submission.Application.Features.CreateArticle;

namespace Submission.API.Endpoints;

public static class CreateArticleEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/articles", async (CreateArticleCommand command, ISender sender) =>   // plain CRUD → no :verb (namingx-4)
        {
            var response = await sender.Send(command);
            return Results.Created($"/api/articles/{response.Id}", response);
        })
        .RequireRoleAuthorization(Role.Author)             // two-layer role + resource gate (security-2)
        .WithName("CreateArticle")
        .WithTags("Articles")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
```

Then register it (the only manual wiring step) in `Submission.API/Endpoints/XEndpointRegistration.cs`:
```csharp
public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
{
    var api = app.MapGroup("/api");
    // ... existing lines ...
    CreateArticleEndpoint.Map(api);      // add this line
    return app;
}
```

---

## C2 — Article-scoped action with a route id

A bare article action inherits `ArticleCommand`, needs no payload, and its validator is just `ArticleCommandValidator<T>`. The endpoint binds `{articleId}` into the command with a record `with` expression.

```csharp
// Submission.Application/Features/ApproveArticle/ApproveArticleCommand.cs
public record ApproveArticleCommand : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.ApproveDraft;
}
public class ApproveArticleCommandValidator : ArticleCommandValidator<ApproveArticleCommand>;

// Submission.API/Endpoints/ApproveArticleEndpoint.cs
public static class ApproveArticleEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{articleId:int}:approve", async (int articleId, ApproveArticleCommand command, ISender sender) =>
        {
            var response = await sender.Send(command with { ArticleId = articleId });   // route-id binding via with-expression
            return Results.Ok(response);
        })
        .RequireRoleAuthorization(Role.Editor, Role.EditorAdmin)
        .WithName("ApproveArticle")
        .WithTags("Articles")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
```

---

## C3 — Query

A query carries the `IQuery<TResponse>` marker (appcqrs-5) and binds route/query-string values with `[AsParameters]`.

```csharp
// Submission.Application/Features/GetArticle/GetArticleQuery.cs
public record GetArticleQuery(int ArticleId) : IQuery<GetArticleResponse>;
public record GetArticleResponse(ArticleDto ArticleSummary);

public class GetArticleValidator : AbstractValidator<GetArticleQuery>
{
    public GetArticleValidator()
        => RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof(GetArticleQuery.ArticleId));
}

// Submission.API/Endpoints/GetArticleEndpoint.cs
public static class GetArticleEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId:int}", async ([AsParameters] GetArticleQuery query, ISender sender) =>
        {
            var article = await sender.Send(query);
            return Results.Ok(article);
        })
        .RequireRoleAuthorization(Role.Author, Role.Editor, Role.EditorAdmin)
        .WithName("GetArticle")
        .WithTags("Articles")
        .Produces<GetArticleResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
```

## C4 — File upload (multipart)

An upload command carries an `IFormFile`, so the endpoint binds it `[FromForm]` and must call `.DisableAntiforgery()`. Submission keeps a shared `UploadFileCommand : ArticleCommand` base (adds `AssetType` + `IFormFile File`, overrides `ActionType`) and a `UploadFileValidator<T>` base that resolves the asset-type definition and checks size/extension; a concrete upload is a thin subclass over them.

```csharp
// Submission.Application/Features/UploadFiles/_Shared/UploadFileCommand.cs (shared base — already exists)
public abstract record UploadFileCommand : ArticleCommand
{
    [Required] public AssetType AssetType { get; init; }
    [Required] public IFormFile File { get; init; } = null!;
    public override ArticleActionType ActionType => ArticleActionType.UploadAsset;
}

// Submission.Application/Features/UploadFiles/UploadManuscriptFile/UploadManuscriptFileCommand.cs
public record UploadManuscriptFileCommand : UploadFileCommand;

public class UploadManuscriptCommandValidator : UploadFileValidator<UploadManuscriptFileCommand>
{
    public UploadManuscriptCommandValidator(AssetTypeRepository assetTypeRepository) : base(assetTypeRepository) { }
    public override IReadOnlyCollection<AssetType> AllowedAssetTypes => AssetTypeCategories.ManuscriptAsset;
}

// Submission.API/Endpoints/UploadManuscriptFileEndpoint.cs
public static class UploadManuscriptFileEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/{articleId:int}/assets/manuscript:upload",
            async ([FromRoute] int articleId, [FromForm] UploadManuscriptFileCommand command, ISender sender) =>
        {
            var response = await sender.Send(command with { ArticleId = articleId });
            return Results.Created($"/api/articles/{articleId}/assets/{response.Id}:download", response);
        })
        .RequireRoleAuthorization(Role.Author)
        .WithName("UploadManuscript")
        .WithTags("Assets")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .DisableAntiforgery();       // required because the command binds an IFormFile
    }
}
```

`[FromRoute]`/`[FromForm]` need `using Microsoft.AspNetCore.Mvc;`. The handler wraps the file write in try/catch with a compensating `TryDeleteAsync` on failure — that is the `file-storage-patterns` skill; this slice shows only the endpoint + command shape.

## Checklist (Variant C)

- [ ] Command/validator/handler in `Submission.Application/Features/{Operation}/` (flat); endpoint in `Submission.API/Endpoints/` (endpoints-2, endpoints-3).
- [ ] Article commands inherit `ArticleCommand`, override `ActionType`, add no provenance wire fields (appcqrs-1); use `MaxLength.C*` and the `Blocks.Core` FluentValidation helpers (conventions-4).
- [ ] Validator extends `AbstractValidator<T>` or `ArticleCommandValidator<T>` (endpoints-4).
- [ ] Handler is `IRequestHandler<TCommand, TResponse>`; no role check inside it (endpoints-5). No handler DI edit.
- [ ] Static `Map` dispatches via `ISender`; binds `{articleId}` with `command with { ArticleId = articleId }`.
- [ ] **Registered** one line in `EndpointRegistration.MapAllEndpoints()` — the only manual wiring in this variant.
- [ ] `.RequireRoleAuthorization(...)` for writes; `:verb` for non-CRUD (namingx-4, security-2).
