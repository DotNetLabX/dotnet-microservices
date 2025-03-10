namespace ArticleHub.Persistence.EntityConfigurations;

public class PersonEntityConfiguration : EntityConfiguration<Person>
{
		protected override bool HasGeneratedId => false;

		public override void Configure(EntityTypeBuilder<Person> builder)
    {
        base.Configure(builder);

				builder.HasIndex(x => x.UserId).IsUnique();

				builder.Property(e => e.UserId).IsRequired(false);
				builder.Property(e => e.FirstName).HasMaxLength(MaxLength.C64).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(MaxLength.C64).IsRequired();
        builder.Property(e => e.Honorific).HasMaxLength(MaxLength.C64);
        builder.Property(e => e.Email).HasMaxLength(MaxLength.C256).IsRequired();
		}
}
