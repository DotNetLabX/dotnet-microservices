namespace ArticleTimeline.Persistence.EntityConfigurations;

public class TimelineTemplateEntityConfiguration : MetadataConfiguration<TimelineTemplate>
{
    public override void Configure(EntityTypeBuilder<TimelineTemplate> builder)
    {
				base.Configure(builder);

				builder.HasKey(e => new { e.SourceType, e.SourceId});

				builder.Property(e => e.SourceType).HasEnumConversion().IsRequired();
				builder.Property(e => e.SourceId).HasMaxLength(MaxLength.C64).IsRequired();
				builder.Property(e => e.TitleTemplate).HasMaxLength(MaxLength.C256).IsRequired();
    }
}
