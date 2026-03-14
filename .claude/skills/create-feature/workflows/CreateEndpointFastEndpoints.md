# Create Endpoint — FastEndpoints (Auth, Journals, Production)

Class extending `Endpoint<TRequest, TResponse>`. Handler logic lives directly in `HandleAsync` — no MediatR dispatch.

## Pattern

**Reference:** `src/Services/Auth/Auth.API/Features/Users/CreateAccount/CreateUserEndpoint.cs`

Create `{FeatureName}Endpoint.cs` in `Features/{Domain}/{FeatureName}/`:

```csharp
[Authorize(Roles = Role.{RequiredRole})]
[Http{Method}("{route}")]
[Tags("{Domain}")]
public class {FeatureName}Endpoint({Dependencies})
    : Endpoint<{CommandType}, {ResponseType}>
{
    public override async Task HandleAsync({CommandType} command, CancellationToken ct)
    {
        // Load aggregate/data
        // Execute domain logic
        // Save changes
        // Publish domain events if needed: await PublishAsync(new {Event}(...));

        await SendOkAsync(new {ResponseType}(...), ct);
    }
}
```

## Alternative: Configure() partial class split

For complex configuration, split into two partial files:

**`{FeatureName}Endpoint.Configure.cs`:**
```csharp
public partial class {FeatureName}Endpoint
{
    public override void Configure()
    {
        AllowAnonymous(); // or Roles(...)
        Post("{route}");
        Description(x => x.WithSummary("...").WithTags("{Domain}"));
    }
}
```

## Registration

FastEndpoints auto-discovers endpoints — no manual registration needed.

## Notes

- Journals: handler class name often ends with `QueryHandler` or `CommandHandler` even though it's an endpoint (legacy naming)
- Production validators extend `BaseValidator<T>` (custom, wraps `Validator<T>`)
- Auth uses `Send.OkAsync()` instead of `SendOkAsync()` in some places — both work
