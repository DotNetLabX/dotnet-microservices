namespace Review.Domain.Invitations.Events;

public record ReviewerInvited(ReviewInvitation Invitation, IArticleAction Action)
		: DomainEvent(Action);
