using Blocks.Core;

namespace Blocks.EntityFrameworkCore;

public abstract class EnumEntityConfiguration<T, TEnum> : EntityConfiguration<T, TEnum>
    where T : EnumEntity<TEnum>
    where TEnum : struct, Enum
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.Name).HasEnumConversion()
            .IsRequired().HasMaxLength(MaxLength.C64).HasColumnOrder(1);
        builder.Property(e => e.Description)
            .IsRequired().HasMaxLength(MaxLength.C64).HasColumnOrder(2);
    }
}