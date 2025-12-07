using Blocks.Core;
using Blocks.Exceptions;
using System.Linq.Expressions;

namespace Blocks.EntityFrameworkCore;

public static class RepositoryExtensions
{
    // talk about why is better to create an extension method in this case.
    public static async Task<TEntity> FindByIdOrThrowAsync<TEntity, TContext>(this RepositoryBase<TContext, TEntity> repository, int id)
        where TContext : DbContext
        where TEntity : class, IEntity<int>
        => Guard.NotFound(await repository.FindByIdAsync(id));

    public static async Task<TEntity> FindByIdOrThrowAsync<TEntity>(this DbSet<TEntity> dbSet, int id)
        where TEntity : class, IEntity<int>
        => Guard.NotFound(await dbSet.FindAsync(id));

    //insight about the difference between FindByIdAsync and GetByIdAsync
    public static async Task<TEntity> GetByIdOrThrowAsync<TEntity, TContext>(this RepositoryBase<TContext, TEntity> repository, int id, CancellationToken ct = default)
        where TContext : DbContext
        where TEntity : class, IEntity<int>
        => Guard.NotFound(await repository.GetByIdAsync(id));
    
    public static async Task<TEntity> SingleOrThrowAsync<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        where TEntity : class, IEntity<int>
        => Guard.NotFound(await source.SingleOrDefaultAsync(predicate, ct));

    public static async Task ExistsOrThrowAsync<TEntity, TContext>(this RepositoryBase<TContext, TEntity> repository, int id, CancellationToken ct = default)
        where TContext : DbContext
        where TEntity : class, IEntity<int>
    {
        if(!await repository.ExistsAsync(id, ct))
            throw new NotFoundException($"{typeof(TEntity).Name}({id}) doesn't exist");
    }

    public static async Task EnsureNotExistsOrThrowAsync<TEntity, TContext>(this RepositoryBase<TContext, TEntity> repository, int id, CancellationToken ct = default)
        where TContext : DbContext
        where TEntity : class, IEntity<int>
    {
        if (await repository.ExistsAsync(id, ct))
            throw new BadRequestException($"{typeof(TEntity).Name}({id}) already exists");
    }
}
