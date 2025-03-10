using Production.Domain.Shared;

namespace Production.Domain.Articles;

public class ArticleContributor : IAssociationEntity
{
		public int ArticleId { get; init; }
		public virtual Article Article { get; init; } = null!;
		public required int PersonId { get; init; }
		public Person Person { get; init; } = null!;
		public UserRoleType Role { get; init; }
}
