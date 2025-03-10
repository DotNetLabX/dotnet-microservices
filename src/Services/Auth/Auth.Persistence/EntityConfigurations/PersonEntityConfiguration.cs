using Auth.Domain.Persons.ValueObjects;

namespace Auth.Persistence.EntityConfigurations;

internal class PersonEntityConfiguration : AuditedEntityConfiguration<Person>
{
		public override void Configure(EntityTypeBuilder<Person> builder)
		{
				base.Configure(builder);

				builder.Property(e => e.FirstName).HasMaxLength(MaxLength.C64).IsRequired();
				builder.Property(e => e.LastName).HasMaxLength(MaxLength.C64).IsRequired();
				builder.Property(e => e.Gender).HasEnumConversion().HasMaxLength(MaxLength.C64).IsRequired();

				// OwnsOne istead of ComplexProperty because EF.Core doesnt support yet indexes on ComplexxProperty
				builder.OwnsOne(
					 e => e.Email, b =>
					 {
							 b.Property(n => n.Value)
								.HasColumnName(nameof(Person.Email))
									 .HasMaxLength(MaxLength.C64);
							 b.Property(e => e.NormalizedEmail)
									 .HasColumnName(nameof(EmailAddress.NormalizedEmail))
									 .HasMaxLength(MaxLength.C64);
							 
							 b.HasIndex(e => e.NormalizedEmail).IsUnique();
					 });


				// OwnsOne istead of ComplexProperty because EF.Core doesnt support yet optional proparties with ComplexProperty
				builder.OwnsOne(
						e => e.Honorific, b =>
						{
								b.Property(e => e.Value).HasMaxLength(MaxLength.C32).HasColumnName(nameof(Person.Honorific));

								b.WithOwner(); // required to avoid navigation issues
						});


				builder.OwnsOne(e => e.ProfessionalProfile, b =>
				{
						b.Property(e => e.Position).HasMaxLength(MaxLength.C64).HasColumnName(nameof(ProfessionalProfile.Position));
						b.Property(e => e.CompanyName).HasMaxLength(MaxLength.C128).HasColumnNameSameAsProperty();
						b.Property(e => e.Affiliation).HasMaxLength(MaxLength.C256).HasColumnNameSameAsProperty();

						b.WithOwner(); // required to avoid navigation issues
				});

				builder.Property(e => e.PictureUrl).HasMaxLength(MaxLength.C2048);
		}
}
