namespace Submission.Domain.Entities;

//insight - modification never happens for an action

public partial class ArticleAction : Entity
{
    public int EntityId { get; set; }
    public string Comment { get; set; } = default!;
    public ArticleActionType TypeId { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedOn { get; set; }
}
