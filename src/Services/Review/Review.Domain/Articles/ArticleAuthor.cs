namespace Review.Domain.Articles;

public class ArticleAuthor : ArticleActor
{
		//public ArticleAuthor() { /*for EF.Core*/ }

		//public ArticleAuthor(HashSet<ContributionArea> contributionAreas)
		//		=> _contributionAreas = contributionAreas;

		//internal HashSet<ContributionArea> _contributionAreas = new();
		//public IReadOnlySet<ContributionArea> ContributionAreas => _contributionAreas;

		public HashSet<ContributionArea> ContributionAreas { get; init; } = null!;
}
