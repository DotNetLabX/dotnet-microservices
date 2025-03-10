using Blocks.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Production.Domain.Shared;
using Production.Domain.Assets;

namespace Production.Persistence.EntityConfigurations;

public class JournalEntityConfiguration : EntityConfiguration<Journal>
{
    public override void Configure(EntityTypeBuilder<Journal> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Abbreviation).HasMaxLength(MaxLength.C8).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(MaxLength.C64).IsRequired();
        //entity.Property(e => e.ShortName).HasMaxLength(Constraints.Twenty).IsRequired();
        
        builder.HasOne(e => e.DefaultTypesetter).WithMany()
            .HasForeignKey(e => e.DefaultTypesetterId)
            .HasPrincipalKey(e=> e.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
