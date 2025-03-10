using Blocks.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Production.Domain.Assets;

namespace Production.Persistence.EntityConfigurations;

public class StageHistoryEntityConfiguration : EntityConfiguration<StageHistory>
{
    public override void Configure(EntityTypeBuilder<StageHistory> entity)
    {
        base.Configure(entity);

        entity.HasIndex(e => e.ArticleId);

        entity.Property(e => e.StartDate).IsRequired();

        entity.HasOne(e => e.Stage).WithMany().HasForeignKey(e => e.StageId).OnDelete(DeleteBehavior.Restrict);        
    }
}
