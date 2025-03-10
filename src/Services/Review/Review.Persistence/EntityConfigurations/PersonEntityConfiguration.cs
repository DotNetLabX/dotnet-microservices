namespace Review.Persistence.EntityConfigurations;

public class PersonEntityConfiguration : EntityConfiguration<Person>
{
		protected override bool HasGeneratedId => false;

		public override void Configure(EntityTypeBuilder<Person> builder)
    {
        base.Configure(builder);

				builder.HasIndex(x => x.UserId).IsUnique();


				//builder.HasIndex(p => new { p.Email.Value, p.TypeDiscriminator }).IsUnique();
				// using Raw SQL here because at this moment we cannot use a value object to create a composite index
				builder.HasAnnotation(
						"RawSql:Index",
						"CREATE UNIQUE INDEX IX_Person_Email_TypeDiscriminator ON Person (Email, TypeDiscriminator)");

				//insight about EF Core inheritance
				builder.HasDiscriminator(e => e.TypeDiscriminator)
						.HasValue<Person>(nameof(Person))
						.HasValue<Author>(nameof(Author))
						.HasValue<Reviewer>(nameof(Reviewer))
						.HasValue<Editor>(nameof(Editor));

				builder.Property(e => e.UserId).IsRequired(false);
				builder.Property(e => e.FirstName).IsRequired().HasMaxLength(MaxLength.C64);
				builder.Property(e => e.LastName).IsRequired().HasMaxLength(MaxLength.C64);
				builder.Property(e => e.Honorific).HasMaxLength(MaxLength.C32);
				builder.Property(e => e.Affiliation).IsRequired().HasMaxLength(MaxLength.C512)
						.HasComment("Institution or organization they are associated with when they conduct their research.");

				builder.ComplexProperty(
					 o => o.Email, builder =>
					 {
							 builder.Property(n => n.Value)
									 .HasColumnName(builder.Metadata.PropertyInfo!.Name)
									 .HasMaxLength(MaxLength.C64);
					 });
		}
}
