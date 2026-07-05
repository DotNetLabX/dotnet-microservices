// {Svc}.Domain/Behaviors/{Aggregate}.cs — EF-backed variant, BEHAVIOR partial.
// What the aggregate DOES. Same class name, same namespace, second `partial` file (domain-2).
// Lifted from src/Services/Submission/Submission.Domain/Behaviors/Article.cs.

using Articles.Abstractions.Events;
using Blocks.Domain;
using {Svc}.Domain.StateMachines;

namespace {Svc}.Domain.Entities;

public partial class {Aggregate}
{
    // RULE (owner feedback — authoritative): the `action` parameter is ALWAYS LAST. Order the
    // parameters: entity/domain args -> domain services/delegates (ArticleStateMachineFactory) ->
    // action last. This is what Review and Production do. Submission's four Article behaviors put
    // action before the factory — that is known drift that predates the rule, do NOT copy it.

    // A behavior that changes stage routes through SetStage so the state machine — the sanctioned
    // second enforcement site (domain-2) — validates the transition BEFORE any mutation.
    public void SetStage({Enum} newStage, ArticleStateMachineFactory stateMachineFactory, IArticleAction<{ActionType}> action)
    {
        stateMachineFactory.ValidateStageTransition(Stage, action.ActionType);   // guards first (domain-3)

        if (newStage == Stage)
            return;

        var currentStage = Stage;
        Stage = newStage;
        LastModifiedOn = action.CreatedOn;
        LastModifiedById = action.CreatedById;

        AddDomainEvent(new ArticleStageChanged(currentStage, newStage, action));  // side channel only (domain-1)
    }

    // A behavior that enforces an invariant throws DomainException — a plain domain exception,
    // no HTTP type (errors-2). action is last (owner feedback).
    public void Assign{Child}({Child} {child}, IArticleAction<{ActionType}> action)
    {
        if (_{child}s.Exists(x => x.Id == {child}.Id))
            throw new DomainException($"{child} {{{child}.Id}} is already assigned");

        _{child}s.Add({child});
        AddDomainEvent(new {Child}Assigned({child}, action));
    }
}
