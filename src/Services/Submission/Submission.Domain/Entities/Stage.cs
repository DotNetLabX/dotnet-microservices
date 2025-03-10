namespace Submission.Domain.Entities;

public partial class Stage : EnumEntity<ArticleStage>
{
    public string Info { get; init; } = null!;
}
