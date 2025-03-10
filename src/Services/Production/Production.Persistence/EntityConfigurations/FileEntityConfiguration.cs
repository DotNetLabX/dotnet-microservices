namespace Production.Persistence.EntityConfigurations;

public class FileEntityConfiguration : AuditedEntityConfiguration<File>
{
    public override void Configure(EntityTypeBuilder<File> builder)
    {
        base.Configure(builder);

        builder.HasIndex(e => e.AssetId);

        builder.Property(e => e.FileServerId).HasMaxLength(MaxLength.C64);
        builder.Property(e => e.OriginalName).HasMaxLength(MaxLength.C256).HasComment("Original full file name, with extension");
        builder.Property(e => e.Size).HasComment("Size of the file in kilobytes");

				builder.ComplexProperty(
					 o => o.Extension, builder =>
					 {
							 builder.Property(n => n.Value)
									 .HasColumnName(builder.Metadata.PropertyInfo!.Name)
									 .HasMaxLength(MaxLength.C8);
					 });
				builder.ComplexProperty(
	         o => o.Name, builder =>
	         {
               builder.Property(n => n.Value)
                   .HasColumnName(builder.Metadata.PropertyInfo!.Name)
									 .HasMaxLength(MaxLength.C64).HasComment("Final name of the file after renaming");
					 });
				builder.ComplexProperty(
	         o => o.Version, builder =>
	         {
			         builder.Property(n => n.Value)
					         .HasColumnName(builder.Metadata.PropertyInfo!.Name)
					         .HasDefaultValue(1);
	         });

				//builder.Property(e => e.StatusId).HasConversion<int>();

        //builder.HasMany(e => e.FileActions).WithOne(e => e.File)
        //    .HasForeignKey(e => e.FileId).IsRequired()
        //    .OnDelete(DeleteBehavior.Cascade);
        
        //builder.HasOne(e => e.LatestAction).WithOne(e => e.File)
        //    .HasForeignKey<FileLatestAction>(e => e.FileId)
        //    .IsRequired()
        //    .OnDelete(DeleteBehavior.Cascade);

    }
}
