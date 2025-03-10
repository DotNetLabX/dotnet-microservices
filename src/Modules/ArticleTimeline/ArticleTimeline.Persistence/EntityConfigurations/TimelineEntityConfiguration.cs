namespace ArticleTimeline.Persistence.EntityConfigurations;

public class TimelineEntityConfiguration : EntityConfiguration<Timeline>
{
    public override void Configure(EntityTypeBuilder<Timeline> entity)
    {
				base.Configure(entity);

				entity.Property(e => e.CurrentStage).HasEnumConversion().IsRequired();
				entity.Property(e => e.NewStage).HasEnumConversion().IsRequired();
				entity.Property(e => e.SourceType).HasEnumConversion().IsRequired();
        entity.Property(e => e.Title).HasMaxLength(MaxLength.C256).IsRequired();
        entity.Property(e => e.Description).IsRequired();

				entity.HasOne(e => e.Template).WithMany()
						.HasForeignKey(e => new { e.SourceType, e.SourceId})
						.IsRequired();
		}
}
