namespace Review.Domain.Invitations.Enums;

public enum InvitationStatus
{
		Open,
		Accepted,
		Declined,
		Expired //todo - create a job to set the status for the expired invitations
}
