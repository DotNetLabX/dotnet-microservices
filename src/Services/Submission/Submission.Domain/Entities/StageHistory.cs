namespace Submission.Domain.Entities;

public partial class StageHistory : Entity
{
    public required DateTime StartDate { get; init; }
    public required ArticleStage StageId { get; init; }
    public required int ArticleId { get; init; }
}
