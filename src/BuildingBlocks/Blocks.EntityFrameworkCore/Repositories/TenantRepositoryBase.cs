using Blocks.Exceptions;
using System.Net;

namespace Blocks.EntityFrameworkCore;


public abstract class TenantRepositoryBase<TContext, TEntity, TKey> : RepositoryBase<TContext, TEntity, TKey>
		where TContext : DbContext
		where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    IMultitenancy _multitenancy;

    protected TenantRepositoryBase(TContext context, IMultitenancy multitenancy) : base(context)
    {
        _multitenancy = multitenancy;
    }

    public virtual TEntity GetById(TKey id, bool throwNotFound = true)
    {
        var entity = Entity.Find(_multitenancy.TenantId, id);
        if (throwNotFound == true && entity == null)
            throw new HttpException(HttpStatusCode.NotFound, "Not found");
        return entity;
    }

    //public TEntity? GetOrDefault(TKey id) => Query().SingleOrDefault(e => e.Id == id);

    public virtual async Task<TEntity> GetAsync(TKey id)
    {
        return await _dbContext.Set<TEntity>().FindAsync(_multitenancy.TenantId, id);
    }

}
