---
name: authorization-security
description: Two-layer authorization for the article lifecycle — the closed UserRoleType role vocabulary, endpoint role+resource gates, per-service resource access checks, framework-specific identity stamping, and JWT validation. Use when gating an endpoint by role, adding a resource access check, stamping the acting user onto a command, configuring JWT authentication, or adding a new role.
---

# Authorization & Security

Authorization here is **two gates stacked at the endpoint boundary, plus a shared identity contract** — never a role check inside a handler or domain body (endpoints-5: zero `IsInRole` in business code). A role-gated article endpoint passes a **role gate** (is the caller one of these roles?) and a **resource gate** (does this caller have access to *this* article?). Read-only aggregate views take authentication only. The acting user is stamped onto the command by a framework-specific hook, and every service validates the same JWT the same way.

## Binding rules

These are not style preferences — code that violates them is wrong here.

1. **No role check in business code.** The gate is declarative at the endpoint; a handler or domain method never inspects the caller's roles (endpoints-5).
2. **Never bare `RequireRole`.** Role-gated article endpoints go through `RequireRoleAuthorization`, which stacks both layers. Bare `RequireRole` never appears outside that extension (security-2).
3. **Resource access is answered from local data only.** `IArticleAccessChecker` reads the service's own `ArticleActors` table — never a synchronous cross-service query (security-3).
4. **Provenance is stamped, never trusted.** `CreatedById` is set by the framework hook from the token, never read from the request body (security-4, appcqrs-1).

## Phase 1 — Role vocabulary (security-1)

Roles are one closed vocabulary shared by every service: the `UserRoleType : int` enum in `Articles.Abstractions`, surfaced as string constants by `Articles.Security.Role`. The integer values are **range-partitioned by domain** so each service — and each future service — owns a numeric block. The gaps are deliberate reserved namespace, not oversights.

```csharp
// Articles.Abstractions/Enums/UserRoleType.cs
public enum UserRoleType : int
{
    // Cross-domain: 1-9
    [Description("Editorial Office Admin")]
    EOF = 1,

    // Submission: 11-19
    [Description("Author")]
    AUT = 11,
    [Description("Corresponding Author")]
    CORAUT = 12,

    // Review: 21-29
    [Description("Review Editor")]
    REVED = 21,
    [Description("Reviewer")]
    REV = 22,

    // Production: 31-39
    [Description("Production Office Admin")]
    POF = 31,
    [Description("Typesetter")]
    TSOF = 32,

    // Auth-only: 91-99
    [Description("User Admin")]
    USERADMIN = 91
}
```

The string constants exist so endpoints and policies name roles without touching the enum. Each is `nameof(UserRoleType.X)` — the constant and the enum member can never drift.

```csharp
// Articles.Security/Role.cs
public static class Role
{
    public const string UserAdmin = nameof(UserRoleType.USERADMIN);

    public const string EditorAdmin = nameof(UserRoleType.EOF);
    public const string Author = nameof(UserRoleType.AUT);

    public const string Editor = nameof(UserRoleType.REVED);
    public const string Reviewer = nameof(UserRoleType.REV);

    public const string ProdAdmin = nameof(UserRoleType.POF);
    public const string Typesetter = nameof(UserRoleType.TSOF);
}
```

**To add a role — three edits, in this order (security-1):**

1. Add the enum member to `UserRoleType`, **inside its domain's numeric range** (a new review role goes in 21-29; a new service claims the next free block). Never reuse a number; the range is namespace.
2. Add the matching `Role.` string constant as `nameof(UserRoleType.NewMember)` — never a hand-typed literal.
3. Use the constant (or the enum member) at the endpoint gate (Phase 2). Do not scatter the role name as a string anywhere else.

## Phase 2 — Endpoint gate (security-2)

Role-gated article endpoints get **both** layers through the single `RequireRoleAuthorization` extension. It adds the ASP.NET role check *and* registers the `ArticleRoleRequirement` that drives the resource gate in Phase 3 — so one call wires both. Two overloads, one for the string constants and one for the enum members:

