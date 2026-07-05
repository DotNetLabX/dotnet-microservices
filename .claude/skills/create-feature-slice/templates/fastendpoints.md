# Variant A — FastEndpoints (no MediatR)

**Services:** Journals, Production, Auth (each declares `Endpoint framework: FastEndpoints (no MediatR)` in its `CLAUDE.md`).

**Defining trait:** there is no MediatR request dispatch, so there is **no handler file** — the handler logic lives in the endpoint's `HandleAsync` (endpoints-2). Everything for the feature is colocated in the API project: `{Service}.API/Features/{Area}/{Operation}/` holds the endpoint + command + validator (endpoints-3).

**Registration:** none. Endpoints are attribute-routed (`[HttpPost]`, `[HttpGet]`, …) and auto-discovered by `UseCustomFastEndpoints()` in `Program.cs`. Validators extending `Validator<T>` / `BaseValidator<T>` are assembly-scanned by FastEndpoints. You add no DI lines.

**Routing style:** this repo configures FastEndpoints endpoints with attributes (`[HttpPost(...)]`, `[Authorize]`, `[Tags]`) — the default here. FastEndpoints also supports a `Configure()` method override that sets route, verbs, and policies in code instead of attributes; it is functionally equivalent, but do not introduce it — stay attribute-based to match every existing endpoint in the repo.

---

## A1 — Non-article command (plain CRUD)

Example: create a journal (Journals). Command + validator in one file; the validator base is FastEndpoints' `Validator<T>` (endpoints-4). A plain CRUD request record carries no CQRS marker.

`{Service}.API/Features/{Area}/Create/CreateJournalCommand.cs`
```csharp
using FastEndpoints;
using FluentValidation;

namespace Journals.API.Features.Journals.Create;

public record CreateJournalCommand(string Abbreviation, string Name, string Description, string ISSN, int ChiefEditorId)
{
    public string NormalizedName => Name.ToLowerInvariant();
}

public class CreateJournalCommandValidator : Validator<CreateJournalCommand>
{
    public CreateJournalCommandValidator()
    {
        RuleFor(r => r.Abbreviation).NotEmpty();
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.ISSN).NotEmpty().Matches(@"\d{4}-\d{3}[\dX]").WithMessage("Invalid ISSN format");
        RuleFor(r => r.Description).NotEmpty();
        RuleFor(r => r.ChiefEditorId).GreaterThan(0);
    }
}
```

`{Service}.API/Features/{Area}/Create/CreateJournalEndpoint.cs`
```csharp
using Microsoft.AspNetCore.Authorization;
using Articles.Security;
using Journals.Domain.Journals.Events;

namespace Journals.API.Features.Journals.Create;

[Authorize(Roles = Role.EditorAdmin)]      // declarative gate (endpoints-5)
[HttpPost("journals")]                      // plain CRUD → no :verb suffix (namingx-4)
[Tags("Journals")]
public class CreateJournalEndpoint(Repository<Journal> _journalRepository)
    : Endpoint<CreateJournalCommand, IdResponse>
{
    public override async Task HandleAsync(CreateJournalCommand command, CancellationToken ct)
    {
        // handler logic lives here — no separate handler file (endpoints-2)
        if (_journalRepository.Collection.Any(j => j.Abbreviation == command.Abbreviation || j.NormalizedName == command.NormalizedName))
            throw new BadRequestException("Journal with the same name or abbreviation already exists");

        var journal = command.Adapt<Journal>();
        await _journalRepository.AddAsync(journal);

        await PublishAsync(new JournalCreated(journal));   // domain event via FastEndpoints publisher

        await Send.OkAsync(new IdResponse(journal.Id));
    }
}
```

---

## A2 — Article-scoped action (Production)

Article writes inherit the service's `ArticleCommand` base (`Production.API/Features/_Shared/ArticleCommand.cs`), so audit and provenance are pipeline-computed and `[JsonIgnore]`d (appcqrs-1). Override `ActionType`. The validator extends the service's `ArticleCommandValidator<T>` base (which itself extends the project-local `BaseValidator<T>`, endpoints-4). The endpoint extends the service's `BaseEndpoint<TCommand, TResponse>` helper.

For reference, the shared bases already in the service:
```csharp
// Production.API/Features/_Shared/ArticleCommand.cs
public abstract record ArticleCommand : ArticleCommandBase<ArticleActionType>, IArticleAction, ICommand<IdResponse>;

public abstract class ArticleCommandValidator<TCommand> : BaseValidator<TCommand>
    where TCommand : Articles.Abstractions.IArticleAction
{
    public ArticleCommandValidator()
    {
        RuleFor(command => command.ArticleId).GreaterThan(ValidatorsConstants.Id)
            .WithMessage(ValidatorsMessagesConstants.InvalidId.FormatWith("articleId"));
    }
}

// Production.API/Features/_Shared/BaseEndpoint.cs
public abstract class BaseEndpoint<TCommand, TResponse> : Endpoint<TCommand, TResponse>
    where TCommand : Articles.Abstractions.IArticleAction
{
    protected readonly ArticleRepository _articleRepository;
    protected Article _article;
    public BaseEndpoint(ArticleRepository articleRepository) => _articleRepository = articleRepository;
}
```

