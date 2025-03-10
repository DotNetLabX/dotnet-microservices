using Production.Domain.Assets;

namespace Production.Persistence.EntityConfigurations;

public class AssetTypeDefinitionEntityConfiguration : EnumEntityConfiguration<AssetTypeDefinition, AssetType>
{
    public override void Configure(EntityTypeBuilder<AssetTypeDefinition> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.DefaultCategoryId);
				builder.Property(e => e.MaxNumber).HasDefaultValue(0);

        builder.Property(e => e.DefaultFileExtension).HasDefaultValue("pdf").IsRequired().HasMaxLength(MaxLength.C8);

				builder.ComplexProperty(e => e.AllowedFileExtensions, builder =>
				{
						var convertor = BuilderExtensions.BuildJsonReadOnlyListConvertor<string>();
						builder.Property(e => e.Extensions)
								.HasConversion(convertor)
								.HasColumnName(builder.Metadata.PropertyInfo!.Name)
								.IsRequired();
				});
		}
}
