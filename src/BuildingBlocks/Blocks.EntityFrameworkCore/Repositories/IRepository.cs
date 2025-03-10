using System.Linq.Expressions;

namespace Blocks.EntityFrameworkCore;

public interface IRepository<TEntity> : IRepository<TEntity, int>
		where TEntity : class, IEntity<int>;


// [Course.AdvancedC#]
public interface IRepository<TEntity, TKey>
		where TEntity : class, IEntity<TKey>
		where TKey : struct
{
		// Core
		Task<TEntity?> GetByIdAsync(TKey id, CancellationToken ct = default);
		Task<bool> ExistsAsync(TKey id, CancellationToken ct = default);
		Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
		Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);
		TEntity Update(TEntity entity);
		void Remove(TEntity entity);
		Task<bool> DeleteByIdAsync(TKey id, CancellationToken ct = default);

		// Batching
		Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
		void UpdateRange(IEnumerable<TEntity> entities);
		void RemoveRange(IEnumerable<TEntity> entities);

		// Unit of work bits
		Task<int> SaveChangesAsync(CancellationToken ct = default);
		void ClearTracking();

		// Query surface (let callers build Where/Select/etc.)
		IQueryable<TEntity> Query();
		IQueryable<TEntity> QueryNotTracked();
}
