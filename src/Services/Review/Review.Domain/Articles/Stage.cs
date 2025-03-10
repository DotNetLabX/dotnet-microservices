namespace Review.Domain.Articles;

public partial class Stage : EnumEntity<ArticleStage>
{
    public string Info { get; init; } = null!;
}
