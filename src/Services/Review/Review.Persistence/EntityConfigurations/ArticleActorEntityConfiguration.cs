namespace Review.Persistence.EntityConfigurations;

internal class ArticleActorEntityConfiguration : EntityConfiguration<ArticleActor>
{
		public override void Configure(EntityTypeBuilder<ArticleActor> builder)
		{
				builder.HasIndex(e => new { e.ArticleId, e.PersonId, e.Role }).IsUnique();
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
