namespace Review.Persistence.EntityConfigurations;

public class StageEntityConfiguration : EnumEntityConfiguration<Stage, ArticleStage>
{
    public override void Configure(EntityTypeBuilder<Stage> entity)
    {
        base.Configure(entity);

        entity.Property(e => e.Info).HasMaxLength(MaxLength.C512).IsRequired();
    }
}
