using Redis.OM;
using StackExchange.Redis;
namespace Blocks.Redis;

public class IntIdGenerationStrategy : IIdGenerationStrategy
{
		private readonly IDatabase _redisDatabase;

		public IntIdGenerationStrategy(IConnectionMultiplexer redis)
		{
				_redisDatabase = redis.GetDatabase();
		}

		public object GenerateId(Type entityType)
		{
				// Generate the Redis key based on the entity type name (e.g., "Journal:id")
				string redisKey = $"{entityType.Name.ToLower()}:id";

				// Increment the value stored in Redis for the generated key
				var newId = _redisDatabase.StringIncrement(redisKey);
				return (int)newId;
		}

		public string GenerateId()
		{
				throw new NotImplementedException();
		}

		public bool GenerateIdOnInsert => true;
}