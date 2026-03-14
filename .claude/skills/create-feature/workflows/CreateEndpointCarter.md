# Create Endpoint — Carter (Review, ArticleHub)

`ICarterModule` with `AddRoutes()`. Dispatches to MediatR via `ISender`.

## Pattern

**Reference:** `src/Services/Review/Review.API/Endpoints/Articles/AcceptArticleEndpoint.cs`

Create `{FeatureName}Endpoint.cs` in the API project's `Endpoints/{Domain}/` folder:

```csharp
public class {FeatureName}Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/{route}", async (int articleId, {CommandType} command, ISender sender) =>
        {
            command.ArticleId = articleId;
            var response = await sender.Send(command);
            return Results.Ok(response);
        })
        .RequireRoleAuthorization(Role.{RequiredRole})
        .WithName("{FeatureName}")
        .WithTags("{Domain}")
        .Produces<{ResponseType}>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
```

## Registration

Carter endpoints are auto-discovered — no manual registration needed. `services.AddCarter()` + `app.MapCarter()` handle discovery.
