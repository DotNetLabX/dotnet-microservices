// {Svc}.Domain/Events/*.cs — domain events for the EF-backed variant.
// Domain events carry FULL domain entities and stay in-process (boundaries-1). They are the
// aggregate's read-only side channel: raised ONLY via AddDomainEvent from inside the aggregate,
// never constructed by a handler or consumer (domain-1).

namespace {Svc}.Domain.Events;

// One local base per service narrows the generic DomainEvent<TAction> to this service's action type.
// Lifted from src/Services/Submission/Submission.Domain/Events/DomainEvent.cs.
public record DomainEvent(IArticleAction Action) : DomainEvent<IArticleAction>(Action);

// Concrete events derive from the local base and carry the aggregate + the action that caused them.
// Lifted from Submission.Domain/Events/ArticleApproved.cs.
public record {Aggregate}Approved({Aggregate} {Aggregate}, IArticleAction Action) : DomainEvent(Action);

public record {Child}Assigned({Child} {Child}, IArticleAction Action) : DomainEvent(Action);

// Note: ArticleStageChanged is shared (Articles.Abstractions.Events) because stage changes cross
// service boundaries — reuse it rather than re-declaring a per-service stage-changed event.
