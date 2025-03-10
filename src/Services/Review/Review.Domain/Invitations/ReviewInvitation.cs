using Review.Domain.Shared.ValueObjects;
using Review.Domain.Invitations.Enums;
using Review.Domain.Invitations.ValueObjects;

namespace Review.Domain.Invitations;

public partial class ReviewInvitation : AggregateRoot
{
    public required int ArticleId { get; init; }

		public int? UserId { get; init; }
		public required EmailAddress Email { get; init; }
		public required string FirstName { get; init; }
		public required string LastName { get; init; }

		public string FullName => FirstName + ' ' + LastName;

		public DateTime SentOn { get; init; } = DateTime.UtcNow;
		public required int SentById { get; set; }
		public Person SentBy { get; set; } = null!;

		public required DateTime ExpiresOn { get; init; }
		public bool IsExpired => ExpiresOn < DateTime.UtcNow;

		public required InvitationToken Token { get; init; }

    public InvitationStatus Status { get; set; } = InvitationStatus.Open;
}
