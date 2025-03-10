using Blocks.Domain;
using Review.Domain.Invitations.Enums;

namespace Review.Domain.Invitations;

public partial class ReviewInvitation
{
		public void Accept()
		{
				if (Status != InvitationStatus.Open)
						throw new DomainException("Invitation is not open anymore.");

				if(ExpiresOn <  DateTime.UtcNow)
						throw new DomainException("Invitation expired.");

				//todo consider adding an InvitationAccepted domain event
				Status = InvitationStatus.Accepted;
		}

		public void Decline()
		{
				if (Status != InvitationStatus.Open)
						throw new DomainException("Invitation is not open anymore.");

				//todo consider adding an InvitationDeclined domain event
				Status = InvitationStatus.Declined;
		}
}
