using Review.Domain.Articles;

namespace Review.Domain.Reviewers.Events
{
		// todo - write a handler to send email to the user
		public record ReviewerAssigned(Article Article, Reviewer Reviewer, IArticleAction action) 
				: DomainEvent(action);
}
