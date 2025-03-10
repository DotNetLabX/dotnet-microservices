namespace Submission.Domain.Events;

public record DomainEvent(IArticleAction Action) : DomainEvent<IArticleAction>(Action);