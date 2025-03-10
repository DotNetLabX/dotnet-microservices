namespace Review.Persistence.EntityConfigurations;

public class ReviewInvitationEntityConfiguration : EntityConfiguration<ReviewInvitation>
{
		public override void Configure(EntityTypeBuilder<ReviewInvitation> builder)
		{
				base.Configure(builder);

				builder.Property(e => e.UserId).IsRequired(false);
				builder.Property(e => e.FirstName).IsRequired().HasMaxLength(MaxLength.C64);
				builder.Property(e => e.LastName).IsRequired().HasMaxLength(MaxLength.C64);				
				builder.Property(e => e.Status).IsRequired().HasEnumToStringConversion(MaxLength.C8);

				builder.ComplexProperty(
					 o => o.Email, builder =>
					 {
							 builder.Property(n => n.Value)
									 .HasColumnName(builder.Metadata.PropertyInfo!.Name)
									 .HasMaxLength(MaxLength.C64);
					 });

				builder.ComplexProperty(
					 o => o.Token, builder =>
					 {
							 builder.Property(n => n.Value)
									 .HasColumnName(builder.Metadata.PropertyInfo!.Name)
									 .HasMaxLength(MaxLength.C64);
					 });

				builder.HasOne(e => e.SentBy).WithMany().HasForeignKey(e => e.SentById);
		}
}
