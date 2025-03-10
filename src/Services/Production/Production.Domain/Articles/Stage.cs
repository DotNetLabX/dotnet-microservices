namespace Production.Domain.Articles;

public partial class Stage : EnumEntity<ArticleStage>
{
    public string Info { get; set; } = null!;
}
