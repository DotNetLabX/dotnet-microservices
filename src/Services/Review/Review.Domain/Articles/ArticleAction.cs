using Review.Domain.Shared.Enums;

namespace Review.Domain.Articles;

//insight - modification never happens for an action

public partial class ArticleAction : Entity, IArticleAction
{
    public int ArticleId { get; init; }
    //public int EntityId { get; init; }
    public string? Comment { get; init; } = default;
    public ArticleActionType ActionType { get; init; }
    public int CreatedById { get; set; }
    public DateTime CreatedOn { get; init; }
}
