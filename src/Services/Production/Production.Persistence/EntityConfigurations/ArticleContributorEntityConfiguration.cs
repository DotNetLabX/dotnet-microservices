namespace Production.Persistence.EntityConfigurations;

internal class ArticleActorEntityConfiguration : IEntityTypeConfiguration<ArticleActor>
{
    public void Configure(EntityTypeBuilder<ArticleActor> entity)
    {
        entity.HasKey(e => new { e.ArticleId, e.PersonId, e.Role });
        entity.Property(e => e.Role).HasEnumConversion().HasDefaultValue(UserRoleType.AUT);

        entity.HasOne(aa => aa.Article)
            .WithMany(a => a.Actors)
            .HasForeignKey(aa => aa.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(aa => aa.Person)
            .WithMany()
            .HasForeignKey(aa => aa.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