```csharp
// Articles.Security/Extensions.cs
public static class Extensions
{
    public static TBuilder RequireRoleAuthorization<TBuilder>(this TBuilder builder, params string[] roles)
        where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(policy =>
        {
            policy.RequireRole(roles);
            policy.Requirements.Add(new ArticleRoleRequirement(roles));
        });

    public static TBuilder RequireRoleAuthorization<TBuilder>(this TBuilder builder, params UserRoleType[] roles)
        where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(policy =>
        {
            policy.RequireRole(roles.Select(r => r.ToString()));
            policy.Requirements.Add(new ArticleRoleRequirement(roles));
        });
}
```

`ArticleRoleRequirement` is the marker the resource-gate handler binds to — it just carries the allowed role set as a `IReadOnlySet` of `UserRoleType`:

```csharp
// Articles.Security/ArticleRoleRequirement.cs
public class ArticleRoleRequirement : IAuthorizationRequirement
{
    public IReadOnlySet<UserRoleType> AllowedRoles { get; }

    public ArticleRoleRequirement(IEnumerable<string> allowedRoles)
        => AllowedRoles = allowedRoles.Select(r => r.ToEnum<UserRoleType>()).ToHashSet();

    public ArticleRoleRequirement(IEnumerable<UserRoleType> allowedRoles)
        => AllowedRoles = allowedRoles.ToHashSet();
}
```

Apply it at the endpoint — either spelling works, matching the overload:

```csharp
// role-gated write endpoint — role gate + resource gate
group.MapPost("/{articleId:int}:approve", ApproveArticle)
     .RequireRoleAuthorization(Role.EditorAdmin, Role.Editor);

// or with the enum members
articleGroup.RequireRoleAuthorization(UserRoleType.EOF, UserRoleType.REVED);
```

**The sanctioned single-layer exception (security-2):** ArticleHub's read-only aggregate views — `SearchArticles`, `GetTimeline`, `GetArticle` — use bare `.RequireAuthorization()`. Any authenticated user, no resource check, because they expose the cross-service read model rather than a write on one article. The rule: **writes get role + resource; aggregate reads get authentication only.**

```csharp
// ArticleHub read-only view — authentication only, no role or resource gate
searchGroup.MapGet("/", SearchArticles)
           .RequireAuthorization(); // allows all authenticated users
```

## Phase 3 — Resource check (security-3)

The resource gate answers "does *this* caller reach *this* article?" It is `IArticleAccessChecker.HasAccessAsync`, implemented **once per write-side service** against that service's *own* `ArticleActors` table — local data replicated by integration events, **never** a synchronous cross-service query.

The `ArticleRoleRequirement` from Phase 2 is handled by `ArticleAccessAuthorizationHandler`. It narrows the caller's token roles to the requirement's allowed set, then — only if at least one survives — asks the checker. This handler lives once, in `Articles.Security`, and is registered by every service that gates article endpoints:

```csharp
// Articles.Security/ArticleAccessAuthorizationHandler.cs
public class ArticleAccessAuthorizationHandler(HttpContextProvider _httpProvider, IArticleAccessChecker _articleRoleChecker)
    : AuthorizationHandler<ArticleRoleRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ArticleRoleRequirement requirement)
    {
        var userRoles = _httpProvider.GetUserRoles<UserRoleType>()
                            .Where(requirement.AllowedRoles.Contains)
                            .ToHashSet();

        if (userRoles.Count > 0
            && await HasUserRoleForArticle(userRoles))
        {
            context.Succeed(requirement);
        }
    }

    private async Task<bool> HasUserRoleForArticle(IReadOnlySet<UserRoleType> userRoles)
        => await _articleRoleChecker.HasAccessAsync(
            _httpProvider.GetArticleId(), _httpProvider.GetUserId(), userRoles,
            _httpProvider.HttpContext.RequestAborted);
}
```

