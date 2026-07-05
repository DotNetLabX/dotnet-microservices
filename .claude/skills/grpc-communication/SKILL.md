---
name: grpc-communication
description: When and how to use synchronous code-first gRPC between services in this repo. Use when a write-path handler needs foreign data (Journal, Person) that may be missing locally, or a business/authorization gate must be authoritative at the instant of a state transition — and, just as important, when to rely on event-replicated local data or the event payload instead of calling gRPC at all. Covers the contract, server, client-registration, and handler-usage phases with lifted template code.
user-invocable: true
---

# gRPC Communication

## Decide first: does this even need gRPC?

Most cross-service data needs do **not**. gRPC is the exception, reserved for two cases; everything else is served from local copies kept fresh by integration events.

Litmus: **gRPC bridges a gap in local data at the exact moment a write needs it, or answers a question that must be authoritative right now.** If you are on a read path, you are almost certainly not using gRPC.

| Situation | Do this | Registry row |
|---|---|---|
| A write-path handler references a foreign entity (`Journal`, `Person`) that may be **missing locally** | gRPC **live**: fetch it, then persist a local copy before proceeding (get-or-create hydration) | datasharing-1 |
| A business / authorization gate must be **true at the instant** of a state transition (e.g. "is this editor assigned to this journal?") | gRPC **live**: call the authoritative owner and throw on failure — before the mutation | datasharing-3 |
| A read path needs foreign **descriptive** data | Read the **local shadow row** that integration-event consumers keep upserted — never gRPC | datasharing-2 |
| A pure **read-model** service (ArticleHub) needs a field | It must already be **in the event payload**. A read model registers **zero** gRPC clients | datasharing-4 |

The two "gRPC live" rows share one principle: only a *now-authoritative* need justifies a synchronous call. Descriptive data that can tolerate eventual freshness is replicated eagerly (datasharing-2), so downstream reads never reach back over the wire.

## The four phases

```
Contract                 ->  Server                    ->  Client registration       ->  Handler usage
Articles.Grpc.Contracts      owning service                consumer's API/DI              consumer's handler
(boundaries-4)               AddCodeFirstGrpc +            AddCodeFirstGrpcClient<T>      datasharing-1 (hydrate)
                             MapGrpcService                (datasharing-5)                datasharing-3 (gate)
```

### Phase 1 — Contract (boundaries-4)

Code-first, defined **once** in `Articles.Grpc.Contracts`, in a folder named for the **owning service** (`Journals/`, `Auth/`). A `[ServiceContract]` interface; `[OperationContract]` methods returning `ValueTask<...>` with a trailing `CallContext context = default`; requests and responses are flat `[ProtoContract]` classes with `[ProtoMember(n)]` on every member. No `.proto` file is hand-written — the attributes are the contract.

Before adding a contract, grep the package for an existing service to extend rather than duplicate:

```bash
rg -n "interface I\w+Service|\[ServiceContract\]" src/BuildingBlocks/Articles.Grpc.Contracts
```

```csharp
// src/BuildingBlocks/Articles.Grpc.Contracts/Journals/JournalContracts.cs
using ProtoBuf;
using ProtoBuf.Grpc;
using System.ServiceModel;

namespace Journals.Grpc;

[ServiceContract]
public interface IJournalService
{
    [OperationContract]
    ValueTask<GetJournalResponse> GetJournalByIdAsync(GetJournalByIdRequest request, CallContext context = default);

    [OperationContract]
    ValueTask<IsEditorAssignedToJournalResponse> IsEditorAssignedToJournalAsync(IsEditorAssignedToJournalRequest request, CallContext context = default);
}

[ProtoContract]
public class GetJournalByIdRequest
{
    [ProtoMember(1)]
    public int JournalId { get; set; } = default!;
}

[ProtoContract]
public class GetJournalResponse
{
    [ProtoMember(1)]
    public JournalInfo Journal { get; set; } = default!;
}

// JournalInfo and the IsEditorAssignedToJournal request/response follow the same
// [ProtoContract] + numbered [ProtoMember(n)] shape. Never reuse a member number.
```

