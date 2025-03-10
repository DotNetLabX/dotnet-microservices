namespace Submission.Persistence.EntityConfigurations;

public class ArticleAuthorEntityConfiguration : IEntityTypeConfiguration<ArticleAuthor>
{
    public void Configure(EntityTypeBuilder<ArticleAuthor> builder)
    {
				builder.Property(e => e.ContributionAreas).HasJsonCollectionConversion().IsRequired();
		}
}
