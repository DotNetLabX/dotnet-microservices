using EFCore.NamingConventions.Internal;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace ArticleHub.Persistence;

public partial class ArticleHubDbContext(DbContextOptions<ArticleHubDbContext> options, IMemoryCache cache)
		: ApplicationDbContext<ArticleHubDbContext>(options, cache)
{
		#region Entities
		public virtual DbSet<Article> Articles { get; set; }
		public virtual DbSet<Journal> Journals { get; set; }
		public virtual DbSet<Person> Persons { get; set; }
		#endregion

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
				// this is the right place to configure the naming convention, but it doesn't work with UseEntityTypeNamesAsTables.
				// so it is better to have both policies in OnModelCreating.
				//optionsBuilder
				//		.UseSnakeCaseNamingConvention();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
				modelBuilder.UseEntityTypeNamesAsTables(new SnakeCaseNameRewriter(CultureInfo.InvariantCulture));

				modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

				base.OnModelCreating(modelBuilder);
		}

		public async override Task<int> SaveChangesAsync(CancellationToken ct = default)
		{
				this.UnTrackCacheableEntities(); 

				return await base.SaveChangesAsync(ct);
		}
}
