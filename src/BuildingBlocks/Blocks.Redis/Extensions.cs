using Blocks.Exceptions;
using Newtonsoft.Json;
using Redis.OM;
using Redis.OM.Searching;
using StackExchange.Redis;

namespace Blocks.Redis;

public static class Extensions
{
		public static async Task<T?> GetByIdAsync<T>(this IRedisCollection<T> collection, int id)
				=> await collection.FindByIdAsync(id.ToString());
		public static async Task<T> GetByIdOrThrowAsync<T>(this IRedisCollection<T> collection, int id)
		{
				var entity = await collection.FindByIdAsync(id.ToString());
				if (entity is null)
						throw new NotFoundException($"{typeof(T).Name} not found");
				return entity!;
		}

		public static async Task<int> GenerateNewId<TEntity>(this IDatabase redisDb) where TEntity : Entity
				=> (int)await redisDb.StringIncrementAsync($"{typeof(TEntity).Name}:Id:Sequence");

		public static async Task<bool> SetSequenceSeed<TEntity>(this IDatabase redisDb, int startValue = 0) where TEntity : Entity
				=> await redisDb.StringSetAsync($"{typeof(TEntity).Name}:Id:Sequence", startValue);


		public static async Task SeedFromJson<TEntity>(this RedisConnectionProvider provider, IDatabase redisDb, string folderPath = "Data/Test")
				where TEntity : Entity
		{
				var collection = provider.RedisCollection<TEntity>();
				if (collection.Any())
						return;

				var filePath = $"{AppContext.BaseDirectory}{folderPath}/{typeof(TEntity).Name}.json";
				if (!File.Exists(filePath))
						return;

				var entities = JsonConvert.DeserializeObject<TEntity[]>(File.ReadAllText(filePath));
				if (entities != null)
				{
						foreach (var entity in entities)
						{
								if (entity.Id == default)
										entity.Id = await redisDb.GenerateNewId<TEntity>();
						}
						await collection.InsertAsync(entities);
				}

				try { await collection.SaveAsync(); }
				catch (Exception ex)
				{
						//collection.DeleteAsync(data);
						Console.WriteLine(ex.Message);
						throw;
				}
		}
}
