namespace Blocks.EntityFrameworkCore;

public abstract class TenantEntityConfiguration<T> : IEntityTypeConfiguration<T> 
    where T : TenantEntity
{
		public void Configure(EntityTypeBuilder<T> entity)
    {
        entity.Property(e => e.TenantId);
        entity.HasKey(e => new { e.TenantId, e.Id });
    }

		protected virtual string EntityName => typeof(T).Name;
}