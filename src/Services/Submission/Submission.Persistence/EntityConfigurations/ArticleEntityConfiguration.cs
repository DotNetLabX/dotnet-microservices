namespace Submission.Persistence.EntityConfigurations;

public class ArticleEntityConfiguration : AuditedEntityConfiguration<Article>
{
    public override void Configure(EntityTypeBuilder<Article> builder)
    {
        base.Configure(builder);

        builder.HasIndex(e => e.Title);

        builder.Property(e => e.Title).HasMaxLength(MaxLength.C256).IsRequired();
				builder.Property(e => e.Scope).HasMaxLength(MaxLength.C2048).IsRequired();
				builder.Property(e => e.Stage).HasEnumConversion().IsRequired();
				builder.Property(e => e.Type).HasEnumConversion().IsRequired();

				builder.HasOne<Stage>().WithMany()
					 .HasForeignKey(e => e.Stage)
					 .HasPrincipalKey(e => e.Name)
					 .IsRequired()
					 .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SubmittedBy).WithMany()
            .HasForeignKey(e => e.SubmittedById)
            .HasPrincipalKey(e => e.Id)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Journal).WithMany(e => e.Articles)
            .HasForeignKey(e => e.JournalId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Assets).WithOne(e => e.Article)
            .HasForeignKey(e => e.ArticleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

				builder.HasMany(e => e.Actors).WithOne()
						.HasForeignKey(e => e.ArticleId)
						.IsRequired()
						.OnDelete(DeleteBehavior.Cascade);

				builder.HasMany(e => e.StageHistories).WithOne()
            .HasForeignKey(e => e.ArticleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

				builder.HasMany(e => e.Actions).WithOne()
		        .HasForeignKey(e => e.EntityId)
		        .IsRequired()
		        .OnDelete(DeleteBehavior.Cascade);
		}
}
