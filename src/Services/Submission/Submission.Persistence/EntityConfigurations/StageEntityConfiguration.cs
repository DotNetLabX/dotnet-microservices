namespace Submission.Persistence.EntityConfigurations;

public class StageEntityConfiguration : EnumEntityConfiguration<Stage, ArticleStage>
{
    public override void Configure(EntityTypeBuilder<Stage> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Info).HasMaxLength(MaxLength.C512).IsRequired();
    }
}
