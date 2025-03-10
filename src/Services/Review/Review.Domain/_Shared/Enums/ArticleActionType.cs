namespace Review.Domain.Shared.Enums;

public enum ArticleActionType
{
		ApproveForReview,
		AssignEditor,
		CreateReviewer,
		InviteReviewer,
		AcceptInvitation,
		DeclineInvitation,
		UploadManuscript,
		UploadReviewReport,
		RevisionRequested,
		AcceptArticle,
		RejectArticle,
}
