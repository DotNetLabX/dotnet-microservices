using Review.Domain.Assets;
using Review.Domain.Invitations;

namespace Review.Domain.Articles;

public partial class Article : AggregateRoot
{
		public required string Title { get; init; }
		public ArticleType Type { get; init; }
		public string Scope { get; init; } = default!;

		public DateTime? SubmittedOn { get; init; }
    public int? SubmittedById { get;  init; }
    public Person? SubmittedBy { get; init; }

    public ArticleStage Stage { get; private set; }

		public required int JournalId { get; init; }
		public Journal Journal { get; init; } = null!;

		// talk - ways to represent collections 
		private readonly List<Asset> _assets = new();
    public IReadOnlyList<Asset> Assets => _assets.AsReadOnly();

		private readonly List<ArticleActor> _actors = new();
		public IReadOnlyList<ArticleActor> Actors => _actors.AsReadOnly();

		public Editor Editor => (Editor)_actors.Single(a => a.Role == UserRoleType.REVED).Person;
		
		private readonly List<StageHistory> _stageHistories = new();
    public IReadOnlyList<StageHistory> StageHistories => _stageHistories.AsReadOnly();

		private readonly List<ArticleAction> _actions = new();
		public IReadOnlyList<ArticleAction> Actions => _actions.AsReadOnly();

		private readonly List<ReviewInvitation> _invitations = new();
		public IReadOnlyList<ReviewInvitation> Invitations => _invitations;
}
