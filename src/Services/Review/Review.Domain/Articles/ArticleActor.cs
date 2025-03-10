namespace Review.Domain.Articles;

public class ArticleActor : Entity
{
    public int ArticleId { get; init; } 
		public int PersonId { get; init; }
		public Person Person { get; init; } = null!;
		public UserRoleType Role { get; init; }

		public string TypeDiscriminator { get; init; } = null!; // EF discriminator to manage inheritance
}
