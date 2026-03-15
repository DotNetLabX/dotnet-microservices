using Production.Domain.Shared.Enums;

namespace Production.Domain.Shared;

public class ArticleAction : IArticleAction
{
    public int ArticleId { get; set; }
    public string? Comment { get; init; }
    public ArticleActionType ActionType { get; init; }
    public int CreatedById { get; set; }
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
}
