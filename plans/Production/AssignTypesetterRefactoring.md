# AssignTypesetter Refactoring

## Summary

Consolidate the AssignTypesetter business operation into the Article aggregate, fix the Typesetter guard bug, and clean up cosmetic issues.

## Affected services

- **Production** — domain behavior, endpoint, validator, repository, summary

## Domain model changes

- `Article.AssignTypesetter` gains stage transition validation (via `Func<ArticleActionType, bool>`) and internally calls `SetStage(InProduction)`
- `ArticleRepository.Query()` includes `Contributors.Person` (fixes Typesetter navigation)

## File list

### Step 1 — Fix ArticleRepository.Query() to include Person
- `src/Services/Production/Production.Persistence/Repositories/ArticleRepository.cs` — modify

### Step 2 — Consolidate AssignTypesetter into the aggregate
- `src/Services/Production/Production.Domain/Articles/Behavior/Article.cs` — modify

### Step 3 — Simplify the endpoint
- `src/Services/Production/Production.API/Features/Articles/AssignTypesetter/AssignTypesetterEndpoint.cs` — modify

### Step 4 — Strip state machine check from validator
- `src/Services/Production/Production.API/Features/Articles/AssignTypesetter/AssignTypesetterCommand.cs` — modify

### Step 5 — Fix the Summary
- `src/Services/Production/Production.API/Features/Articles/AssignTypesetter/AssignTypesetterSummary.cs` — modify

### Step 6 — Fix typo in exception message
- `src/Services/Production/Production.Domain/TypesetterAlreadyAssignedException.cs` — modify

## Numbered steps

### Step 1 — Fix ArticleRepository.Query() to include Person

**Bug:** `Article.Typesetter` is a computed property that checks `aa.Person is Typesetter`. Without `ThenInclude(Person)`, Person is null and the `is Typesetter` check always returns false — allowing multiple typesetters to be assigned.

In `ArticleRepository.cs`, change `Query()` to:
```
Include(e => e.Contributors)
    .ThenInclude(e => e.Person)
```

This is safe for all callers — Contributors is already included, Person is a small TPH entity.

### Step 2 — Consolidate AssignTypesetter into the aggregate

In `Production.Domain/Articles/Behavior/Article.cs`, modify `AssignTypesetter` to:
- Accept a third parameter: `Func<ArticleActionType, bool> canFire`
- First: check `canFire(action.ActionType)` — if false, throw `DomainException("Action not allowed for current stage")`
- Then: the existing Typesetter guard (already there)
- Then: add the contributor (already there)
- Then: raise `TypesetterAssigned` event (already there)
- Finally: call `SetStage(ArticleStage.InProduction, action)` — moved from the endpoint

The method signature becomes:
```
public void AssignTypesetter(Typesetter typesetter, IArticleAction action, Func<ArticleActionType, bool> canFire)
```

This requires adding `using Production.Domain.Shared.Enums;` for `ArticleActionType`.

Also fix the typo in the guard message: "aldready" → "already".

### Step 3 — Simplify the endpoint

In `AssignTypesetterEndpoint.cs`:
- Remove `CheckAndThrowStageTransition` method entirely
- Remove `NextStage` override
- Simplify `HandleAsync` to:
  1. Load article via `_articleRepository.GetByIdOrThrowAsync(command.ArticleId)`
  2. Load typesetter via `_dbContext.Typesetters.FindByIdOrThrowAsync(command.TypesetterId)`
  3. Build state machine: `_stateMachineFactory(_article.Stage).CanFire`
  4. Call `_article.AssignTypesetter(typesetter, command, stateMachine.CanFire)` — pass `CanFire` as the func
  5. Save and respond

The endpoint no longer decides the target stage or validates transitions — the aggregate owns that.

### Step 4 — Strip state machine check from validator

In `AssignTypesetterCommand.cs`:
- Remove the `IsActionValid` method
- Remove the `RuleFor(r => r).MustAsync(...)` rule that checks the state machine
- Remove the unused `using` statements (`Blocks.EntityFrameworkCore`, `Production.Application.StateMachines`, `Production.Persistence.Repositories`)
- Keep only the `RuleFor(r => r.ArticleId).GreaterThan(0)` and `RuleFor(r => r.TypesetterId).GreaterThan(0)` rules — these are genuine input validation
- Remove the two todo comments

### Step 5 — Fix the Summary

In `AssignTypesetterSummary.cs`:
- Change Summary to: `"Assigns a typesetter to an article"`
- Change Description to: `"Assigns a typesetter to an article, transitioning it to the InProduction stage"`
- Change Response to: `Response<IdResponse>(200, "Typesetter was successfully assigned")`
- Remove the todo comment

### Step 6 — Fix typo in exception message

In `TypesetterAlreadyAssignedException.cs` — no change needed here, the message is passed by the caller. The typo "aldready" is in `Behavior/Article.cs` — already fixed in Step 2 as part of the guard message.

## Cross-service impacts

None. All changes are internal to Production.

## Migration notes

None. No schema changes.

## Open questions

None.
