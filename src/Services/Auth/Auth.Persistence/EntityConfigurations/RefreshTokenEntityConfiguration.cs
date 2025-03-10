namespace Auth.Persistence;

internal class RefreshTokenEntityConfiguration : EntityConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.CreatedByIp).HasMaxLength(MaxLength.C128);
        builder.Property(e => e.RevokedByIp).HasMaxLength(MaxLength.C128);
		}
}
