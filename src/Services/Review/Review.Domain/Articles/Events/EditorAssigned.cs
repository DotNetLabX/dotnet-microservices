namespace Review.Domain.Articles.Events;

// todo - write a handler to send email to the user
public record EditorAssigned(int AuthorId, int UserId, IArticleAction action) 
		: DomainEvent(action);
