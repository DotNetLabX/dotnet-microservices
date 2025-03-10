using Production.Persistence.EntityConfigurations;
using Microsoft.Extensions.Caching.Memory;
using Production.Domain.Assets;
using Production.Domain.Shared;

namespace Production.Persistence;

public partial class ProductionDbContext(DbContextOptions<ProductionDbContext> options, IMemoryCache cache)
    : ApplicationDbContext<ProductionDbContext>(options, cache)
{

    #region Entities
    public virtual DbSet<Article> Articles { get; set; }

		public virtual DbSet<ArticleContributor> ArticleContributors { get; set; }
		public virtual DbSet<Asset> Assets { get; set; }
    public virtual DbSet<AssetTypeDefinition> AssetTypes { get; set; }

    //public virtual DbSet<FileAction> FileActions { get; set; }

    public virtual DbSet<Journal> Journals { get; set; }
    public virtual DbSet<Stage> Stages { get; set; }
    public virtual DbSet<StageHistory> StageHistories { get; set; }
		public virtual DbSet<Person> Persons { get; set; }
		public virtual DbSet<Author> Authors { get; set; }
		public virtual DbSet<Typesetter> Typesetters { get; set; }
		#endregion


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

				//todo use the following line:
				//modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

				modelBuilder.ApplyConfiguration(new ArticleEntityConfiguration());
				modelBuilder.ApplyConfiguration(new ArticleStageTransitionConfiguration());
				modelBuilder.ApplyConfiguration(new AssetEntityConfiguration());
				modelBuilder.ApplyConfiguration(new AssetStateTransitionConfiguration());
				modelBuilder.ApplyConfiguration(new AssetStateTransitionConditionConfiguration());
				modelBuilder.ApplyConfiguration(new AssetCurrentFileLinkEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AssetActionEntityConfiguration());
				modelBuilder.ApplyConfiguration(new AssetTypeDefinitionEntityConfiguration());
				modelBuilder.ApplyConfiguration(new ArticleContributorEntityConfiguration());
				modelBuilder.ApplyConfiguration(new AuthorEntityConfiguration());
        modelBuilder.ApplyConfiguration(new FileEntityConfiguration());
        modelBuilder.ApplyConfiguration(new JournalEntityConfiguration());
        modelBuilder.ApplyConfiguration(new StageEntityConfiguration());
        modelBuilder.ApplyConfiguration(new StageHistoryEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TypesetterEntityConfiguration());
				modelBuilder.ApplyConfiguration(new PersonEntityConfiguration());

        //modelBuilder.ApplyConfiguration(new CustomerEntityConfiguration());


        modelBuilder.UseEntityTypeNamesAsTables();


				base.OnModelCreating(modelBuilder);
    }

		public async override Task<int> SaveChangesAsync(CancellationToken ct = default)
		{
				this.UnTrackCacheableEntities();

				return await base.SaveChangesAsync(ct);
		}
}