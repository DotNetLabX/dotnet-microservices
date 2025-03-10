using Microsoft.Extensions.Caching.Memory;
using ArticleTimeline.Persistence.EntityConfigurations;

namespace ArticleTimeline.Persistence;

public partial class ArticleTimelineDbContext //(DbContextOptions<ArticleTimelineDbContext> options, IMemoryCache cache)
    : ApplicationDbContext<ArticleTimelineDbContext>//(options, cache)
{
    public ArticleTimelineDbContext(DbContextOptions<ArticleTimelineDbContext> options, IMemoryCache cache)
        :base(options, cache)
    {
            
    }
    #region Entities
    public virtual DbSet<Timeline> Timelines { get; set; }
		public virtual DbSet<TimelineTemplate> TimelineSourceConfigurations { get; set; }
		public virtual DbSet<TimelineVisibility> TimelineVisibilities { get; set; }
		#endregion


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ArticleTimeline");

				modelBuilder.ApplyConfiguration(new TimelineEntityConfiguration());
				modelBuilder.ApplyConfiguration(new TimelineTemplateEntityConfiguration());
				modelBuilder.ApplyConfiguration(new TimelineVisibilityEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}