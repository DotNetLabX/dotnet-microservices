namespace Submission.Persistence.EntityConfigurations;

public class StageHistoryEntityConfiguration : EntityConfiguration<StageHistory>
{
    public override void Configure(EntityTypeBuilder<StageHistory> builder)
    {
        base.Configure(builder);

        builder.HasIndex(e => e.ArticleId);

        builder.Property(e => e.StartDate).IsRequired();

        builder.HasOne<Stage>().WithMany().HasForeignKey(e => e.StageId).OnDelete(DeleteBehavior.Restrict);        
    }
}
