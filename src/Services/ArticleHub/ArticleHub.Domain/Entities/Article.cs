using Articles.Abstractions.Enums;

namespace ArticleHub.Domain.Entities;

public class Article : Entity
{
    public required string Title { get; set; }
    public string? Doi { get; set; }
    public ArticleStage Stage { get; set; }

    public required virtual int SubmittedById { get; set; }
    public virtual Person SubmittedBy { get; set; } = null!;

    public DateTime SubmittedOn { get; set; }
    public DateTime? AcceptedOn { get; set; }
    public DateTime? PublishedOn { get; set; }

    public required int JournalId { get; set; }
    public Journal Journal { get; set; } = null!;

    public List<ArticleActor> Actors { get; set; } = new();
}
