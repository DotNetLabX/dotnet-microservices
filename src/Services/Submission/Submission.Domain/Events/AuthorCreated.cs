namespace Submission.Domain.Events;

public record AuthorCreated(Author author, IArticleAction action)
    : DomainEvent(action);
