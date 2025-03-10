namespace Submission.Domain.Entities;

public partial class Article : AggregateRoot
{
    internal Article() {}

    public required string Title { get; init; }
		public ArticleType Type { get; init; }
		public string Scope { get; init; } = default!;

		public DateTime? SubmittedOn { get; set; }
    public int? SubmittedById { get; set; }
    public Person? SubmittedBy { get; set; }

    public ArticleStage Stage { get; set; }

		public int JournalId { get; init; }
		public required Journal Journal { get; init; } = null!;

		// talk - ways to represent collections 
		private readonly List<Asset> _assets = new();
    public IReadOnlyList<Asset> Assets => _assets.AsReadOnly();

		private readonly List<ArticleActor> _actors = new();
		public IReadOnlyList<ArticleActor> Actors => _actors.AsReadOnly();

		private readonly List<StageHistory> _stageHistories = new();
    public IReadOnlyList<StageHistory> StageHistories => _stageHistories.AsReadOnly();

		private readonly List<ArticleAction> _actions = new();
		public IReadOnlyList<ArticleAction> Actions => _actions.AsReadOnly();
}