**`[ProtoContract]` mechanics** — protobuf-net serializes by member *number*, not by name, so these are correctness rules, not style:

- **Numbers are unique and sequential** within a type, starting at 1. Once a contract has shipped, a number is permanent — never renumber or reuse one, or old and new peers silently mis-map fields.
- Every member is an auto-property initialized with `= default!` (satisfies nullable analysis), and the class needs a **parameterless constructor** — protobuf-net constructs the object empty, then sets members. The implicit default ctor is enough; don't add a required-args ctor.
- Collections are `List<T>`, **not arrays** — protobuf-net appends into an existing instance.
- A **nested complex type** carried inside a message is itself a `[ProtoContract]` (that is why `JournalInfo` carries the attribute); a plain primitive, string, or enum member needs nothing extra.
- An **enum** that crosses the wire needs `[ProtoContract]` on the enum plus `[ProtoEnum]` on its members.
- Members are required by default; mark a genuinely **optional** one `[ProtoMember(n, IsRequired = false)]` rather than relying on a nullable type alone.

### Phase 2 — Server, in the owning service

Implement the contract interface in the owning service's API, inject that service's repository, and map the domain entity to the contract DTO (Mapster `Adapt`). The service owning the data owns the server — no service serves another's aggregate.

```csharp
// src/Services/Journals/Journals.API/Features/Journals/JournalGrpcService.cs
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

Register the server host in DI, then map the service in `Program.cs`:

```csharp
// Journals.API/DependencyInjection.cs
services.AddCodeFirstGrpc(options =>
{
    options.ResponseCompressionLevel = CompressionLevel.Fastest;
    options.EnableDetailedErrors = true;
});

