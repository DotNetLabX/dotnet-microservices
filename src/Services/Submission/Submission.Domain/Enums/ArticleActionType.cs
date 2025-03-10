namespace Submission.Domain.Enums;

//decide -  keep AssetActionType & ArticleActionType separetly?
public enum ArticleActionType
{
		CreateArticle,
		CreateAuthor,
		AssignAuthor,
		UploadAsset,
		SubmitDraft,
		ApproveDraft,
		RejectDraft,
}
