namespace Submission.Persistence.EntityConfigurations;

internal class ArticleActorEntityConfiguration : IEntityTypeConfiguration<ArticleActor>
{
		public void Configure(EntityTypeBuilder<ArticleActor> builder)
		{
				builder.HasKey(e => new { e.ArticleId, e.PersonId, e.Role });
				builder.Property(e => e.Role).HasEnumConversion().HasDefaultValue(UserRoleType.AUT);

				//insight about EF Core inheritance
				builder.HasDiscriminator(e => e.TypeDiscriminator)
						.HasValue<ArticleActor>(nameof(ArticleActor))
						.HasValue<ArticleAuthor>(nameof(ArticleAuthor));

				builder.HasOne(aa => aa.Person)
						.WithMany(a => a.ArticleActors)
						.HasForeignKey(aa => aa.PersonId)
						.OnDelete(DeleteBehavior.Restrict);
		}
}
