using Submission.Persistence.EntityConfigurations;
using Microsoft.Extensions.Caching.Memory;

namespace Submission.Persistence;

public partial class SubmissionDbContext(DbContextOptions<SubmissionDbContext> options, IMemoryCache cache)
    : ApplicationDbContext<SubmissionDbContext>(options, cache)
{
    #region Entities
    public virtual DbSet<Article> Articles { get; set; }
		public virtual DbSet<ArticleActor> ArticleActors { get; set; }
		public virtual DbSet<Asset> Assets { get; set; }
    public virtual DbSet<AssetTypeDefinition> AssetTypes { get; set; }
    public virtual DbSet<Journal> Journals { get; set; }
    public virtual DbSet<Stage> Stages { get; set; }
    public virtual DbSet<StageHistory> StageHistories { get; set; }
		public virtual DbSet<Person> Persons { get; set; }
		public virtual DbSet<Author> Authors { get; set; }
		#endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
				modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

        modelBuilder.UseEntityTypeNamesAsTables();

				base.OnModelCreating(modelBuilder);
    }

		public async override Task<int> SaveChangesAsync(CancellationToken ct = default)
		{
				this.UnTrackCacheableEntities();

				return await base.SaveChangesAsync(ct);
		}
}