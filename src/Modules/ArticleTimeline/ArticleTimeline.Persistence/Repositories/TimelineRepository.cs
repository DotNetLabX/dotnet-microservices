using ArticleTimeline.Domain.Enums;

namespace ArticleTimeline.Persistence.Repositories;

public class TimelineRepository(ArticleTimelineDbContext dbContext) 
    : RepositoryBase<ArticleTimelineDbContext, Timeline>(dbContext)
{
    public virtual IQueryable<Domain.Timeline> GetByArticleId(int articleId, UserRoleType role,int lastTakenId,int take) 
    {
        return base.Query()
            .Join(_dbContext.TimelineVisibilities.Where(v => v.RoleType == role), 
            h => new { h.SourceType, h.SourceId }, 
            v => new { v.SourceType, v.SourceId }, 
            (h, v) => h)
            .OrderByDescending(x => x.Id)
            .Where(h => h.ArticleId == articleId&&(h.Id< lastTakenId|| lastTakenId==0))              
            .Take(take);
    }
    public virtual async Task<int> GetHistoryCountByRole(int articleId, UserRoleType role)
    {
        return await this.Entity
            .Join(_dbContext.TimelineVisibilities.Where(v => v.RoleType == role),
            h => new { h.SourceType, h.SourceId},
            v => new { v.SourceType, v.SourceId},
            (h, v) => h)
            .OrderByDescending(x => x.Id)
            .Where(h => h.ArticleId == articleId)
            .CountAsync();
    }

    public async Task<TimelineTemplate> GetTimelineTemplate(SourceType sourceType, string sourceId)
    {
        return _dbContext.GetAllCached<TimelineTemplate>()
            .FirstOrDefault(x => x.SourceType == sourceType && x.SourceId == sourceId);
    }

    public async Task<List<Timeline>> GetAllArticleHistoriesByArticleIdAsync(int articleId)
    {
        return this.Entity.Where(x => x.ArticleId == articleId).ToList();
    }

    public async Task DeleteArticleHistoriesAsync(List<Timeline> articleHistories)
    {
        if(articleHistories.Any())
        {
            _dbContext.Timelines.RemoveRange(articleHistories);
            await _dbContext.SaveChangesAsync();
        }
    }
}
