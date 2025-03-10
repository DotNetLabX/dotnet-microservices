namespace Blocks.EntityFrameworkCore;

public abstract class AuditedEntityConfiguration<T> : AuditedEntityConfiguration<T, int>
	  where T : class, IAggregateRoot
{
		protected virtual bool HasGeneratedId => true;
		public override void Configure(EntityTypeBuilder<T> builder)
		{
				if (HasGeneratedId)
						builder.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnOrder(0);
				else
						builder.Property(e => e.Id).ValueGeneratedNever().HasColumnOrder(0);
				
				base.Configure(builder);
		}
}

public abstract class AuditedEntityConfiguration<T, TKey> : EntityConfiguration<T, TKey> 
    where T : class, IEntity<TKey>, IAggregateRoot<TKey>
    where TKey : struct
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.CreatedOn).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(e => e.CreatedById).IsRequired();
        builder.Property(e => e.LastModifiedOn);
        builder.Property(e => e.LastModifiedById);
    }
}