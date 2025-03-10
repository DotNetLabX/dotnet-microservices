namespace Production.Domain.Articles.Events;

public record TypesetterAssigned(int TypesetterId, int TypesetterUserId, IArticleAction action) 
		: DomainEvent(action);
