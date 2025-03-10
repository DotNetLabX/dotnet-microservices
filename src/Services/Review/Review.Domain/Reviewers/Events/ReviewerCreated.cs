namespace Review.Domain.Reviewers.Events;

public record ReviewerCreated(Reviewer author, IArticleAction action) 
		: DomainEvent(action);

