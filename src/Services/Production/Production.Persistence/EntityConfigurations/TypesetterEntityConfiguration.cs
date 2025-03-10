using Blocks.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Production.Domain.Assets;

namespace Production.Persistence.EntityConfigurations;

public class TypesetterEntityConfiguration : IEntityTypeConfiguration<Typesetter>
{
    public void Configure(EntityTypeBuilder<Typesetter> entity)
    {
        //base.Configure(entity);

        //insight - small table, practically we don't need an index, teoretically we may add it
        //entity.HasKey(e => e.UserId);
        
        entity.Property(e => e.IsDefault).HasDefaultValue(false);
        entity.Property(e => e.CompanyName).HasMaxLength(MaxLength.C64);
        
        //todo - investigate ignore and maybe talk about it
        //entity.Ignore(t => t.Id);
    }
}
