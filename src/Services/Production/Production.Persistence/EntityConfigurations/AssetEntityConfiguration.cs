using Production.Domain.Assets;

namespace Production.Persistence.EntityConfigurations;

internal class AssetEntityConfiguration : AuditedEntityConfiguration<Asset>
{
    public override void Configure(EntityTypeBuilder<Asset> builder)
    {
        base.Configure(builder);

        //insight - simple vs composite indexes
        // we don't need the following indexes because we are always taking the assets based on the articleId
        //entity.HasIndex(e => e.StatusId);
        //entity.HasIndex(e => e.TypeId);

				builder.ComplexProperty(
	         o => o.Name, builder =>
	         {
			         builder.Property(n => n.Value)
					         .HasColumnName(builder.Metadata.PropertyInfo!.Name)
					         .HasMaxLength(MaxLength.C64).IsRequired();
	         });

				builder.ComplexProperty(
	         o => o.Number, builder =>
	         {
			         builder.Property(n => n.Value)
					         .HasColumnName(builder.Metadata.PropertyInfo!.Name)
                   .HasDefaultValue(0).IsRequired();
	         });

				builder.Property(e => e.State).HasEnumConversion().IsRequired();
        builder.Property(e => e.CategoryId).HasConversion<int>().HasDefaultValue(AssetCategory.Core).IsRequired();
        builder.Property(e => e.Type).HasEnumConversion().IsRequired();

        builder.HasOne(d => d.Article).WithMany(p => p.Assets)
            .HasForeignKey(d => d.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.TypeDefinition).WithMany()
            .HasForeignKey(e => e.Type)
            .HasPrincipalKey(e => e.Name)
            .IsRequired();

        builder.HasMany(e => e.Files).WithOne(e => e.Asset)
            .HasForeignKey(e => e.AssetId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CurrentFileLink).WithOne(e => e.Asset)
            .HasForeignKey<AssetCurrentFileLink>(e => e.AssetId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