// Journals.API/Program.cs
app.MapGrpcService<JournalGrpcService>();
```

**The load-or-throw repository method forks by the owner's persistence engine:** the Redis.OM-backed Journals server uses `GetByIdOrThrowAsync` (shown above); an EF-backed owning service's server uses `FindByIdOrThrowAsync` — same load-or-throw intent, different repository base. Either way, return the **nested-DTO envelope** (`GetJournalResponse` wrapping `JournalInfo`) and map with Mapster — never return the domain entity.

One caveat to carry into the client: the server currently lets domain exceptions propagate **raw** — there is no gRPC-status interceptor yet (see the end of this skill), so a client does not receive typed gRPC statuses. Do not assume otherwise.

### Phase 3 — Client registration (datasharing-5)

Register every client through the **one shared helper** `AddCodeFirstGrpcClient<T>` (in `Blocks.AspNetCore/Grpc/GrpcClientRegistrationExtensions.cs`). It is the only place a `GrpcChannel` is created — it owns address resolution, the dev-only self-signed-cert bypass, and Polly retry. **Never new up a channel in a service.** URLs come from config-bound `GrpcServicesOptions`; the second argument is the service key into its `Services` dictionary.

```csharp
// Consumer's API/DependencyInjection.cs
var grpcOptions = config.GetSectionByTypeName<GrpcServicesOptions>();
services.AddCodeFirstGrpcClient<IPersonService>(grpcOptions, "Person");
services.AddCodeFirstGrpcClient<IJournalService>(grpcOptions, "Journal");
```

```jsonc
// Consumer's appsettings.json — the "Url" values are internal Docker ports (services share one network)
"GrpcServicesOptions": {
  "Retry": { "Count": 3, "InitialDelayMs": 2 },
  "Services": {
    "Person":  { "Url": "https://auth-api:8081",     "EnableRetry": true },
    "Journal": { "Url": "https://journals-api:8081", "EnableRetry": true }
  }
}
```

The helper resolves the client as a scoped service, so a handler just takes the **contract interface** as a constructor dependency — no channel, no generated client type.

### Phase 4 — Handler usage

Inject the contract interface (`IJournalService`, `IPersonService`) into the handler. Two shapes, matching the two decision-rule rows.

**Pass the cancellation token.** protobuf-net.Grpc takes it through the contract's trailing `CallContext` parameter; construct one from `new CallOptions(cancellationToken: ct)` (from `Grpc.Core` — it converts to `CallContext` implicitly). The codebase is currently split — 4 of 6 call sites pass it — so standardize on passing it in new handlers.

**Lazy get-or-create hydration (datasharing-1)** — check local first; call gRPC only on a miss, then persist the copy so the next request stays local:

```csharp
// Submission.Application/Features/CreateArticle/CreateArticleCommandHandler.cs
public class CreateArticleCommandHandler(
    SubmissionDbContext _dbContext, Repository<Journal> _journalRepository, IJournalService _journalClient)
    : IRequestHandler<CreateArticleCommand, IdResponse>
{
    public async Task<IdResponse> Handle(CreateArticleCommand command, CancellationToken ct)
    {
        var journal = await _journalRepository.FindByIdAsync(command.JournalId);
        if (journal is null)
            journal = await CreateJournal(command, ct);   // gRPC fires only on a local miss
        // ... use journal, then SaveChangesAsync
    }

    private async Task<Journal> CreateJournal(CreateArticleCommand command, CancellationToken ct)
    {
        var response = await _journalClient.GetJournalByIdAsync(
            new GetJournalByIdRequest { JournalId = command.JournalId },
            new CallOptions(cancellationToken: ct));
        var journal = Journal.Create(response.Journal, command);
        await _journalRepository.AddAsync(journal);
        return journal;
    }
}
```

**Authoritative gate on a state transition (datasharing-3)** — call the owner live and throw before mutating, so the transition can never proceed on stale data:

```csharp
// Submission.Application/Features/ApproveArticle/ApproveArticleCommandHandler.cs
public async Task<IdResponse> Handle(ApproveArticleCommand command, CancellationToken ct)
{
    var article = await _articleRepository.FindByIdOrThrowAsync(command.ArticleId);

    if (!await IsEditorAssignedToJournal(article.JournalId, command.CreatedById, ct))
        throw new BadRequestException($"Editor is not assigned to the article's Journal (Id: {article.JournalId})");

    var editor = await GetOrCreatePersonByUserId(command.CreatedById, command, ct);  // datasharing-1 hydration
    article.Approve(editor, command, _stateMachineFactory);                          // gate proven true before mutation
    await _articleRepository.SaveChangesAsync();
    return new IdResponse(article.Id);
}

private async Task<bool> IsEditorAssignedToJournal(int journalId, int userId, CancellationToken ct)
{
    var response = await _journalClient.IsEditorAssignedToJournalAsync(
        new IsEditorAssignedToJournalRequest { JournalId = journalId, UserId = userId },
        new CallOptions(cancellationToken: ct));
    return response.IsAssigned;
}
```

## The mistake to avoid

Reaching over gRPC on a **read path** for data you could have replicated. The freshness you gain rarely justifies the synchronous coupling — a downstream service that must call upstream to render a read is now down whenever upstream is. Keep foreign descriptive data current with `JournalCreated` / `JournalUpdated` consumers that upsert a local shadow row (datasharing-2), and give a pure read model everything it needs **in the event payload** (datasharing-4). If you find yourself adding a gRPC client to a read-model service, that is the signal you have the wrong tool.

## What this skill does NOT cover

- **The replication side** — building the `JournalCreated` / `JournalUpdated` integration events and their upsert consumers that make gRPC unnecessary on read paths. That is the messaging flow (datasharing-2).
- **Turning a gRPC exception into a proper gRPC status** — the server currently throws domain exceptions straight through (see the `//todo` interceptor note in `JournalGrpcService`).
