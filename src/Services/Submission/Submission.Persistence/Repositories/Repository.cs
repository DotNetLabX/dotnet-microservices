using Blocks.Entities;

namespace Submission.Persistence.Repositories;

public class Repository<TEntity>(SubmissionDbContext dbContext) 
    : RepositoryBase<SubmissionDbContext, TEntity>(dbContext)
        where TEntity : class, IEntity<int>
{
}