Each service implements the checker with the **same decision tree** and a **service-specific admin bypass** — the bypass role set is *not* uniform across services. The tree:

1. `articleId` is null → **true** (the endpoint is not article-specific, so there is nothing to gate).
2. `userId` is null or the role set is empty → **false**.
3. Caller holds an **admin bypass role for this service** → **true** (admins reach every article).
4. Otherwise → is the caller an actor on this article? — the `ArticleActors` lookup by `(articleId, userId)`.

Here is the Submission implementation verbatim; the other services differ only in the `DbContext` type and the bypass line:

```csharp
// Submission.Application/ArticleAccessChecker.cs
public class ArticleAccessChecker(SubmissionDbContext _dbContext) : IArticleAccessChecker
{
    public async Task<bool> HasAccessAsync(int? articleId, int? userId, IReadOnlySet<UserRoleType> roles, CancellationToken ct = default)
    {
        if (articleId is null) // the endpoint is not article specific
            return true;

        if (userId is null || roles.IsNullOrEmpty())
            return false;

        if (roles.Overlaps([UserRoleType.EOF, UserRoleType.REVED])) // admin bypass — set per service
            return true;

        return await _dbContext.ArticleActors
            .AsNoTracking()
            .AnyAsync(e => e.ArticleId == articleId && e.Person.UserId == userId, ct);
    }
}
```

The admin bypass is the one line that changes per service (security-3):

| Service | Admin bypass line |
|---|---|
| Submission | `roles.Overlaps([UserRoleType.EOF, UserRoleType.REVED])` |
| Review | `roles.Contains(UserRoleType.EOF)` |
| Production | `roles.Contains(UserRoleType.POF)` |

When you add the checker to a new write-side service: copy this skeleton into `{Svc}.Application`, swap `SubmissionDbContext` for the service's context, set the bypass to the role(s) that own every article in that service, and register `IArticleAccessChecker` scoped in the service DI.

## Phase 4 — Identity stamping — pick by framework (security-4)

The acting user's id is stamped onto every command that implements `IAuditableAction`, as `CreatedById`. Provenance is pipeline-computed, never client-trusted — `ArticleCommandBase` even `[JsonIgnore]`s the field so the wire can't set it (appcqrs-1).

The stamp is reimplemented **once per endpoint framework**, each hooking the one shared `IClaimsProvider`. Register the variant that matches how the service dispatches its requests:

| Dispatch / framework | Hook | Register as |
|---|---|---|
| MediatR pipeline (Submission, Review) | `AssignUserIdBehavior` | **first** open pipeline behavior — before Validation, before Logging (appcqrs-2) |
| Minimal API, no MediatR | `AssignUserIdFilter` | endpoint filter |
| FastEndpoints (Production) | `AssignUserIdPreProcessor` | global pre-processor |

**MediatR** — stamps inside the pipeline, ahead of validation so downstream behaviors see the id:

```csharp
// Blocks.MediatR/Behaviors/AssignUserIdBehavior.cs
public class AssignUserIdBehavior<TRequest, TResponse>(IClaimsProvider _claimsProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuditableAction
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is IAuditableAction action)
        {
            var userId = _claimsProvider.TryGetUserId();
            if (userId is not null)
                action.CreatedById = userId.Value;
        }

        return await next();
    }
}
```

**Minimal API** — an endpoint filter that scans the bound arguments:

```csharp
// Blocks.AspNetCore/Filters/AssignUserIdFilter.cs
public sealed class AssignUserIdFilter(IClaimsProvider _claimsProvider) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        foreach (var arg in ctx.Arguments)
        {
            if (arg is IAuditableAction action && action.CreatedById == default)
            {
                var userId = _claimsProvider.TryGetUserId();
                if (userId is not null)
                    action.CreatedById = userId.Value;
            }
        }
        return await next(ctx);
    }
}
```

**FastEndpoints** — a global pre-processor resolving the provider from the request scope:

