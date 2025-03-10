using Redis.OM;
using Redis.OM.Aggregation;
using Redis.OM.Searching;
using StackExchange.Redis;

namespace Blocks.Redis;

public class Repository<T>
		where T : Entity
{
		private readonly IRedisCollection<T> _collection;
		private readonly RedisConnectionProvider _provider;
		private readonly IDatabase _redisDb;

		public Repository(IConnectionMultiplexer redis, RedisConnectionProvider provider) =>
				(_redisDb, _provider, _collection) = (redis.GetDatabase(), provider, provider.RedisCollection<T>());

		public IRedisCollection<T> Collection => _collection;

		public RedisAggregationSet<T> Aggregate => _provider.AggregationSet<T>();

		public T? GetById(int id) => _collection.FindById(id.ToString());
		public async Task<T?> GetByIdAsync(int id) => await _collection.FindByIdAsync(id.ToString());
		public async Task<T> GetByIdOrThrowAsync(int id) => await _collection.GetByIdOrThrowAsync(id);

		public async Task<bool> Exists(int id) => await _collection.AnyAsync(e =>e.Id == id);

		public async Task<IEnumerable<T>> GetAllAsync() => await _collection.ToListAsync();

		public async Task AddAsync(T entity)
		{
				if (entity.Id < 0)
						throw new InvalidOperationException($"Invalid ID: {entity.Id}. IDs must be positive or zero.");

				if (entity.Id == 0)
						entity.Id = await GenerateNewId();

				await _collection.InsertAsync(entity);
		}

		public async Task UpdateAsync(T entity) => await _collection.UpdateAsync(entity);

		public async Task ReplaceAsync(T entity)
		{
				//this is a workaround for Redis.OM not properly updating child collections (e.g Sections in Journal)
				await _collection.DeleteAsync(entity);
				await _collection.InsertAsync(entity);
		}

		public async Task DeleteAsync(T entity) => await _collection.DeleteAsync(entity);

		public async Task<int> GenerateNewId() => (int) await _redisDb.StringIncrementAsync($"{typeof(T).Name}:Id:Sequence");
		public async Task<int> GenerateNewId<TOther>() => (int)await _redisDb.StringIncrementAsync($"{typeof(TOther).Name}:Id:Sequence");
		public async Task<int> SetNewId(Entity entity) => entity.Id = (int)await _redisDb.StringIncrementAsync($"{entity.GetType().Name}:Id:Sequence");
}