`Production.API/Features/Articles/AssignTypesetter/AssignTypesetterCommand.cs`
```csharp
using FluentValidation;
using Production.API.Features.Shared;
using Production.Domain.Shared.Enums;

namespace Production.API.Features.Articles.AssignTypesetter;

public record AssignTypesetterCommand : ArticleCommand
{
    public int TypesetterId { get; init; }
    public override ArticleActionType ActionType => ArticleActionType.AssignTypesetter;
}

public class AssignTypesetterCommandValidator : ArticleCommandValidator<AssignTypesetterCommand>
{
    public AssignTypesetterCommandValidator()
    {
        // ArticleId > 0 is already enforced by the ArticleCommandValidator<T> base — don't repeat it here
        RuleFor(r => r.TypesetterId).GreaterThan(0);
    }
}
```

`Production.API/Features/Articles/AssignTypesetter/AssignTypesetterEndpoint.cs`
```csharp
using Production.Persistence.Repositories;
using Production.API.Features.Shared;
using Production.Domain.StateMachines;
using Production.Persistence;

namespace Production.API.Features.Articles.AssignTypesetter;

[Authorize(Roles = Role.ProdAdmin)]        // role-gated write (endpoints-5, security-2)
[HttpPut("articles/{articleId:int}/typesetter/{typesetterId:int}")]
[Tags("Articles")]
public class AssignTypesetterEndpoint(ArticleRepository articleRepository, ArticleStateMachineFactory _stateMachineFactory, ProductionDbContext _dbContext)
    : BaseEndpoint<AssignTypesetterCommand, IdResponse>(articleRepository)
{
    public override async Task HandleAsync(AssignTypesetterCommand command, CancellationToken ct)
    {
        _article = await _articleRepository.GetByIdOrThrowAsync(command.ArticleId);
        var typesetter = await _dbContext.Typesetters.FindByIdOrThrowAsync(command.TypesetterId);

        _article.AssignTypesetter(typesetter, _stateMachineFactory, command);   // existing domain behavior — action is last

        await _articleRepository.SaveChangesAsync();

        await Send.OkAsync(new IdResponse(command.ArticleId));
    }
}
```

> For a non-CRUD article action without an extra route id, put the RPC verb on the attribute route, for example `[HttpPost("articles/{articleId:int}:approve")]` (namingx-4).

---

## A3 — Query

Queries are plain records with no CQRS marker; the endpoint extends `Endpoint<TQuery, TResponse>`. A read that only needs authentication uses a bare `[Authorize]` (security-2).

```csharp
// Query + validator
public record GetJournalQuery(int JournalId);
public record GetJournalResponse(JournalDto Journal);

public class GetJournalQueryValidator : Validator<GetJournalQuery>
{
    public GetJournalQueryValidator() => RuleFor(r => r.JournalId).GreaterThan(0);
}

// Endpoint
[Authorize]                                 // authentication only, no resource gate (security-2)
[HttpGet("journals/{journalId:int}")]
[Tags("Journals")]
public class GetJournalEndpoint(JournalDbContext _dbContext)
    : Endpoint<GetJournalQuery, GetJournalResponse>
{
    public override async Task HandleAsync(GetJournalQuery query, CancellationToken ct)
    {
        var journal = await _dbContext.Journals.GetByIdOrThrowAsync(query.JournalId);
        await Send.OkAsync(new GetJournalResponse(journal.Adapt<JournalDto>()));
    }
}
```

## A4 — File upload

FastEndpoints handles multipart differently from the MediatR variants: there is no `[FromForm]` or `.DisableAntiforgery()`. Production routes every asset upload through a shared generic base — `_Shared/UploadFileEndpoint<TUploadCommand>` — which owns the upload plus the compensating `TryDeleteAsync`; each concrete endpoint sets its stage and decorates the class with `[AllowFileUploads]` (FastEndpoints' multipart opt-in):

```csharp
[Authorize(Roles = Role.Typesetter)]
[HttpPost("articles/{articleId:int}/assets/draft/files:upload")]   // non-CRUD action → :upload (namingx-4)
[AllowFileUploads]                                                  // FE multipart opt-in — not DisableAntiforgery
[Tags("Files")]
public class UploadDraftFileEndpoint(ArticleRepository articleRepository, AssetTypeRepository assetTypeRepository, IFileService fileService, AssetStateMachineFactory factory)
    : UploadFileEndpoint<UploadDraftFileCommand>(articleRepository, assetTypeRepository, fileService, factory)
{
    protected override ArticleStage NextStage => ArticleStage.DraftProduction;
}
```

The upload write + compensating `TryDeleteAsync` in the base is the `file-storage-patterns` skill's territory; this slice shows only the endpoint shape.

## Checklist (Variant A)

- [ ] Folder `{Service}.API/Features/{Area}/{Operation}/` created (endpoints-3).
- [ ] Command/query record; article writes inherit `ArticleCommand`, override `ActionType`, add no provenance wire fields (appcqrs-1).
- [ ] Validator in the same file extends `Validator<T>` / `BaseValidator<T>` / `ArticleCommandValidator<T>` (endpoints-4).
- [ ] Logic in `HandleAsync` — no handler file (endpoints-2); no role check inside it (endpoints-5).
- [ ] Attribute route; `:verb` for non-CRUD (namingx-4). No manual registration.
- [ ] `[Authorize(Roles = ...)]` for writes / `[Authorize]` for reads (endpoints-5, security-2).
