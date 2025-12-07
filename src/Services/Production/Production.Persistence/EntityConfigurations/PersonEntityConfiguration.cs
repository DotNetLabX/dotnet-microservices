using Blocks.EntityFrameworkCore;
using Blocks.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Production.Domain.Assets;
using Production.Domain.Shared;

namespace Production.Persistence.EntityConfigurations;

public class PersonEntityConfiguration : EntityConfiguration<Person>
{
    public override void Configure(EntityTypeBuilder<Person> entity)
    {
        base.Configure(entity);

        entity.HasIndex(x => x.UserId).IsUnique();

        //insight about EF Core inheritance
        entity.HasDiscriminator(e => e.TypeDiscriminator)
            .HasValue<Person>(nameof(Person))
            .HasValue<Author>(nameof(Author))
            .HasValue<Typesetter>(nameof(Typesetter));

        entity.Property(e => e.UserId).IsRequired(false);
        entity.Property(e => e.FirstName).HasMaxLength(MaxLength.C64).IsRequired();
        entity.Property(e => e.LastName).HasMaxLength(MaxLength.C64).IsRequired();
        entity.Property(e => e.Title).HasMaxLength(MaxLength.C64);
        entity.Property(e => e.Email).HasMaxLength(MaxLength.C256).IsRequired();

        //entity.HasOne(p => p.User)
        //    .WithOne(u => u.Person)
        //    .HasForeignKey<Person>(p => p.UserId)
        //    .OnDelete(DeleteBehavior.SetNull);
    }
}
