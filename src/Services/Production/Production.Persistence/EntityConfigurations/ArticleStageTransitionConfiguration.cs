using Production.Domain.Assets;

namespace Production.Persistence.EntityConfigurations;

internal class ArticleStageTransitionConfiguration : MetadataConfiguration<ArticleStageTransition>
{
		public override void Configure(EntityTypeBuilder<ArticleStageTransition> builder)
		{
				base.Configure(builder);

				builder.HasKey(e => new { e.CurrentStage, e.ActionType, e.DestinationStage });

				builder.Property(e => e.CurrentStage).IsRequired().HasEnumConversion();
				builder.Property(e => e.DestinationStage).IsRequired().HasEnumConversion();
				builder.Property(e => e.ActionType).IsRequired().HasEnumConversion();
		}
}
