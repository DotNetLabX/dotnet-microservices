namespace Review.Persistence.EntityConfigurations;

public class EditorEntityConfiguration : IEntityTypeConfiguration<Editor>
{
    public void Configure(EntityTypeBuilder<Editor> builder)
    {
				///builder.HasBaseType<Reviewer>();

				//builder.HasMany(e => e.JournalEditors).WithOne()
				//		.IsRequired()
				//		.OnDelete(DeleteBehavior.NoAction);
		}
}
