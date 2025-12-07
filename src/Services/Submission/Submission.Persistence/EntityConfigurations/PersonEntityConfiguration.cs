namespace Submission.Persistence.EntityConfigurations;

public class PersonEntityConfiguration : EntityConfiguration<Person>
{
    protected override bool HasGeneratedId => false;

    public override void Configure(EntityTypeBuilder<Person> builder)
    {
        base.Configure(builder);

        builder.HasIndex(x => x.UserId).IsUnique();

        //insight about EF Core inheritance
        builder.HasDiscriminator(e => e.TypeDiscriminator)
            .HasValue<Person>(nameof(Person))
            .HasValue<Author>(nameof(Author));

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
                     .HasMaxLength(MaxLength.C64)
                     .UseCollation("SQL_Latin1_General_CP1_CI_AS"); // case insensitive for comparation
             });
    }
}
