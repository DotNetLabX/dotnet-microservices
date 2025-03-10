using Microsoft.Extensions.Options;

namespace Blocks.EntityFrameworkCore;

internal class TenantDbContext : DbContext, IMultitenancy
{
		public int TenantId { get; set; }

		public TenantDbContext(DbContextOptions<TenantDbContext> options, IOptions<TenantOptions> tenantOptions)
				: base(options)
		{
				TenantId = tenantOptions.Value.TenantId;
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
				SetTenantId();
				return base.SaveChangesAsync(cancellationToken);
		}

		private void SetTenantId()
		{
				if (!TenantId.Equals(0))
				{
						//this.ChangeTracker.DetectChanges();
						var entries = this.ChangeTracker.Entries()
								.Where(t => t.State == EntityState.Added);

						foreach (var entry in entries)
						{
								if (entry.Entity is IMultitenancy)
								{
										((IMultitenancy)entry.Entity).TenantId = TenantId;
								}
						}
				}
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
				foreach (var entityType in modelBuilder.Model.GetEntityTypes())
				{
						if (typeof(IMultitenancy).IsAssignableFrom(entityType.ClrType))
						{
								var entityBuilder = modelBuilder.Entity(entityType.ClrType);
								entityBuilder.AddQueryFilter<IMultitenancy>(e => e.TenantId.Equals(TenantId));
						}
				}

				base.OnModelCreating(modelBuilder);
		}
}
