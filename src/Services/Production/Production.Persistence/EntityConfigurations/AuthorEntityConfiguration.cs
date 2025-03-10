using Blocks.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Production.Domain.Assets;

namespace Production.Persistence.EntityConfigurations;

public class AuthorEntityConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
				builder.Property(e => e.Affiliation).IsRequired().HasMaxLength(MaxLength.C512)
						.HasComment("Institution or organization they are associated with when they conduct their research.");
    }
}
