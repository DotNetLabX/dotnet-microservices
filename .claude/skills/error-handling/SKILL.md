---
name: error-handling
description: Exception hierarchy, guards, and error flow for this codebase — which exception to throw where, and the one place errors turn into HTTP responses. Use when throwing an error, adding a new exception type, choosing between a domain and an HTTP exception, writing a null/not-found check, or considering a try/catch.
---

# Error Handling

Three rules hold the whole system together:

1. Exceptions **self-describe their status** — an `HttpException` carries its own `HttpStatusCode`; there is no separate exception→status lookup table (errors-1).
2. **One place translates** an exception into an HTTP response: the switch inside `GlobalExceptionMiddleware`. Nowhere else (errors-3).
3. **Business, domain, and endpoint code just throws.** Failures are never returned as values (appcqrs-3).

## Which exception to throw where

| You are in… | Throw | Lives in | Why |
|---|---|---|---|
| Application / endpoint / handler code — bad input or a missing resource | `BadRequestException`, `NotFoundException`, `UnauthorizedException` (subclasses of `HttpException`) (errors-1) | `Blocks.Exceptions` | The exception carries the status; the middleware reads it, no lookup |
| Domain code — aggregate, value object, state machine — an invariant is broken | `DomainException` (errors-2) | `Blocks.Domain` | The domain knows nothing about HTTP; the middleware maps it to **400** later |
| A command/query fails validation | **nothing** — let the pipeline's FluentValidation `ValidationException` propagate (appcqrs-3) | — | Validators throw; the handler is never reached, and never returns an error |

Why `DomainException` maps to 400 and not 500: a broken invariant is the caller's fault (they asked for an illegal transition), so the middleware treats it as a bad request (errors-2, errors-3).

## Binding prohibitions

These are not style preferences — code that violates them is wrong here.

- **No error-monad, ever.** No `Result<T>`, `OneOf`, `ErrorOr`, `FluentResults`, `LanguageExt`. None of those libraries is referenced and none may be added. A handler returns its success payload (`IdResponse`, a DTO, …) or it throws — it never returns a failure value (appcqrs-3).
- **Translation lives in one switch only.** Exception→status mapping happens solely in `GlobalExceptionMiddleware.MapStatusCode`. Never add a per-endpoint `try/catch` that turns an exception into a response — that duplicates the seam and the two drift (errors-3).
- **No inline null/not-found checks.** Never hand-write `if (x is null) throw …` for the null-or-missing case. Use the guards below, at the top of the method (errors-4).
- **`try/catch` is for I/O boundaries only** — see the list further down. Business logic never catches (errors-5).

## The exception types (lifted from source)

`HttpException` is the base; the status is chosen once, in each subclass's constructor.

```csharp
// Blocks.Exceptions/HttpException.cs
public class HttpException : Exception
{
    public HttpException(HttpStatusCode statusCode, string? message, Exception ex) : base(message, ex)
        => this.HttpStatusCode = statusCode;

    public HttpException(HttpStatusCode statusCode, string? message)
        : base(string.IsNullOrEmpty(message) ? statusCode.ToString() : message)
        => this.HttpStatusCode = statusCode;

    public HttpStatusCode HttpStatusCode { get; }
    public int StatusCode => (int) HttpStatusCode;
}

// Blocks.Exceptions/NotFoundException.cs — a thin status-picking subclass
public class NotFoundException : HttpException
{
    public NotFoundException(string exceptionMessage) : base(HttpStatusCode.NotFound, exceptionMessage) { }
}
```

`DomainException` carries **no** status — it is a plain `Exception` in a different package, so the domain layer never takes a dependency on HTTP concepts (errors-2).

```csharp
// Blocks.Domain/DomainException.cs
public class DomainException(string message, Exception? innerException = null)
    : Exception(message, innerException);
```

## Guards: throw at the top, don't null-check inline (errors-4)

`Blocks.Core.Guard` and `Blocks.Core.GuardExtensions` centralize the throwing checks. Call them at the top of the method; then the body runs against known-good values.

```csharp
public static class Guard
{
    public static void ThrowIfNullOrWhiteSpace(string value) => ArgumentException.ThrowIfNullOrWhiteSpace(value);
    public static void ThrowIfFalse(this bool condition, string message = "Condition must be true.") { if (!condition) throw new ArgumentException(message); }
    public static T AgainstNull<T>(T? value, string parameterName) => value ?? throw new ArgumentNullException(parameterName, $"Value cannot be null: '{parameterName}'.");
    public static T NotFound<T>(T? value) where T : class => value ?? throw new NotFoundException($"{typeof(T).Name} not found");
}

// GuardExtensions.cs — the fluent spelling
public static T OrThrowNotFound<T>(this T? value, string? message = null) where T : class
    => value ?? throw new NotFoundException(message ?? $"{typeof(T).Name} not found");
```

