using Blocks.Domain;

namespace Articles.Abstractions;

public abstract record DomainEvent<TAction>(TAction Action) : IDomainEvent
		where TAction: IArticleAction;
