# Create Handler — MediatR (Submission, Review)

For services that use MediatR. Handler lives in the Application layer.

## Command record

**Reference:** `src/Services/Review/Review.Application/Features/Articles/AcceptArticle/AcceptArticleCommand.cs`

Create `{FeatureName}Command.cs` (or `{FeatureName}Query.cs`):

```csharp
// Command (mutates state)
public record {FeatureName}Command : ArticleCommand
{
    public override ArticleActionType ActionType => ArticleActionType.{ActionType};
    // Additional properties from request body
}

// Query (reads data)
public record {FeatureName}Query(int ArticleId) : IQuery<{FeatureName}Response>;
public record {FeatureName}Response({ResponseProperties});
```

For non-article commands:
```csharp
public record {FeatureName}Command({Properties}) : ICommand<{ResponseType}>;
```

## Handler

**Reference:** `src/Services/Review/Review.Application/Features/Articles/AcceptArticle/AcceptArticleCommandHandler.cs`

Create `{FeatureName}CommandHandler.cs` (or `{FeatureName}QueryHandler.cs`):

```csharp
public class {FeatureName}CommandHandler({Repository} _repository, {OtherDependencies})
    : IRequestHandler<{FeatureName}Command, {ResponseType}>
{
    public async Task<{ResponseType}> Handle({FeatureName}Command command, CancellationToken ct)
    {
        // 1. Load aggregate
        var entity = await _repository.FindByIdOrThrowAsync(command.ArticleId);

        // 2. Call domain method (raises domain events internally)
        entity.{DomainMethod}(command);

        // 3. Save (interceptor dispatches domain events after save)
        await _repository.SaveChangesAsync();

        // 4. Return response
        return new {ResponseType}(entity.Id);
    }
}
```

## File location

- Submission: `src/Services/Submission/Submission.Application/Features/{Domain}/{FeatureName}/`
- Review: `src/Services/Review/Review.Application/Features/{Domain}/{FeatureName}/`
