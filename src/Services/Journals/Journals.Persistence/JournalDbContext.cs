namespace Journals.Persistence;

public class JournalDbContext
{
		private readonly RedisConnectionProvider _provider;
		private readonly IDatabase _redisDb;

		public JournalDbContext(IConnectionMultiplexer redis, RedisConnectionProvider provider) =>
				(_redisDb, _provider)= (redis.GetDatabase(), provider);

		public IRedisCollection<Journal> Journals => _provider.RedisCollection<Journal>();
		//public IRedisCollection<Section> Sections => _provider.RedisCollection<Section>();
		public IRedisCollection<Editor> Editors => _provider.RedisCollection<Editor>();

		public RedisConnectionProvider Provider => _provider;

		public async Task<int> GenerateNewId<T>() => (int)await _redisDb.StringIncrementAsync($"{typeof(T).Name}:Id:Sequence");
}
