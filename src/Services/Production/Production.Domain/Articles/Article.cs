using Production.Domain.Assets;

namespace Production.Domain.Articles;

public partial class Article : AggregateRoot
{
    public required string Title { get; set; }
    public required string Doi { get; set; }
    //public string JournalSection { get; set; } = default!;

    public required virtual int SubmitedById { get; set; }
    public virtual Person SubmitedBy { get; set; } = null!;
		public DateTime SubmitedOn { get; set; }

		public DateTime AcceptedOn { get; set; }

		public ArticleStage Stage { get; set; }

    public required int JournalId { get; set; }
    public int VolumeId { get; set; }

    public DateTime? PublishedOn { get; set; }
    public Person? PublishedBy{ get; set; }

    public Journal Journal { get; init; } = null!;

		// talk - ways to represent collections 
		private readonly List<Asset> _assets = new();
    public IReadOnlyList<Asset> Assets => _assets.AsReadOnly();

		private readonly List<StageHistory> _stageHistories = new();
    public IReadOnlyList<StageHistory> StageHistories => _stageHistories.AsReadOnly();

		private readonly List<ArticleContributor> _contributors = new();
		public IReadOnlyCollection<ArticleContributor> Contributors => _contributors.AsReadOnly();
		public Typesetter? Typesetter => Contributors.Where(aa => aa.Person is Typesetter).Select(aa => aa.Person as Typesetter).FirstOrDefault();
}
