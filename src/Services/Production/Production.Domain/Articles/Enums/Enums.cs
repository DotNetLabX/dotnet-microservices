using System.ComponentModel;

namespace Production.Domain.Assets.Enums;

//decide -  keep AssetActionType & ArticleActionType separetly?
public enum AssetActionType
{
    Upload,
    Approve,
    Request,
    CancelRequest,
    Download
}

public enum AssetState
{
    None = 0,
    Requested = 1,
    Uploaded = 2,
    Approved = 3
}

public enum DiscussionType
{
		Default = 0,
		Public = 1,
		Private = 2
}

