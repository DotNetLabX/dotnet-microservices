namespace ArticleHub.Persistence.EntityConfigurations;

public class JournalEntityConfiguration : EntityConfiguration<Journal>
{
		protected override bool HasGeneratedId => false;
		public override void Configure(EntityTypeBuilder<Journal> entity)
    {
        base.Configure(entity);

        entity.Property(e => e.Abbreviation).HasMaxLength(MaxLength.C16).IsRequired();
        entity.Property(e => e.Name).HasMaxLength(MaxLength.C64).IsRequired();
    }
}
