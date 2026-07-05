// {Svc}.Persistence/EntityConfigurations/*.cs — EF-backed variant.
// The configuration ladder does the boilerplate; you supply only the aggregate-specific mapping.
//   EntityConfiguration<T>        -> key + generated-id toggle + JSON-file seeding (free in base Configure)
//   AuditedEntityConfiguration<T> -> audit columns MANDATORY (constrained to IAggregateRoot);
//                                    RowVersion concurrency token is opt-in, default OFF (persistence-2)
//   EnumEntityConfiguration<T,E>  -> unique Name index + Name/Description columns for enum entities
// Lifted from Submission.Persistence/EntityConfigurations/{ArticleEntityConfiguration, StageEntityConfiguration}.cs.

namespace {Svc}.Persistence.EntityConfigurations;

// Aggregate root -> AuditedEntityConfiguration. Always call base.Configure(builder) FIRST.
public class {Aggregate}EntityConfiguration : AuditedEntityConfiguration<{Aggregate}>
{
    public override void Configure(EntityTypeBuilder<{Aggregate}> builder)
    {
        base.Configure(builder);   // key, id, audit columns, seeding

        builder.Property(e => e.Title).HasMaxLength(MaxLength.C256).IsRequired();   // lengths from MaxLength.* (conventions-4)
        builder.Property(e => e.Stage).HasEnumConversion().IsRequired();

        builder.HasMany(e => e.{Child}s).WithOne()
            .HasForeignKey(e => e.{Aggregate}Id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);   // children die with the aggregate
    }
}

// Enum-metadata entity -> EnumEntityConfiguration. base.Configure adds the unique Name index + columns.
public class {EnumEntity}EntityConfiguration : EnumEntityConfiguration<{EnumEntity}, {Enum}>
{
    public override void Configure(EntityTypeBuilder<{EnumEntity}> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.MaxAssetCount).HasDefaultValue(0);
    }
}
