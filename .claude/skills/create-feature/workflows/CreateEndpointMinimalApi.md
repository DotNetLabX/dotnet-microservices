# Create Endpoint — Minimal APIs (Submission)

Static class with `Map()` extension method. Dispatches to MediatR via `ISender`.

## Pattern

**Reference:** `src/Services/Submission/Submission.API/Endpoints/CreateArticleEndpoint.cs`

Create `{FeatureName}Endpoint.cs` in the API project's `Endpoints/` folder:

```csharp
public static class {FeatureName}Endpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/{route}", async ({RequestType} command, ISender sender) =>
        {
            var response = await sender.Send(command);
            return Results.Ok(response);
        })
        .RequireRoleAuthorization(Role.{RequiredRole})
        .WithName("{FeatureName}")
        .WithTags("{Domain}")
        .Produces<{ResponseType}>(StatusCodes.Status200OK);
    }
}
```

## Registration

Add to `EndpointRegistration.MapAllEndpoints()`:
```csharp
{FeatureName}Endpoint.Map(api);
```

**File:** `src/Services/Submission/Submission.API/Endpoints/XEndpointRegistration.cs`

## Route parameter binding

For commands with `ArticleId` from route: `app.MapPost("/articles/{articleId:int}:action", async (int articleId, {Command} command, ISender sender) => { command.ArticleId = articleId; ... })`