Two spellings of the not-found guard coexist **on purpose** (there's an in-code insight comment marking it): the static `Guard.NotFound<T>(x)` and the fluent `x.OrThrowNotFound()`. They do the same thing — pick whichever reads better at the call site; do not add a third. There is also `IEnumerable<T>.SingleOrThrow(predicate)` in `Blocks.Linq` for the query case, which throws `NotFoundException` too.

At the repository layer the same rule has a dedicated family in `Blocks.EntityFrameworkCore/Extensions/RepositoryExtensions.cs` — these wrap `Guard.NotFound`, so they are the load-and-guard case expressed once, not a competing fourth spelling:

```csharp
// load by id or 404 (untracked / tracked)
public static async Task<TEntity> FindByIdOrThrowAsync<TEntity, TContext>(this RepositoryBase<TContext, TEntity> repository, int id)
    => Guard.NotFound(await repository.FindByIdAsync(id));
public static async Task<TEntity> GetByIdOrThrowAsync<TEntity, TContext>(this RepositoryBase<TContext, TEntity> repository, int id, CancellationToken ct = default)
    => Guard.NotFound(await repository.GetByIdAsync(id));

// existence gates
public static async Task ExistsOrThrowAsync<…>(…)          // throws NotFoundException (404) if the id is absent
public static async Task EnsureNotExistsOrThrowAsync<…>(…) // throws BadRequestException (400) if it already exists — the consumer idempotency guard
```

The discipline holds across all of them: pick the guard at your layer — value → `Guard.NotFound`/`OrThrowNotFound`, query → `SingleOrThrow`, repository → this family — and don't hand-roll the check inline.

Use:
- `var journal = await _repository.GetByIdAsync(id).OrThrowNotFound();` — missing resource → 404.
- `Guard.AgainstNull(command.Title, nameof(command.Title));` — argument contract → `ArgumentNullException` (→ 400 via the switch).
- `EmailAddress.IsValidEmail(value).ThrowIfFalse("Invalid email.");` — value-object construction guard.

## try/catch only at true I/O boundaries (errors-5)

A `try/catch` is sanctioned **only** where you cross into something that can fail for reasons outside the domain:

- File-storage adapters — MinIO, MongoGridFS, AzureBlob.
- SMTP send.
- Third-party SDK / external HTTP — the Hasura client.
- EF seeding.
- The upload-file handlers, which wrap the domain mutation to **compensate with a delete** when the file write fails, then rethrow (the file store sits outside the SQL transaction — the catch substitutes for cross-store atomicity; see the file-storage skill).

Everywhere else — handlers, domain methods, consumers, endpoints — **just throw**. Do not wrap business logic in `try/catch` to log-and-rethrow or to convert an exception; the middleware already logs 5xx and converts everything.

## Adding a new exception type

Two edits, in two files — that is the whole recipe. Example: a 409 conflict.

```csharp
// 1. Subclass HttpException, pick the status once in the ctor (Blocks.Exceptions/ConflictException.cs)
public class ConflictException : HttpException
{
    public ConflictException(string exceptionMessage) : base(HttpStatusCode.Conflict, exceptionMessage) { }
}

// 2. Add ONE arm to GlobalExceptionMiddleware.MapStatusCode
ConflictException => HttpStatusCode.Conflict,
```

Gotcha worth knowing: the switch matches **concrete** types, not the `HttpException` base, so a new subclass that you forget to add falls through to `_ => InternalServerError` (500) — even though it already carries the right status. The subclass and the switch arm always ship together (errors-3).

For a **domain-rule** violation, subclass `DomainException` instead — and this path needs **no** switch arm, because the `DomainException => BadRequest` arm is a type pattern that already matches every subclass:

```csharp
// Blocks.Domain-side or the service's Domain — no middleware change needed
public class TypesetterAlreadyAssignedException(string message) : DomainException(message);
```

Both land on 400; the choice is which package the exception lives in and whether the domain layer is allowed to name it (errors-2).

## The middleware switch (reference — don't touch it per endpoint)

Source: `src/BuildingBlocks/Blocks.AspNetCore/Middlewares/GlobalExceptionMiddleware.cs`. Registered once per service as `app.UseMiddleware<GlobalExceptionMiddleware>()` in `Program.cs` (all 6 services). This is the single translation seam (errors-3):

```csharp
private static HttpStatusCode MapStatusCode(Exception ex) => ex switch
{
    ValidationException  => HttpStatusCode.BadRequest,
    ArgumentException    => HttpStatusCode.BadRequest,
    BadRequestException  => HttpStatusCode.BadRequest,
    NotFoundException    => HttpStatusCode.NotFound,
    DomainException      => HttpStatusCode.BadRequest,
    UnauthorizedException => HttpStatusCode.Unauthorized,
    _                    => HttpStatusCode.InternalServerError
};
```

`InvokeAsync` also special-cases two flows before this switch: a FluentValidation `ValidationException` is unpacked into a structured errors body, and `OperationCanceledException` (client abandoned the request) returns **499** without logging. 5xx responses are logged with the `TraceId`; the stack trace is included in the body only in Development.

The client sees one of two JSON shapes. The general shape (every non-validation error):

```json
{
  "StatusCode": 404,
  "Message": "Article not found",
  "TraceId": "0HN...",
  "Details": null
}
```

The validation shape (a FluentValidation `ValidationException` from the pipeline):

```json
{
  "StatusCode": 400,
  "Message": "One or more validation errors occurred.",
  "TraceId": "0HN...",
  "Errors": [
    { "PropertyName": "Title", "ErrorMessage": "'Title' must not be empty." }
  ],
  "Details": null
}
```

`Details` carries the stack trace only when the environment is Development; it is `null` in every other environment.

## What this skill does NOT do

- **How the FluentValidation `ValidationException` is thrown.** This skill states only that validation failures are thrown by the pipeline and translated once here. The `ValidationBehavior`, its pipeline order, and the framework-split validator vocabulary live in `service-infra-conventions` (validators are placed per feature by `create-feature-slice`).
- **The mechanics of the compensating-delete upload wrappers.** Named here as a sanctioned I/O boundary (errors-5); the `TryDeleteAsync` compensation, provider registration, and cross-store byte migration belong to `file-storage-patterns`.
