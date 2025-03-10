namespace ArticleTimeline.Persistence.EntityConfigurations;

public class TimelineVisibilityEntityConfiguration : MetadataConfiguration<TimelineVisibility>
{
    public override void Configure(EntityTypeBuilder<TimelineVisibility> builder)
    {
				base.Configure(builder);
				
				builder.HasKey(e => new { e.SourceType, e.SourceId, e.RoleType });

				builder.Property(e => e.SourceType).HasEnumConversion().IsRequired();
				builder.Property(e => e.SourceId).HasMaxLength(MaxLength.C64).IsRequired();
				builder.Property(e => e.RoleType).HasEnumConversion().IsRequired();
    }
}