```csharp
// Blocks.FastEndpoints/AssignUserIdPreProcessor.cs
public class AssignUserIdPreProcessor : IGlobalPreProcessor
{
    public Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        if (context.Request is IAuditableAction articleCommand)
        {
            var claimsProvider = context.HttpContext.Resolve<IClaimsProvider>();
            articleCommand.CreatedById = claimsProvider.GetUserId();
        }

        return Task.CompletedTask;
    }
}
```

All three share the contract: **stamp only when the request is an `IAuditableAction`, and source the id from `IClaimsProvider`** — nothing else may set `CreatedById`. The presence guard differs, and you should pick with it in mind: the MediatR behavior and the Minimal-API filter use `TryGetUserId()` and skip silently when the claim is absent (the filter also only stamps when `CreatedById` is still default); the FastEndpoints pre-processor calls `GetUserId()`, which **throws** if the id claim is missing. Use the throwing form only where every request through that pipeline is guaranteed authenticated.

`IClaimsProvider` is the shared seam these hooks read — the same interface the resource gate uses for roles:

```csharp
// Blocks.Core/Security/IClaimsProvider.cs
public interface IClaimsProvider
{
    string GetClaimValue(string claimName);
    string? TryGetClaimValue(string claimName);
    IEnumerable<string> GetClaimValues(string claimName);

    int GetUserId();
    int? TryGetUserId();
    string GetUserEmail();
    string GetUserName();

    IReadOnlySet<TEnum> GetUserRoles<TEnum>() where TEnum : struct, Enum;
    IReadOnlySet<string> GetUserRoles();
}
```

## Phase 5 — JWT validation (security-5)

Every service validates the bearer token identically through `Articles.Security.AddJwtAuthentication`, called from the service DI as `services.AddJwtAuthentication(configuration)`. It enforces **issuer + signing key** and **deliberately skips audience** — this is a single-audience internal system, so issuer and signature are the trust boundary and audience is not. Roles map from `ClaimTypes.Role`, which is what feeds `GetUserRoles` in Phases 3 and 4.

```csharp
// Articles.Security/ConfigureAuthentication.cs
public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
{
    var jwtOptions = configuration.GetSectionByTypeName<JwtOptions>();

    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                ValidateAudience = false,   // single-audience internal system — deliberate (security-5)
                RequireExpirationTime = true,
                RoleClaimType = ClaimTypes.Role
            };
        });

    return services;
}
```

The knobs that matter, and why:

- `ValidateAudience = false` — **intentional**, not an oversight. Do not "harden" it by turning audience validation on; there is one audience.
- `ValidateIssuer` + `ValidateIssuerSigningKey` — the real boundary. The signing key is a symmetric secret from `JwtOptions.Secret`.
- `RoleClaimType = ClaimTypes.Role` — makes `GetUserRoles<UserRoleType>()` resolve, so the role and resource gates work.
- `RequireHttpsMetadata = false` is a dev-environment setting; `SaveToken = true` keeps the token available on `HttpContext`.

## What this skill does NOT do

- **Issue or sign tokens.** That is the Auth service's login flow — this skill only *validates* an incoming token.
- **Populate `ArticleActors`.** That table is filled by integration-event consumers (see the consumer/projection patterns); the resource gate only reads it.
- **Implement `IClaimsProvider` / `HttpContextProvider`.** Those live in `Blocks.Core` / `Blocks.AspNetCore` and are consumed here.
- **Define `IAuditableAction` / `ArticleCommandBase`.** The command base and its `[JsonIgnore]` provenance fields are the CQRS command contract (appcqrs-1), used here, not created here.

## Changelog

- **v1.0** — 2026-07-05 — Initial build from reference-model rows security-1..security-5 (source commit `cd4d0b1`). Clean-room: nothing imported from the shipped `nexus-dotnet:authorization-patterns` skill.

## Lessons

Found this skill wrong, stale, or thin against the code? Capture it in the feature's `lessons.md` (or `docs/skill-backlog.md`) with the `security-N` row and the file it disagrees with — a fix folds back here as a consolidating pass, never an additive patch.
