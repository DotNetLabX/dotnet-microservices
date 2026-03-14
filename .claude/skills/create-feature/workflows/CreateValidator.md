# Create Validator

## MediatR services (Submission, Review)

Extend `AbstractValidator<T>`. Can be in a separate file or inlined in the command file.

**Reference:** `src/Services/Auth/Auth.API/Features/Users/CreateAccount/CreateUserCommandValidator.cs`

```csharp
public class {FeatureName}Validator : AbstractValidator<{FeatureName}Command>
{
    public {FeatureName}Validator()
    {
        RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof({Command}.ArticleId));
        RuleFor(c => c.{Property}).NotEmptyWithMessage(nameof({Command}.{Property}));
    }
}
```

For article commands, extend the shared base:
```csharp
public class {FeatureName}Validator : ArticleCommandValidator<{FeatureName}Command>
{
    public {FeatureName}Validator() : base()
    {
        // Additional rules beyond ArticleId validation
    }
}
```

## FastEndpoints services (Production)

Extend `BaseValidator<T>` (custom, wraps FastEndpoints `Validator<T>`):

**Reference:** `src/Services/Production/Production.API/Features/_Shared/Validators.cs`

```csharp
public class {FeatureName}Validator : BaseValidator<{FeatureName}Command>
{
    public {FeatureName}Validator()
    {
        RuleFor(c => c.ArticleId).GreaterThan(0).WithMessageForInvalidId(nameof({Command}.ArticleId));
    }
}
```

## FastEndpoints services (Auth, Journals)

Use `AbstractValidator<T>` directly (no custom base).

## Custom validation extensions

From `Blocks.Core/FluentValidation/Extensions.cs`:

| Extension | Message |
|-----------|---------|
| `.NotEmptyWithMessage(propertyName)` | "{PropertyName} is required." |
| `.WithMessageForInvalidId(propertyName)` | "The {PropertyName} should be greater than zero." |
| `.WithMessageForMaxLength(name, maxLen)` | "{Name} must not exceed {N} characters." |
| `.MaximumLengthWithMessage(maxLen, name)` | Same as above |

## Registration

- MediatR services: auto-discovered via `AddValidatorsFromAssemblyContaining<T>()`, invoked by `ValidationBehavior`
- FastEndpoints: auto-discovered from assembly
