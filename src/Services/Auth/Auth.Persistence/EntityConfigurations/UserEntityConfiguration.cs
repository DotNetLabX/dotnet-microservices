namespace Auth.Persistence.EntityConfigurations;

internal class UserEntityConfiguration : AuditedEntityConfiguration<User>
{
		public override void Configure(EntityTypeBuilder<User> builder)
		{
				base.Configure(builder);

				builder.HasMany(p => p.UserRoles).WithOne().HasForeignKey(p => p.UserId)
						.IsRequired().OnDelete(DeleteBehavior.Cascade);
				builder.HasMany(p => p.RefreshTokens).WithOne().HasForeignKey(p => p.UserId)
						.IsRequired().OnDelete(DeleteBehavior.Cascade);

				builder.HasOne(u => u.Person).WithOne(p => p.User)
						.HasForeignKey<User>(u => u.PersonId)
						.IsRequired()
						.OnDelete(DeleteBehavior.Restrict);
		}
}
