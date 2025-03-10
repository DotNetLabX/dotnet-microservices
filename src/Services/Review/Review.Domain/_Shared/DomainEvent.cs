namespace Review.Domain.Shared;

public abstract record DomainEvent(IArticleAction Action) : DomainEvent<IArticleAction>(Action);
